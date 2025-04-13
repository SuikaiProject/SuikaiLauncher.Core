import json
from suikailauncher.core.base import exceptions

version = "0.0.1"


def get_json_object(content:str) ->dict:
    try:
        return json.loads(content)
    except json.JSONDecodeError as e:
        return exceptions.InvalidJsonException("无效的 Json 结构")
    
def to_json(json_data:dict) ->str:
    return json.dumps(json_data)
