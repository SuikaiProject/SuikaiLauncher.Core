from suikailauncher.core.base import network
from suikailauncher.core.mod import database
from suikailauncher.core.base.logger import logger
import asyncio
asyncio.run(database.load_mod_database())

curseforge_api = "https://api.curseforge.com/v1"


curseforge_mirror_api = "https://mod.mcimirror.top/curseforge/v1"

async def search_mod(mod_name:str):
    search_api = f"{curseforge_api}/mods/search?"
    pending_search_keyword = await database.get_mod_i18n_en(mod_name)
    logger.debug(f"[Mod] 工程列表搜索文本：{" ".join(pending_search_keyword)}")



asyncio.run(search_mod("应用能源"))