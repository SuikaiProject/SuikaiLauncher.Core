from suikailauncher.core.base import network
from suikailauncher.core.base.logger import logger

modrinth_api = "https://api.modrinth.com/v2"

modrinth_mirror_api = "https://mod.mcimirror.top/modrinth/v2"

# [["categories:forge"],["versions:1.17.1"],["project_type:mod"]]

async def search_mod(mod_name:str,mod_loader:str = "",version:str = ""):
    search_url = modrinth_api+f"/search?query={mod_name}&limit=1&facets=[[\"project_type:mod\"]"
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

