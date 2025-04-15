from xxlimited import new
import zipfile
import tarfile
import pathlib
from suikailauncher.core.base import track
from suikailauncher.core.base.logger import logger



class Archive:
    def __init__(self,path:pathlib.Path|str,extract_path:pathlib.Path):
        if isinstance(path,str):
            path = pathlib.Path(path)
        self.f = path
        if isinstance(extract_path,str):
            extract_path = pathlib.Path(extract_path)
        self.extract_folder = extract_path
        self.handler:zipfile.ZipFile|tarfile.TarFile = None
    def __enter__(self):
        self._load_archive_file()
        return self
    def __exit__(self, exc_type, exc_val, exc_tb):
        # 如果 exc_val 为 None，说明没有异常发生
        self.handler.close()
        if exc_val:
            logger.error(f"[Archive] 尝试解压文件失败：{exc_val}\n详细信息：{track.get_ex_summary(exc_val)}")
        # 避免抛出异常
        return True
    def count(self):
        if not self.handler:
            return 0
        try:
            if isinstance(self.handler,zipfile.ZipFile):
                return len(self.handler.infolist())
            return len(self.handler.getmembers())
        except Exception as ex:
            logger.error(f"[Archive] 读取压缩文件失败：{ex}\n详细信息：{track.get_ex_summary(ex)}")
            return 0
    def extract(self,path:pathlib.Path|str):
        if not self.handler:
            return
        if isinstance(self.handler,zipfile.ZipFile):
            new_path = self.extract_folder.joinpath(path)
            new_path.parent.mkdir(parents=True,exist_ok=True)
            with new_path.open("wb") as f:
                f.write(self.handler.read(path))
        else:
            try:
                member = self.handler.getmember(path)
        
    def get_file_list(self):
        if not self.handler:
            return []
        try:
            if isinstance(self.handler,zipfile.ZipFile):
                return self.handler.namelist()
            return [member.name for member in self.handler.getmembers()]
        except Exception as ex:
            logger.error(f"[Archive] 读取压缩文件失败：{ex}\n详细信息：{track.get_ex_summary(ex)}")
            return []
    def _load_archive_file(self,format:str = ""):
        # 根据文件名确定文件格式，或者根据参数（强制指定）
        if self.f.suffix == ".zip" or format == "zip":
            logger.debug(f"[Archive] 文件格式：ZIP")
            try:
                self.handler = zipfile.ZipFile(self.f,"r")
                return
            except zipfile.BadZipFile as ex:
                self.handler = None
                logger.error(f"[Archive] 加载压缩文件失败：{ex}\n详细信息：{track.get_ex_summary(ex)}")
                # 避免循环调用
                if format == "zip":
                    raise ex
                logger.info(f"[Archive] 尝试将 {self.f} 作为 TAR 格式加载")
                self._load_archive_file("tar")
        else:
            logger.debug(f"[Archive] 文件格式：TAR")
            try:
                self.handler = tarfile.TarFile(self.f,"r")
                return
            except tarfile.ReadError as ex:
                self.handler = None
                logger.error(f"[Archive] 加载压缩文件失败：{ex}\n详细信息：{track.get_ex_summary(ex)}")
                # 同理
                if format == "tar":
                    raise ex
                logger.info(f"[Archive] 尝试将 {self.f} 作为 ZIP 格式加载")
                self._load_archive_file("zip")
                
progress = 0.0
with Archive("D:/HugoMoveData/User/seewo/Downloads/Documentation-main.zip","D:/HugoMoveData/User/seewo/Downloads/Documentation-main") as archive:
    files = archive.get_file_list()
    for file in files:
        print(file)
        
