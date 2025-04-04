import aiohttp
import aiohttp.client_exceptions
from suikailauncher.core.base import track,version,get_json_object,exceptions
import time as t
import asyncio
from suikailauncher.core.base.logger import logger


# 连接复用
session = None

# 请求头
global_headers = {
    "User-Agent":f"SuikaiLauncher/{version}",
    "Accept-Language":"zh-CN,zh;q=0.9,en;q=0.8"
}

# 对应状态码
http_redirect = range(300,400)
http_err = range(400,600)
http_ok = range(200,300)
http_internal_err = range(2000,2100)

# HTTP 响应类
class HttpResponse:
    def __init__(self,status:int,headers:dict|None = None,data:bytes|None = None,requested:int|None = None,usage:int|None = None,redirect_history:list|None = None):
        self.status = status
        self.headers = headers
        self.data = data
        self.requested = requested
        self.usage = usage
        self.redirect = redirect_history
    def get_response(self) -> bytes:
        return self.data
    def get_status(self) -> int:
        return self.status
    def get_headers(self) -> dict:
        return self.headers
    def get_description(self) -> str:
        pass
    def get_time(self) -> float:
        return self.usage
    def get_requested(self) -> int:
        return self.requested
    def get_redirect(self,split_str:str = " -> ",return_list:bool = False) -> str|list:
        return split_str.join(self.redirect)
    def is_error(self) -> bool:
        return self.status in http_err or self.status in http_internal_err
    def json(self) -> dict:
        return get_json_object(self.decode("utf-8"))
    def decode(self,encoding:str = "utf-8") ->str:
        return self.data.decode(encoding)
# 万能的网络请求方法
async def network_request(url:str,method:str="GET",headers:dict|None = None,data:str|bytes|None = None,verify_ssl:bool = True,retry:int = 5,use_browser_ua:bool = False,max_redirect:int = 20,use_stream:bool = True,allow_http_err:bool = True):
    global session
    if not session:
        session = aiohttp.ClientSession(loop=asyncio.get_event_loop())
    _break = False
    # 存一下 Url
    request_url = url
    # 请求头
    request_headers = {}
    # 重定向历史和尝试次数
    redirect_history = []
    redirect_history.append(request_url)
    redirect = 0
    retried = 0
    # 添加自定义请求头
    if headers:
        for key,value in headers.items():
            request_headers[key] = value
    else:
        request_headers = global_headers
    # 语言标记
    if not request_headers.get("Accept-Language"):
        request_headers["Accept-Language"] = global_headers.get("Accept-Language")
    # 复写 User-Agent，避免请求被阻止
    if not request_headers.get("User-Agent") or use_browser_ua:
        if use_browser_ua:
            request_headers["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/134.0.0.0 Safari/537.36"
        else:
            request_headers["User-Agent"] = global_headers.get("User-Agent")
    if data:
        if not request_headers.get("Content-Type"):
            request_headers["Content-Type"] = "application/json"
    try:
        # 记录开始时间
        time_start = t.time()
        time_end = 0
        while redirect <= max_redirect:
            # 根据方法判断请求
            async with session.request(method.upper(),request_url,headers=request_headers,data=data,verify_ssl=verify_ssl,allow_redirects=False) as resp:                
                # 尝试获取响应
                try:
                    if resp.status in http_redirect:
                        request_url = resp.headers.get("location")
                        redirect += 1
                        redirect_history.append(request_url)
                        continue
                    else:
                        _break = True
                        if resp.status in http_err and not allow_http_err:
                            raise exceptions.WebException(f"远程服务器返回错误：{resp.status}",resp.status,resp.headers,await resp.read())
                        return HttpResponse(resp.status,resp.headers,await resp.read(),retried,time_end,redirect_history)
                except aiohttp.ClientConnectionResetError as e:
                    logger.error(f"[Network] 发送 HTTP 连接请求时发生错误：连接被重置\n详细详细{track.get_ex_summary(e)}")
                    logger.error(f"[Network] 远程服务器：{url}，重定向记录：{"->".join(redirect_history)}")
                except aiohttp.ClientConnectorCertificateError as e:
                    logger.error(f"[Network] 发送 HTTP 连接请求时发生错误：根据验证结果，目标服务器的 SSL 证书无效\n详细详细{track.get_ex_summary(e)}")
                    logger.error(f"[Network] 远程服务器：{url}，重定向记录：{"->".join(redirect_history)}")
                except aiohttp.ClientConnectorSSLError as e:
                    logger.error(f"[Network] 发送 HTTP 连接请求时发生错误：建立 TLS/SSL 基础连接失败\n详细详细{track.get_ex_summary(e)}")
                    logger.error(f"[Network] 远程服务器：{url}，重定向记录：{"->".join(redirect_history)}")
                except aiohttp.ClientConnectorDNSError as e:
                    logger.error(f"[Network] 发送 HTTP 连接请求时发生错误：未能解析此远程名称\n详细详细{track.get_ex_summary(e)}")
                    logger.error(f"[Network] 远程服务器：{url}，重定向记录：{"->".join(redirect_history)}")
                except asyncio.TimeoutError:
                    logger.error(f"[Network] 发送 HTTP 连接请求时发生错误：连接超时\n详细详细{track.get_ex_summary(e)}")
                    logger.error(f"[Network] 远程服务器：{url}，重定向记录：{"->".join(redirect_history)}")
                except Exception as e:
                    logger.error(f"[Network] 发送 HTTP 连接请求时发生错误：未知错误\n详细详细{track.get_ex_summary(e)}")
                    logger.error(f"[Network] 远程服务器：{url}，重定向记录：{"->".join(redirect_history)}")
                finally:
                    if retried >= retry:
                        break
                    if not _break:
                        retried +=1
                    
        # 古希腊掌管重定向的神（
        if redirect > max_redirect:
            return HttpResponse(2001,resp.headers,await resp.read(),retried,time_end,redirect_history)
    except Exception as e:
        logger.error(f"[Network] 发送 HTTP 请求时出现未知错误\n{track.get_ex_summary(e)}")

async def close():
    try:
        if not session.closed:
            await session.close()
        return True
    except Exception as e:
        logger.error(f"[Network] 关闭连接池时发生错误\n详细信息{track.get_ex_summary(e)}")

