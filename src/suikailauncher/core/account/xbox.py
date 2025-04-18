from suikailauncher.core.base import network,exceptions
from suikailauncher.core.base.logger import logger
import json


async def login_xbox_live(access_token:str) -> tuple[str,str]:
    login_data = {
        "Properties": {
            "AuthMethod": "RPS",
            "SiteName": "user.auth.xboxlive.com",
            "RpsTicket": access_token
        },
        "RelyingParty": "http://auth.xboxlive.com",
        "TokenType": "JWT"
    }
    
    user_auth_resp = await network.network_request("https://user.auth.xboxlive.com/user/authenticate","POST",data=json.dumps(login_data))
    if user_auth_resp.is_error():
        raise exceptions.LoginException("远程服务器返回错误",user_auth_resp.decode())
    user_auth_result = user_auth_resp.json()
    token,uhs=user_auth_result.get("Token"),user_auth_result.get("DisplayClaims").get("xui")[0].get("uhs")
    return token,uhs
async def login_xsts(access_token:str) -> tuple[str,str]:
    login_data = {
        "Properties": {
            "SandboxId": "RETAIL",
            "UserTokens": [
                access_token
            ]
        },
        "RelyingParty": "rp://api.minecraftservices.com/",
        "TokenType": "JWT"
    }
    user_auth_resp = await network.network_request("https://xsts.auth.xboxlive.com/xsts/authorize","POST",data=json.dumps(login_data))
    if user_auth_resp.is_error():
        raise exceptions.LoginException("远程服务器返回错误",user_auth_resp.decode())
    user_auth_result = user_auth_resp.json()
    token,uhs = user_auth_result.get("Token"),user_auth_result.get("DisplayClaims").get("xui")[0].get("uhs")
    return token,uhs
