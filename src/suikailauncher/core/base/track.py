import traceback

def get_ex_summary(ex:Exception) -> str:
    
    ex_type = ex.__class__.__name__
    
    
    tb_list = traceback.format_exception(type(ex), ex, ex.__traceback__)
    tb_str = "".join(tb_list)  
    
    
    return f"{tb_str}错误类型：{ex_type}"
