from suikailauncher.core.base.network import network_request,HttpResponse

import pathlib
from suikailauncher.core.base.system import get_file_hash 
async def download(force_mirror:bool = False):
    auth_off = ""
    auth_bmcl = ""
    complete = False
    if force_mirror and download.startswith("https://auth"):
        pass
    resp = await network_request(download)

    if not resp.is_error():
        data = resp.json()
        sha2 = data.get("checksums").get("sha256")
        url = data.get("download_url")
        file = pathlib.Path("authlib-injector/authlib-injector.jar")
        if not file.exists() or get_file_hash(file=file) != sha2:
            # 触发更新
            resp = await network_request(url)
            if not resp.is_error():
                data = resp.get_response()
                with file.open("wb") as f:
                    f.write(data)

    else:
        pass