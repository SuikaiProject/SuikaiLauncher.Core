from suikailauncher.core.base.logger import logger
from enum import Enum
import importlib.resources
# 读取 Mod 数据库，来自龙猫从 MC 百科爬的数据
with importlib.resources.files("suikailauncher.core.resources").joinpath("ModData.txt").open(encoding="utf-8") as f:
    mod_database = f.read()
    
class LoaderType(Enum):
    Any = 0
    Forge = 1
    LiteLoader = 3
    Fabric = 4
    Quilt = 5
    NeoForge = 6
    OptiFine = 7

mod_translate_mapping = {}
mod_translate_index = {}

# 会在 Core 被加载时一并加载
async def load_mod_database():
    global mod_translate_mapping
    logger.info("[Mod] 开始初始化 Mod 数据库")
    
    # 统一换行符处理
    lines = mod_database.replace("\r\n", "\n").replace("\r", "").split("\n")
    wiki_id = 0
    
    for line in lines:
        if not line:
            continue
        wiki_id += 1
        # 可能有多个内容
        entries = line.split("¨")
        for entry in entries:
            if not entry:
                continue
            fields = entry.split("|")
            mod_meta:dict[str,dict[str,str]|int|str] = {}

            # 标记来源            
            if fields[0].startswith("@"):
                mod_meta["source"] = 2
                mod_meta["slug"] = {
                    "modrinth": fields[0].replace("@", "")
                }
            elif fields[0].endswith("@"):
                mod_meta["source"] = 0
                slug_val = fields[0].removesuffix("@")
                mod_meta["slug"] = {
                    "modrinth": slug_val,
                    "curseforge": slug_val
                }
            elif "@" in fields[0]:
                mod_meta["source"] = 0
                parts = fields[0].split("@")
                mod_meta["slug"] = {
                    "curseforge": parts[0],
                    "modrinth": parts[1]
                }
            else:
                mod_meta["source"] = 1
                mod_meta["slug"] = {
                    "curseforge": fields[0]
                }
            
            mod_meta["WikiId"] = wiki_id  

            # 如果存在中文名称
            if len(fields) >= 2:
                chinese_name = fields[1]
                # 如果中文名称中包含 "*"，则进行替换处理
                if "*" in chinese_name:
                    # 决定使用 Modrinth 还是 CurseForge 的 slug
                    slug_val = mod_meta["slug"].get("modrinth") or mod_meta["slug"].get("curseforge", "")
                    raw_name = slug_val.replace("-", " ")
                    chinese_name = chinese_name.replace("*", "")
                    mod_meta["RawName"] = raw_name.capitalize()
                    mod_meta["ChineseName"] = chinese_name
            
                    mod_translate_mapping[wiki_id] = mod_meta
                    mod_translate_index[chinese_name] = wiki_id
        

async def get_mod_i18n_en(mod_name:str):
    pending_search_keyword = []
    for key,value in mod_translate_index.items():
        if mod_name in key:
            pending_search_keyword.append(mod_translate_mapping.get(value).get("RawName"))
    return pending_search_keyword