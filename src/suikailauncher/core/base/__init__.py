import json
from suikailauncher.core.base import exceptions


version = "0.0.1"


def get_json_object(content:str) ->dict:
    try:
        return json.loads(content)
    except json.JSONDecodeError as e:
        raise exceptions.InvalidJsonException("无效的 Json 结构")
    
def to_json(json_data:dict) ->str:
    return json.dumps(json_data)

def sign_secret(url:str = "",data:str = "",headers:dict = None) ->tuple[str,str,dict]:
    if not url and not data and not headers:
        return "","",{}