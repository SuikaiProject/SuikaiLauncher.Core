from PIL import Image
from suikailauncher.core.base import network,exceptions
from io import BytesIO

async def loadImage(address:str):
    if address.startswith("http:"):
        remote_resp = await network.network_request(address)
        if remote_resp.is_error():
            raise exceptions.InvalidFileException(f"未能从远程地址下载皮肤文件：远程服务器返回错误 {remote_resp.get_status()}")
        return Image.open(BytesIO(remote_resp.get_response()))
    else:
        return Image.open(address)

async def getImageSize(image:Image.Image):
    try:
        return image.size
    except Exception as e:
        raise exceptions.InvalidOperationException("无效的文件操作句柄")
async def coverImage(image:Image.Image,output_format:str = "png"):
    if output_format == "jpg" and image.mode == "RGBA":
        image = image.convert("RGB")
    buffer = BytesIO()
    image.save(buffer,output_format)
    return buffer.getvalue()

async def getImageFromcrop(image:Image.Image,clac:float|None = None,x1:int|None = None,y1:int|None = None,x2:int|None = None,y2:int|None = None):
    pass