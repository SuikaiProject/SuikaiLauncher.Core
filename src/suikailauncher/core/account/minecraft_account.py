from suikailauncher.core.base import network,exceptions,crypto,get_json_object,to_json

async def login_minecraft(access_token:str,uhs:str) -> str:
    login_data = {
        "identityToken": f"XBL3.0 x={uhs};{access_token}"
    }
    user_auth_resp = await network.network_request("https://api.minecraftservices.com/authentication/login_with_xbox","POST",data=to_json(login_data))
    if user_auth_resp.is_error():
        raise exceptions.LoginException("远程服务器返回错误",user_auth_resp.decode())
    user_auth_result = user_auth_resp.json()
    access_token = user_auth_result.get("access_token")
    return access_token

async def check_onwership(access_token:str) -> bool:
    onwership_resp = await network.network_request("https://api.minecraftservices.com/entitlements/mcstore",headers={"Authorization":f"Beraer {access_token}"})
    return not onwership_resp.json().get("items") == []

async def get_minecraft_profile(access_token:str) -> tuple[str,str]:
    profile_resp = await network.network_request("https://api.minecraftservices.com/minecraft/profile",headers={"Authorization":f"Bearer {access_token}"})
    if profile_resp.is_error():
        raise exceptions.LoginException(f"远程服务器返回错误{profile_resp.get_status()}")
    profile = profile_resp.json()
    name,id=profile.get("name"),profile.get("id")
    return name,id

async def get_skin_url(id:str|None = None,name:str|None = None) ->str|None:
    if not id and not name:
        raise exceptions.InvalidArgumentException("指定的参数无效\n 参数名:id/name")
    if id:
        session_data = await network.network_request(f"https://sessionserver.mojang.com/session/minecraft/profile/{id}")
        if session_data.is_error():
            return ""
        session_json = session_data.json()
        
        for _property in session_json.get("properties"):
            if _property.get("name") == "textures":
                profile = get_json_object(await crypto.base64_decode(_property.get("value")))
                return profile.get("textures").get("SKIN").get("url")
    else:
        profile_resp = await network.network_request(f"https://api.minecraftservices.com/users/profiles/minecraft/{name}")
        if profile_resp.is_error():
            return ""
        profile_json = profile_resp.json()
        return await get_skin_url(profile_json.get("id","")) 
        