from loguru import logger

# TODO:根据账号类型输出文本
enumerate(
    (0,"Legacy"),
    (1,"Yggdrasil"),
    (2,"NideAuth"),
    (4,"Microsoft")
)

# 启动游戏
def launch(version:str,game_folder:str,jre_version:str,account_type:int):
    pass

# 尝试获取 JVM 参数
def get_jvm():
    pass

# 游戏参数
def get_argument():
    pass

# 检查游戏文件
def check():
    pass

# 修复游戏文件
def repair():
    pass

# 去除参数中的重复内容 
def check_duplicate():
    pass