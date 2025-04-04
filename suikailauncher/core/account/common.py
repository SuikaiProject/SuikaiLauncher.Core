import jwt
from suikailauncher.core.base import network,track
from suikailauncher.core.base.logger import logger

async def verify_jwt(key:str,algorithms:list = ["RS256"],aud:str = "") -> bool:
    """验证给定的 JWT 是否来自令牌签发方
    
    如果是返回 True，否则返回 False"""
    try:
        decode_jwt = jwt.decode(key,verify=False)
        iss = decode_jwt["iss"]
        jwt_aud = decode_jwt["aud"]
        logger.debug(f"[Security] 令牌签发者为：{iss}")
        if iss:
            logger.debug("[Security] 验证令牌 (1/3):获取 OIDC Server 元数据")
            oidc_server_resp = await network.network_request(iss+"/.well-known/openid-configuration")
            if oidc_server_resp.is_error():
                return False
            result = oidc_server_resp.json()
            issuer = result.get("issuer")
            key_server = result.get("jwks_uri")
            algorithms = result.get("id_token_signing_alg_values_supported",algorithms)
            logger.debug(f"[Security] 当前 OIDC Server 密钥地址:{key_server}")
            logger.debug(f"[Security] 当前 OIDC Server 支持算法:{algorithms}")
            logger.debug("[Security] 验证令牌 (2/3)：获取 OIDC Server 签名公钥")
            try:
                client = jwt.PyJWKClient(key_server)
                sign_key = client.get_signing_key_from_jwt(key)
            except jwt.PyJWKClientError:
                return False
            logger.debug(f"[Security] 当前获取的密钥为：{sign_key.key}")
            logger.debug("[Security] 验证令牌 (3/3)：验证令牌有效性")
            try:
                jwt.decode(jwt=key,key=sign_key.key,algorithms=algorithms,audience=aud,issuer=issuer)
                logger.info("[Security] 验证令牌成功")
                return True
            except jwt.InvalidTokenError as e:
                logger.error("[Security] 验证令牌失败：此令牌无效")
            except jwt.ExpiredSignatureError as e:
                logger.error("[Security] 验证令牌失败：令牌签名已过期")
            except jwt.InvalidAudienceError as e:
                logger.error("[Security] 验证令牌失败：令牌使用者与令牌记录值不一致")
            except jwt.InvalidIssuerError as e:
                logger.error("[Security] 验证令牌失败：令牌签发方与令牌记录值不一致")
            except Exception as e:
                logger.error(f"[Security] 验证令牌出错：{track.get_ex_summary(e)}")
            return False
        return False
    except Exception as e:
        logger.error(f"[Security] 验证令牌出错\n{track.get_ex_summary(e)}")
        return False
    

async def get_jwt_data(_jwt:str,name:str):
    return jwt.decode(_jwt,verify=False)[name]