import pathlib
import sys
from suikailauncher.core.base import network
from suikailauncher.core.base.logger import logger
from suikailauncher.core.base import to_json,system

modrinth_api = "https://api.modrinth.com/v2"

modrinth_mirror_api = "https://mod.mcimirror.top/modrinth/v2"

# [["categories:forge"],["versions:1.17.1"],["project_type:mod"]]

async def search_mod(mod_name:str,mod_loader:str = "",version:str = ""):
    search_url = modrinth_api+f"/search?query={mod_name}&limit=50&facets=[[\"project_type:mod\"]"
    if mod_loader:
        search_url += f"[\"categories:{mod_loader.lower()}\"]"
    if version:
        search_url += f"[\"version:{version}\"]"
    search_url += "]"
    mirror_api = search_url.replace(modrinth_api,modrinth_mirror_api)
    search_result = await network.network_request(search_url)

    
    result = search_result.json()
    mods = result.get("hits")
    if not mods:
        return []
    for mod in mods:
        logger.info("")

async def get_project_meta(project_id_or_slug:str):
    pass

async def get_meta_by_hash(files:list[pathlib.Path]):
    hashes = []
    # 计算文件哈希（SHA 256）
    for file in files:
        hashes.append(system.get_file_or_string_hash("sha256",file=file.absolute()))
    # 构建请求数据
    data = {
        "hashes": hashes,
        "algorithm": "sha256"
    }
    modrinth_resp = await network.network_request(modrinth_api + "/version_files", "POST",{"Content-Type":"application/json"}, data=to_json(data))
    if modrinth_resp.is_error():
        return {}
    return modrinth_resp.json()
    
