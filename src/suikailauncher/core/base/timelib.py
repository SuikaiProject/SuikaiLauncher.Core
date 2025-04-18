from datetime import datetime,timedelta,timezone
from suikailauncher.core.base.logger import logger

def get_current_time_by_convert(iso_time:str,tzone:int) ->str:
    try:
        dt = datetime.fromisoformat(iso_time)
        tz = timezone(timedelta(hours=tzone))
        dt = dt.astimezone(tz)
        return dt.strftime("%Y-%m-%d %H:%M:%S")
    except Exception as ex:
        logger.error(f"[System] 转换时区失败：{ex}")
        return iso_time.replace("T"," ").rsplit("+",1)[0]

