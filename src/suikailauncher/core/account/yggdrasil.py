from suikailauncher.core.base import network,exceptions,to_json
from urllib.parse import urlparse



async def login(yggdrasil_api:str,user:str,password:str) -> tuple[str,dict|list] :
    login_data = {
        "username":user,
        "password":password,
        "agent":{
		    "name":"Minecraft",
		    "version":1
	    }
    }
    user_auth_resp = await network.network_request(yggdrasil_api+"/authserver/authenticate","POST",data=to_json(login_data))
    if user_auth_resp.is_error():
        error = result.get("errorMessage",result.get("message"))
        if error:
            raise exceptions.LoginException("登录失败",error)
        raise exceptions.LoginException("登录失败",user_auth_resp.decode())
    result = user_auth_resp.json()
    selectedProfile = result.get("selectedProfile")
    access_token = result.get("access_token")
    availableProfile = result.get("availableProfiles")
    
    if selectedProfile:
        return access_token,selectedProfile
    else:
        return access_token,availableProfile
        
        

async def refresh(yggdrasil_api:str,access_token:str,user:str = "",id:str = "") -> tuple[str,dict]:
    login_data = {
        "accessToken":access_token
    }
    if user and id:
        login_data["selectedProfile"] = {
            "name":user,
            "id":id
        }
    user_auth_resp = await network.network_request(yggdrasil_api + "/authserver/refresh","POST",data=to_json(login_data))
    if user_auth_resp.is_error():
        error = result.get("errorMessage")
        if error:
            raise exceptions.LoginException("登录失败",error)
        raise exceptions.LoginException("登录失败",user_auth_resp.decode())
    result = user_auth_resp.json()
    access_token = result.get("access_token")
    selectedProfile = result.get("selectedProfile",{})
    return access_token,selectedProfile

async def validate(yggdrasil_api:str,access_token:str) -> bool:
    validate_data = {
        "accessToken":access_token
    }
    
    return (await network.network_request(yggdrasil_api+"/authserver/validate","POST",data=to_json(validate_data))).get_status() == 204

async def invalidate(yggdrasil_api:str,access_token:str):
    invalidate_data = {
        "accessToken":access_token
    }

    await network.network_request(yggdrasil_api+"/authserver/invalidate","POST",data=to_json(invalidate_data))

async def signout(yggdrasil_api:str,user:str,password:str):
    signout_data = {
        "username":user,
        "password":password
    }
    return (await network.network_request(yggdrasil_api+"/authserver/signout","POST",data=to_json(signout_data))).get_status() == 204
    

async def get_yggdrasil_root(url:str):
    absolute_url = url
    parse_url = urlparse(url)
    if parse_url.scheme == "":
        absolute_url = "https://" + parse_url.geturl()
    validate_result = await network.network_request(absolute_url,"HEAD")
    yggdrasil_api_location:str = validate_result.get_headers().get("x-authlib-injector-api-location")
    if yggdrasil_api_location:
        if yggdrasil_api_location == "/":
            return "https://" if parse_url.scheme == "" else "" + parse_url.geturl().lstrip("/",1)[0]
        elif yggdrasil_api_location == "./":
            return "https://" if parse_url.scheme == "" else "" + parse_url.geturl()
        elif yggdrasil_api_location.startswith("/") and len(yggdrasil_api_location) > 1:
            return "https://" if parse_url.scheme == "" else "" + parse_url.geturl().lstrip("/",1)[0] + yggdrasil_api_location
        else:
            return "https://" if parse_url.scheme == "" else "" + parse_url.geturl() + yggdrasil_api_location
    

async def upload_skin_or_cape(file:str,is_cape:bool = False):
    pass