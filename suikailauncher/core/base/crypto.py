import base64
from suikailauncher.core.account import key
import secrets
from cryptography.hazmat.primitives.ciphers import Cipher, algorithms, modes
from cryptography.hazmat.primitives import padding
from cryptography.hazmat.backends import default_backend

# 获取密钥和 IV
secret_key = key.get_password("MainSecretKey")
secret_iv = key.get_password("MainSecretIV")

#生成密钥和 IV
if not secret_key:
    secret_key = secrets.token_urlsafe(256)
    key.set_password("MainSecretKey",secret_key)
if not secret_iv:
    secret_iv = secrets.token_urlsafe(16)
    key.set_password("MainSecretIV",secret_iv)

# 初始化加密
cipher = Cipher(
    algorithms.AES256(secret_key.encode()),
    modes.CBC(secret_iv.encode()),
    default_backend()
    )
# 获取上下文
encryptor = cipher.encryptor()
decryptor = cipher.decryptor()

async def base64_encode(value:object):
    string = value.encode() + (b"=" * (len(str(value)) % 4))
    return base64.urlsafe_b64encode(string.encode()).decode()

async def base64_decode(value:object):
    string = value.encode() + (b"=" * (len(str(value)) % 4))
    return base64.urlsafe_b64decode(string.encode()).decode()

async def get_ase256_encrypt(content):
    pass

async def get_ase256_decrypt(content):
    pass

async def reset_cipher(key:str = secret_key,iv:str = secret_iv):
    """
    重置加密密钥

    指定 key 与 iv 参数可使用对应密钥
    """
    pass
