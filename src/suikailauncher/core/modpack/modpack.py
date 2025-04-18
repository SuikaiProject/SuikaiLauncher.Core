from struct import pack
from suikailauncher.core.base.logger import logger
from suikailauncher.core.base import track,archive,get_json_object
from suikailauncher.core.modpack import mcbbs
import pathlib
from enum import Enum

class ModpackType(Enum):
    MCBBS = 0
    Modrinth = 1
    CurseForge = 2
    HMCL = 3
    MultiMC = 4
    


async def install_modpack_by_local(path:pathlib.Path):
    pack_type = -1
    with archive.Archive(path,path) as af:
        file = af.get_file_list()
        if "mcbbs.packmeta" in file:
            pack_type = ModpackType.MCBBS
        elif "mmc-pack.json" in file:
            pack_type = ModpackType.MultiMC
        elif "modrinth.index.json" in file:
            pack_type = ModpackType.Modrinth
        elif "modpack.json" in file:
            pack_type = ModpackType.HMCL
        else:
            manifest = ""
            for f in file:
                if f == "manifest.json":
                    manifest = af.read(f)
                    break
            manifest_json = get_json_object(manifest)
            if manifest_json.get("addons") is None:
                pack_type = ModpackType.CurseForge
    print(pack_type)

import asyncio
asyncio.run(install_modpack_by_local(pathlib.Path("D:/Me.zip")))