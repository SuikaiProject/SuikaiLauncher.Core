import pathlib
from suikailauncher.core.base.logger import logger
from suikailauncher.core.base import network,track
class DownloadTask:
    pass

# 异步下载单个文件，不支持流式写入和指定 range
async def download_async(url:str,path:pathlib.Path,UseeAgent:str|None = None,use_browser_ua:bool = False):
    try:
        logger.info(f"[Network] 直接下载文件：{url}")
        with path.open("wb") as f:
            resp = await network.network_request(url,use_browser_ua=use_browser_ua)
            if resp.is_error():
                return False
            f.write(resp.data)
        return True
    except Exception as e:
        logger.error(f"[Network] 下载文件时发生错误：{track.get_ex_summary(e)}")
        return False