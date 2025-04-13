from ctypes import windll

def get_code_page():
    return windll.kernel32.GetConsoleOutputCP()

