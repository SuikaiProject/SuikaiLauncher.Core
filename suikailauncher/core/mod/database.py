from suikailauncher.core.base.logger import logger
import importlib.resources
# 读取 Mod 数据库，来自龙猫从 MC 百科爬的数据
with importlib.resources.files("suikailauncher.core.resources").joinpath("ModData.txt").open(encoding="utf-8") as f:
    mod_database = f.read()
    
mod_translate_mapping = {}
mod_translate_list = []

# 会在 Core 被加载时一并加载
async def load_mod_database():
    logger.info("[Mod] 开始初始化 Mod 数据库")
    mod_translate_mapping = {}
    
    # 假设 get_resources("ModData") 返回读取到的文本内容，类似 VB 中 GetResources("ModData")
    # 这里为了示例直接使用变量 mod_data_str，实际调用请根据项目环境修改。
    mod_data_str = mod_database
    
    # 统一换行符处理
    lines = mod_data_str.replace("\r\n", "\n").replace("\r", "").split("\n")
    wiki_id = 0
    
    for line in lines:
        if not line:
            continue
        wiki_id += 1
        # 每行内可能有多个条目，使用分隔符 "¨" 分割
        entries = line.split("¨")
        for entry in entries:
            if not entry:
                continue
            fields = entry.split("|")
            mod_meta = {}
            
            # 依据 slug 字段进行判断处理
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
            
            mod_meta["WikiId"] = wiki_id  # 对应 VB 中 Entry.WikiId

            # 如果存在中文名称字段
            if len(fields) >= 2:
                chinese_name = fields[1]
                # 如果中文名称中包含 "*"，则进行替换处理
                if "*" in chinese_name:
                    # 如果存在 modrinth slug，则使用其值否则使用 curseforge slug
                    slug_val = mod_meta["slug"].get("modrinth") or mod_meta["slug"].get("curseforge", "")
                    raw_name = slug_val.replace("-", " ")
                    # 替换 "*" 为 " (RawName)"（RawName 首字母大写）
                    chinese_name = chinese_name.replace("*", f" ({raw_name.capitalize()})")
                mod_meta["ChineseName"] = chinese_name
            
            # 用原始的 slug 字段作为 key 存入映射表，实际情况可根据需求调整 key
            mod_translate_mapping[fields[0]] = mod_meta

    print(mod_translate_mapping)
        

import asyncio

asyncio.run(load_mod_database())
