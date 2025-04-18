from venv import logger
from suikailauncher.core.base import archive,to_json,get_json_object,system
import pathlib
import subprocess
export_packmeta = {
    "manifestType":"minecraftModpack",
    "manifestVersion":2,
}

launcher_config = {}


async def install_modpack(file:pathlib.Path|str,install_path:pathlib.Path|str = None):
    if isinstance(file,str):
        file = pathlib.Path(file)
    if isinstance(install_path,str):
        install_path = pathlib.Path(install_path)
    logger.info("[Modpack] 开始安装整合包")
    logger.info("[Modpack] 整合包类型：MCBBS")
    with archive.Archive(file,extract_path=install_path) as af:
        packmeta = af.read("mcbbs.packmeta")
        data = get_json_object(packmeta)
        files_list = sorted(af.get_file_list())
        logger.info("[Modpack] ========== 整合包元数据 ==========")
        logger.info("[Modpack] 整合包名称：%s",data.get("name"))
        logger.info("[Modpack] 整合包作者：%s",data.get("author"))
        logger.info("[Modpack] 整合包版本：%s",data.get("version"))
        logger.info("[Modpack] 整合包描述：%s",data.get("description"))
        logger.info("[Modpack] 整合包下载地址：%s",data.get("url"))
        logger.info(f"[Modpack] 由 SuikaiLauncher 导出的整合包：{True if "suikailauncher" in files_list else False}")
        for addon in data.get("Addons"):
            if addon.get("id") == "game":
                logger.info("[Modpack] 游戏版本：%s",addon.get("version"))
            else:
                logger.info(f"[Modpack] {str(addon.get("id")).capitalize()} 版本：%s",addon.get("version"))
        logger.info("[Modpack] ========== 整合包元数据 ==========")
        lgger.info("[Modpack] 开始安装整合包")
        for file,data in files_list,data.get("files"):
            fp = install_path.joinpath(file.replace("/overrides",""))
            if file == "mcbbs.packmeta" or file == "manifest.json":
                continue
            af.extract(file,fp)
            if system.get_file_or_string_hash(file=fp) != data.get("hash"):
                logger.error("[Modpack] 安装失败：文件 %s 校验失败",file)
                return False
                

async def export_modpack(input_folder:str|pathlib.Path,output_folder:str|pathlib.Path,game_version:str,name:str = "",author:str = "",version:str = "",addons:list[dict] = [],description:str = "",config_str:str = "",update_url:str = "",**require):
    resources = []
    if isinstance(input_folder,str):
        input_folder = pathlib.Path(input_folder)
    if isinstance(output_folder,str):
        output_folder = pathlib.Path(output_folder)
    export_temp = export_packmeta
    export_temp["name"] = name
    export_temp["author"] = author
    export_temp["version"] = version
    export_temp["description"] = description
    export_temp["addons"] = [
            {
                "id":"game",
                "version":game_version,
            }
        ]
    if addons:
        for addon in addons:
            export_temp["addons"].append({
                "id":addon.get("id"),
                "version":addon.get("version")
            })
    # TODO: 通过配置文件筛选需要导出的文件
    if config_str:
        pass
    # 启动器配置文件
    if require:
        launcher_config["authmethod"] = require.get("auth")
        launcher_config["authserver"] = require.get("yggdrasil")
        launcher_config["require"] = {
            "java":{
                "min":require_java.get("minVersion"),
                "max":require_java.get("maxVersion")
            }
        }
        for res in input_folder.iterdir():
            # 不能列入资源文件的内容
            if res.is_file() and (
                # 1. Mojang 官方有版权的文件，如游戏核心和版本 json
                input_folder.name not in res.name 
                # 2. 启动器在文件夹内生成的文件
                # 3. 日志等无用文件
                or "logs" not in res.parts
                or "suikailauncher" not in res.name
                # ....或者 Mod 生成的包含账号敏感信息的文件 (https://github.com/Hex-Dragon/PCL2/issues/5694)
                or "account" not in res.name
                or "token" not in res.name
                or "login" not in res.name
                or "session" not in res.name
                ):

                resources.append(res.absolute().replace(input_folder.absolute(),"/"))
                

    with archive.Archive(output_folder.joinpath(name)) as af:
        af.delta_file("./mcbbs.packmeta",text=to_json(export_temp))
        af.delta_file("./overrides/suikailauncher/config.json",text=to_json(launcher_config))
        for resource in res:
            af.delta_file(f"./overrides/{resource}",path=input_folder.joinpath(resource))
    # 拉起资源管理器并选中文件
    subprocess.run(executable="explorer.exe",args=f"/select,{output_folder.joinpath(name)}")
    