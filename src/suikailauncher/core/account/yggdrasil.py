from suikailauncher.core.base import network,exceptions
import json



async def login(yggdrasil_api:str,user:str,password:str) -> tuple[str,dict|list] :
    login_data = {
        "username":user,
        "password":password,
        "agent":{
		    "name":"Minecraft",
		    "version":1
	    }
    }
    login_str = json.dumps(login_data)
    user_auth_resp = await network.network_request(yggdrasil_api+"/authserver/authenticate","POST",data=login_str)
    result = user_auth_resp.json()
    selectedProfile = result.get("selectedProfile")
    access_token = result.get("access_token")
    availableProfile = result.get("availableProfiles")
    error = result.get("errorMessage",result.get("message"))
    if selectedProfile:
        return access_token,selectedProfile
    elif availableProfile:
        return access_token,availableProfile
    else:
        if error:
            raise exceptions.LoginException(error)
        raise exceptions.LoginException(user_auth_resp.decode())
        

async def refresh(yggdrasil_api:str,access_token:str,user:str = "",id:str = ""):
    login_data = {
        "accessToken":access_token
    }
    if user and id:
        login_data["selectedProfile"] = {
            "name":user,
            "id":id
        }
    
    login_str = json.dumps(login_data)
    user_auth_resp = await network.network_request(yggdrasil_api + "/authserver/refresh")
    result = user_auth_resp.json()
    


async def get_yggdrasil_root():
    pass

