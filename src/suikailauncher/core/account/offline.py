import uuid
from aiohttp import web

def login(user:str) -> str:
    return str(uuid.uuid3(uuid.uuid3(uuid.NAMESPACE_URL,f"OfflinePlayer:{user}")))