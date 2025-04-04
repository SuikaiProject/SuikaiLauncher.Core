import base64

async def base64_encode(value:object):
    string = value.encode() + (b"=" * (len(str(value)) % 4))
    return base64.urlsafe_b64encode(string.encode()).decode()

async def base64_decode(value:object):
    string = value.encode() + (b"=" * (len(str(value)) % 4))
    return base64.urlsafe_b64decode(string.encode()).decode()