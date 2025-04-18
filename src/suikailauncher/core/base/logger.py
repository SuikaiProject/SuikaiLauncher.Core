import logging
from typing import Optional
import sys


class ColoredFormatter(logging.Formatter):
    
    COLOR_CODES = {
        logging.DEBUG:"\033[36m",
        logging.INFO:"\033[32m",
        logging.WARNING:"\033[33m",
        logging.ERROR:"\033[31m",
    }

    RESET_CODE = "\033[0m"

    def __init__(self, fmt: Optional[str] = None, datefmt: Optional[str] = None):
        super().__init__(
            fmt or "%(asctime)s | %(name)s | %(module)s:%(lineno)d | [%(levelname)s] %(message)s",
            datefmt or "%Y-%m-%d %H:%M:%S"
        )

    def format(self, record):
        # 添加颜色
        color = self.COLOR_CODES.get(record.levelno, "")
        message = super().format(record)
        return f"{color}{message}{self.RESET_CODE}"

# 初始化logger
logger = logging.getLogger("SuikaiLauncher.Core")
logger.setLevel(logging.DEBUG)

# 避免重复添加handler
if not logger.handlers:
    # 创建控制台handler
    console_handler = logging.StreamHandler(sys.stdout)
    console_handler.setFormatter(ColoredFormatter())
    logger.addHandler(console_handler)

def set_logger_level(level: str):
    match level.lower():
        case "debug":
            logger.setLevel(logging.DEBUG)
        case "info":
            logger.setLevel(logging.INFO)
        case "warn" | "warning":
            logger.setLevel(logging.WARNING)
        case "error":
            logger.setLevel(logging.ERROR)
        case _:
            return False
    return True
