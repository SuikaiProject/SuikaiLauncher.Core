from suikailauncher.core.base import network
from suikailauncher.core.account import common,yggdrasil
from suikailauncher.core.base.logger import logger
import webbrowser
from enum import Enum
import secrets
import asyncio

class YggdtasilConnectLoginResult(Enum):
    LoginSuccess = 0
    ReturnToStandardOAuth = 1
    ReturnToLegacyLogin = 2
    AccessDenied = 3
    ServerError = 4
    NotClientId = 5
    LoginFailed = 6
    ExpiredCode = 7

client_id_mapping:dict[str,str] = {}

async def yggdrasil_get_device_code(yggdrasil_address:str,scope:str = "offline_access Yggdrasil.PlayerProfiles.Select openid Yggdrasil.Server.Join"):
    logger.info("[Account] 开始第三方账号登录（authlib-injector,使用 Yggdrasil Connect 协议）。")
    support_yggdrasil_connect:tuple[bool,str,str] = await check_yggdrasil_meta(yggdrasil_address)
    client_id = support_yggdrasil_connect[2]
    if support_yggdrasil_connect[0]:
        logger.debug("[Account] Yggdrasil Connect 协议预检结果：目标服务器支持此协议。")
        logger.info("[Account] 开始 Yggdrasil Connect 登录步骤 [1/3]：获取登录端点。")
        auth_server_resp = await network.network_request(support_yggdrasil_connect[1])
        if auth_server_resp.is_error():
            logger.error("[Account] Yggdrasil Connect 登录失败：由于目标服务器配置文件或当前网络可能存在问题，未能获取有效配置文件。")
            logger.info("[Account] 由于目标服务器存在问题，将自动回退至标准 OAuth 登录。")
            return YggdtasilConnectLoginResult.ReturnToStandardOAuth
        server_oidc_profile = auth_server_resp.json()
        authorize_endpoint = server_oidc_profile.get("authorization_endpoint")
        device_endpoint = server_oidc_profile.get("device_authorization_endpoint")
        token_endpoint = server_oidc_profile.get("token_endpoint")
        share_client_id = server_oidc_profile.get("shared_client_id")
        logger.debug(f"[Account] 目标服务器授权代码流认证端点：{authorize_endpoint}")
        logger.debug(f"[Account] 目标服务器设备代码流认证端点：{device_endpoint}")
        logger.debug(f"[Account] 目标服务器授权令牌端点：{token_endpoint}")
        if not device_endpoint:
            logger.error("[Account] Yggdrasil Connect 登录失败：目标服务器不支持任何授权端点")
            logger.info("[Account] 由于目标服务器存在问题，将自动回退至传统登录。")
            return YggdtasilConnectLoginResult.ReturnToLegacyLogin
        if not token_endpoint:
            logger.error("[Account] Yggdrasil Connect 登录失败：目标服务器不支持令牌签发端点")
            logger.info("[Account] 由于目标服务器存在问题，将自动回退至传统登录。")
            return YggdtasilConnectLoginResult.ReturnToLegacyLogin
        if not support_yggdrasil_connect[2] and share_client_id:
            client_id = share_client_id
        elif support_yggdrasil_connect[2]:
            client_id = support_yggdrasil_connect[2]
        else:
            return YggdtasilConnectLoginResult.ReturnToLegacyLogin
        logger.info("[Account] 开始 Yggdrasil Connect 登录步骤 [2/3]：获取代码对。")
        _code_challenge = await common.get_code_verifier_and_challenge()
        _code_verifier = _code_challenge[0]
        _code_verifier_urlsafe = _code_challenge[1]
        login_data = f"client_id={client_id}&scope={scope}&code_challenge={_code_challenge}"
        server_resp = await network.network_request(device_endpoint,"POST",{"Content-Type":"application/x-www-form-urlencoded","Accept":"application/json"},login_data)
        if server_resp.is_error():
            return YggdtasilConnectLoginResult.LoginFailed
        server_resp_json = server_resp.json()
        server_resp_json["token_endpoint"] = token_endpoint
        server_resp_json["client_id"] = client_id
        server_resp_json["code_verifier"] = _code_verifier
        return server_resp_json

async def loop_login(codepair:dict[str,str|int]):
    logger.info("[Account] 开始 Yggdrasil Connect 登录步骤 [3/3]：获取轮询登录结果")
    device_code = codepair.get("device_code")
    interval = codepair.get("interval")
    token_authorize = codepair.get("token_endpoint")
    client_id = codepair.get("client_id")
    code_verifier = codepair.get("code_verifier")
    login_data = f"client_id={client_id}&device_code={device_code}&code_verifier={code_verifier}"
    while True:
        await asyncio.sleep(interval)
        server_auth_resp = await network.network_request(token_authorize,"POST",login_data)
        if server_auth_resp.is_error():
            resp_json = server_auth_resp.json()
            match resp_json.get("error"):
                case "authorization_pending":
                    continue
                case "slow_down":
                    continue
                case "expired_token":
                    logger.error("[Account] Yggdrasil Connect 登录失败：设备代码已过期。")
                    return YggdtasilConnectLoginResult.ExpiredCode
                case "access_denied":
                    logger.error("[Account] Yggdrasil Connect 登录失败：用户拒绝了授权请求。")
                    return YggdtasilConnectLoginResult.AccessDenied
                case "server_error":
                    logger.error("[Account] Yggdrasil Connect 登录失败：目标服务器在处理请求时出现错误。")
                    return YggdtasilConnectLoginResult.ServerError
                case "":
                    pass


async def yggdrasil_authorize_with_pkce():
    pass

async def check_yggdrasil_meta(address):
    yggdrasil_api:str = await yggdrasil.get_yggdrasil_root(address)
    server_resp = await network.network_request(yggdrasil_api)
    if server_resp.is_error():
        return False,"",""
    server_resp_json = server_resp.json()
    openid_discovery:str = server_resp_json.get("meta").get("feature.openid_configuration_url")
    site_name = server_resp_json.get("serverName")
    if openid_discovery:
        return True,openid_discovery,client_id_mapping.get(site_name)
    return False,"",""
