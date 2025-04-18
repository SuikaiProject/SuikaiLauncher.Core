from json import load
from suikailauncher.core.base.logger import logger
from suikailauncher.core.base import track,network,downloader,launcher_folder,get_json_object,timelib
import time

mojang_version_manifest = "https://piston-meta.mojang.com/mc/game/version_manifest_v2.json"

bmclapi = "bmclapi2.bangbang93.com"

# 用于记录上次更新时间的时间戳，用于判断是否需要更新
latest_update = 0

update_status = False

local_cache = launcher_folder.joinpath("cache","version.json")

manifest = ""

versions = {
    "最新正式版":"",
    "最新快照版":"",
    "正式版":[],
    "快照版":[],
    "远古版":[],
    "愚人节版本":[]
    }

# 加载版本列表，若失败返回 None
async def load_version():
    global versions
    # Mojang 可能会在这期间更新版本列表
    if int(time.time()) - latest_update > 3600 or not manifest:
        logger.info("[Minecraft] 正在尝试更新版本列表")
        await download_version_manifest()
        if not update_status:
            logger.error("[Minecraft] 更新版本列表失败，将继续使用缓存。")
    versions_json = get_json_object(manifest)
    versions["最新正式版"] = versions_json.get("latest").get("release")
    versions["最新快照版"] = versions_json.get("latest").get("snapshot")
    for version in versions_json.get("versions"):
        version["releaseTime"] = timelib.get_current_time_by_convert(version.get("releaseTime"),8)
        match version.get("id"):
            case "20w14infinite", "20w14∞":
                version["type"] = "愚人节版本"
                version["id"] = version.get("id").replace("∞", "infinite")
            case "3d shareware v1.34" | "1.rv-pre1" | "15w14a" | "2.0" | "22w13oneblockatatime" | "23w13a_or_b" | "24w14potato":
                version["type"] = "愚人节版本"
            case _:
                if "/04/01" in version.get("releaseTime"):
                    version["type"] = "愚人节版本"
        match version.get("type"):
            case "release":
                version["type"] = "正式版"
                versions["正式版"].append(version)
            case "snapshot":
                version["type"] = "快照版"
                versions["快照版"].append(version)
            case "old_alpha"|"old_beta":
                version["type"] = "远古版"
                versions["远古版"].append(version)
            case "愚人节版本":
                versions["愚人节版本"].append(version)
            case _:
                version["type"] = "快照版"
                versions["快照版"].append(version)
        
            

async def download_version_manifest(use_mirror:bool = False):
    global latest_update,update_status,manifest
    result = ""
    version_manifest = mojang_version_manifest
    if use_mirror:
        version_manifest = version_manifest.replace("piston-meta.mojang.com",bmclapi)
        latest_update = int(time.time())
        result = await downloader.download_async(version_manifest,local_cache,require_output=True)
        print(result)
        update_status = result != ""
        manifest = result

        return
    result = await downloader.download_async(version_manifest,local_cache,require_output=True)
    latest_update = int(time.time())
    manifest = result
    if not use_mirror and not result:
        # 从镜像下载版本列表
        await download_version_manifest(True)
        return
            
import asyncio

asyncio.run(load_version())

print(versions)

asyncio.run(network.close())