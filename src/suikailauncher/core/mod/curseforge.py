import pwd
from suikailauncher.core.base import network
from suikailauncher.core.mod import database
from suikailauncher.core.base.logger import logger
import asyncio
import pathlib
asyncio.run(database.load_mod_database())

curseforge_api = "https://api.curseforge.com/v1"

curseforge_mirror_api = "https://mod.mcimirror.top/curseforge/v1"

async def search_mod(mod_name:str,game_version:str = "",mod_loader:database.LoaderType = database.LoaderType.Any):
    pending_search_keyword = await database.get_mod_i18n_en(mod_name)
    search_api = f"{curseforge_api}/mods/search?gameId=432&sortField=2&sortOrder=desc&pageSize=50"
    if game_version:
        search_api += f"&gameVersion={game_version}"
    logger.debug(f"[Mod] CurseForge 工程列表搜索文本：{" ".join(pending_search_keyword)}")
    logger.debug(f"[Mod] 加载器类型：{mod_loader.name}")
    search_api += f"&modLoaderType={mod_loader.value}"
    search_api += f"&searchFilter={'+'.join(pending_search_keyword)}"
    server_search_resp = await network.network_request(search_api)
    if server_search_resp.is_error():
        logger.error(f"[Mod] 获取 CurseForge 工程列表失败：远程服务器返回错误（{server_search_resp.status}）")
        return []
    server_search_json = server_search_resp.json()
    logger.debug(f"[Mod] 搜索到的工程数量：{server_search_json.get('totalCount')}")
    return server_search_json.get("data")

async def get_mod_version(project_id_or_slug:str,version_id:str = ""):
    pass

async def get_local_mod_info(localfile_or_folder:pathlib.Path):
    pass

asyncio.run(search_mod("应用能源"))