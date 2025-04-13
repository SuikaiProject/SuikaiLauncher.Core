# 存储密码
import keyring

def set_password(user:str,password:str):
    keyring.set_password("SuikaiLauncher.Core",user,password)

def get_password(user:str):
    return keyring.get_password("SuikaiLauncher.Core",user)

def delete_password(user:str):
    keyring.delete_password("SuikaiLauncher.Core",user)