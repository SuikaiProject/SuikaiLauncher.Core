from suikailauncher.core.base.logger import logger
from suikailauncher.core.base import track,network,downloader

mojang_version_manifest = "https://piston-meta.mojang.com/mc/game/version_manifest_v2.json"

# 用于记录上次更新时间的时间戳，用于判断是否需要更新
latest_update = None

# 加载本地版本列表，若失败返回 None
async def load_version(use_mirror:bool = False):
    pass

async def download_version_manifest(use_mirror:bool = False):
    global latest_update
    result = await downloader.download_async(mojang_version_manifest)