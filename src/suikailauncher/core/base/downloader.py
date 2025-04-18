import pathlib
from suikailauncher.core.base.logger import logger
from suikailauncher.core.base import network,track
class DownloadTask:
    pass

# 异步下载单个文件，不支持流式写入和指定 range
async def download_async(url:str,path:pathlib.Path,use_browser_ua:bool = False,require_output:bool = False):
    try:
        logger.info(f"[Network] 直接下载文件：{url}")
        resp = await network.network_request(url,use_browser_ua=use_browser_ua)
        if require_output:
            if resp.is_error():
                return ""
            return resp.decode()
        with path.open("wb") as f:
            f.write(resp.data)
        return True
    except Exception as ex:
        logger.error(f"[Network] 下载文件时发生错误：{track.get_ex_summary(ex)}")
        return False