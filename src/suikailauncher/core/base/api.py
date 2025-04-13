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
def get_file_or_string_hash(Algorithm:str="sha1",string:str|None = None,file:str|pathlib.Path|None = None,return_raw_bytes:bool = False):
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
            if return_raw_bytes:
                return hashlib.sha1(data).digest()
            return hashlib.sha1(data).hexdigest()
        case "sha2" | "sha256":
            if return_raw_bytes:
                return hashlib.sha1(data).digest()
            return hashlib.sha256(data).hexdigest()
        case "md5":
            if return_raw_bytes:
                return hashlib.sha1(data).digest()
            return hashlib.md5(data).hexdigest()
        case "sha5"|"sha512":
            if return_raw_bytes:
                return hashlib.sha1(data).digest()
            return hashlib.sha512(data).hexdigest()
        case _:
            return "Unknown Algorithm"