from suikailauncher.core.base import network,exceptions,track
from suikailauncher.core.base.logger import logger
from suikailauncher.core.account import key,common
from suikailauncher.core.base.secret import ms_oauth_id
from msal import PublicClientApplication 
import keyring
import webbrowser
import json
import asyncio

async def mslogin_perferlib():
    pass

# OAuth 设备代码流登录
async def mslogin_device() -> str:
    logger.info("[Login] 开始 Microsoft 账户登录（设备代码流）")
    logger.info("[Login] 登录步骤 1/2：获取授权代码对")
    login_data_url_encoded = f"client_id={ms_oauth_id}&scope=XboxLive.signin offline_access openid profile"
    server_auth_resp = await network.network_request("https://login.microsoftonline.com/consumers/oauth2/v2.0/devicecode","POST",{"Content-Type":"application/x-www-form-urlencoded"},login_data_url_encoded)
    if server_auth_resp.is_error():
        raise exceptions.LoginException(f"登录失败\n远程服务器返回错误：{server_auth_resp.decode()}")
    return server_auth_resp.decode()

# 发送一次登录请求，获取授权状态
async def mslogin_send_login_request(code:str):
    logger.info("[Login] 登录步骤 2/2：获取访问令牌")
    login_data_urlencoded = f"grant_type=authorization_code&client_id={ms_oauth_id}&code={code}"
    server_auth_resp = await network.network_request("https://login.microsoftonline.com/consumers/oauth2/v2.0/token","POST",{"Content-Type":"application/x-www-form-urlencoded"},login_data_urlencoded)
    if server_auth_resp.is_error():
        raise exceptions.LoginException("登陆失败",server_auth_resp.decode())
    result = server_auth_resp.json()
    
async def mslogin_send_refresh_login_request(refresh_token:str=""):
    logger.info("[Login] 登录步骤 1/1：刷新访问令牌")

async def mslogin_loop_send_login_request(codepair:str = ""):
    
    logger.info("[Login] 登录步骤 2/2：获取访问令牌")
    try:
        _codepair:dict = json.loads(codepair)
    except json.JSONDecodeError as e:
        logger.error(f"[Login] 登录失败:{track.get_ex_summary(e)}")
        raise exceptions.LoginException("无效的登录数据")
    device = _codepair.get("device_code")
    interval = _codepair.get("interval")
    login_data_urlencoded = f"grant_type=urn:ietf:params:oauth:grant-type:device_code&client_id={ms_oauth_id}&device_code={device}"
    while True:
        await asyncio.sleep(interval)
        server_auth_resp = await network.network_request("https://login.microsoftonline.com/consumers/oauth2/v2.0/token","POST",{"Content-Type":"application/x-www-form-urlencoded"},login_data_urlencoded)
        server_auth_result = server_auth_resp.json()
        if server_auth_resp.is_error():
            match server_auth_result.get("error").lower():
                case "authorization_pending":
                    continue
                case "slow_down":
                    interval += interval
                    continue
                case "authorization_declined":
                    raise exceptions.LoginException("登陆失败：用户拒绝授权",server_auth_resp.decode())
                case "expired_token":
                    raise exceptions.LoginException("登陆失败：登录超时")
                case _:
                    raise exceptions.LoginException("登陆失败",server_auth_resp.decode())
        access_token = server_auth_result.get("access_token")
        refresh_token = server_auth_result.get("refresh_token")
        id_token = server_auth_result.get("id_token")
        verify_result = await common.verify_jwt(id_token)
        if verify_result:
            oid = common.get_jwt_data(id_token,"oid")
            keyring.set_password("SuikaiLauncher",oid,refresh_token)
            return access_token,""
        else:
            logger.error("[Login] 验证 ID 令牌失败：根据验证结果，此令牌无效。")
            # 也有可能是非验证部分炸了（例如没法连接到 OIDC Server），但是令牌本身有效
            # 但是这种情况下没法确定 ID 令牌是否有效，所以应当丢弃
            # 返回两个 Token 的内容，并在 Xbox 登录时保存下来
            return access_token,refresh_token
async def mslogin_auth_code():
    logger.info("[Login] 开始 Microsoft 账户登录（授权代码流）")
    logger.info("[Login] 正在打开授权页面")
    url = f"https://login.microsoftonline.com/consumers/oauth2/v2.0/authorize?client_id={ms_oauth_id}&response_type=code&scope=XboxLive.signin%20offline_access%20openid%20profile&redirect_uri=http://localhost:9900/api.account.msa.oauth.login"
    webbrowser.open(url)

async def mslogin_wam_auth_code():
    app = PublicClientApplication(
    ms_oauth_id,
    authority="https://login.microsoftonline.com/consumers",
    enable_broker_on_windows=True)

    result = app.acquire_token_interactive(["XboxLive.Signin","offline_access"],
            parent_window_handle=app.CONSOLE_WINDOW_HANDLE)