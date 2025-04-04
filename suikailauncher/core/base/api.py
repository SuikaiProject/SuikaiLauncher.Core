import shutil
import pathlib
import hashlib
import keyring

def delete(path:str) -> bool:
    try:
        _path = pathlib.Path(path)
        if _path.is_dir():
            shutil.rmtree(path)
        else:
            _path.unlink(missing_ok=True)
        return True
    except Exception as e: 
        return False
def get_file_hash(Algorithm:str="sha1",string:str|None = None,file:str|pathlib.Path|None = None):
    data = ""
    if not data and not file:
        return ""
    elif string:
        data = string.encode("utf-8")
    else:
        with pathlib.Path(file).open("rb") as f:
            data = f.read()
    match Algorithm.lower():
        case "sha1":
            return hashlib.sha1(data).hexdigest()
        case "sha2" | "sha256":
            return hashlib.sha256(data).hexdigest()
        case "md5":
            return hashlib.md5(data).hexdigest()
        case "sha5"|"sha512":
            return hashlib.sha512(data).hexdigest()
        case _:
            return "Unknown Algorithm"