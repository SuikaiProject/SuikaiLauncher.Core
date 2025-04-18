from suikailauncher.core.base import archive,to_json
import pathlib
import subprocess

async def export_modpack_modrinth(input_folder:str|pathlib.Path,output_folder:str|pathlib.Path,game_version:str,name:str = "",author:str = "",version:str = "",addons:list[dict] = [],description:str = "",config_str:str = "",update_url:str = "",**require):
    resources = []
        #if isinstance(input_folder,str):