class LoginException(Exception):
    def __init__(self,message:str,raw_resp:str):
        self.message = message
        self.raw_resp = raw_resp
        super().__init__(f"{self.message}:+\n{self.raw_resp}")
    def __str__(self):
        return f"{self.message}:+\n{self.raw_resp}"
    def get_raw_resp(self):
        return self.raw_resp
    
class InvalidOperationException(Exception):
    def __init__(self,message:str):
        self.message = message
        super().__init__(self.message)
    def __str__(self):
        return self.message
    
class InvalidArgumentException(Exception):
    def __init__(self,message:str):
        self.message = message
        super().__init__(self.message)
    def __str__(self):
        return self.message
    
class InvalidJsonException(Exception):
    def __init__(self,message:str):
        self.message = message
        super().__init__(self.message)
    def __str__(self):
        return self.message
    
class WebException(Exception):
    def __init__(self,message:str,status:int,headers:dict,resp:bytes):
        self.message = message
        self.status = status
        self.headers = headers
        self.resp = resp
        super().__init__(self.message)
    def __str__(self):
        return self.message
    def get_status(self):
        return self.status
    def get_headers(self,value:str|None = None):
        if value:
            return self.headers.get(value)
        return self.headers

class InvalidFileException(Exception):
    def __init__(self,message:str):
        self.message = message
        super().__init__(self.message)
    def __str__(self):
        return self.message