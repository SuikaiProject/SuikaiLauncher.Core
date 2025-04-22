using System.Threading;
using System.Text;
using System.Security;
using System.Runtime.CompilerServices;

public class Logger
{
    private static string LogFile = $"{DateTime.Now.ToString("yyyy-MM-dd")}.log";
    private readonly static object LogOutputLock = new object[1];
    private static FileStream LogStream = File.OpenWrite(LogFile);
    private readonly static List<String> LogText = new List<String>();
    private static Thread? LogThread;
    
    public static bool DebugMode = false;

    public enum LogLevel
    {
        Normal = 0,
        Debug = 1,
        Exception = 2,
        Msgbox = 3,
        Exit = 4
    }
    public static string getCurrentTime()
    {
        return DateTime.Now.ToString("HH:mm:ss.FFF");
    }
    public static void StartLogWatcher()
    {
        LogThread = new Thread(new ThreadStart(LogFlush));
        LogThread.Start();

    }
    public static void LogFlush()
    {
        while (true)
        {
            try
            {
                Thread.Sleep(1000);
                if (LogText.Count() <=0) continue;
                lock (LogOutputLock)
                {
                    foreach (string Text in LogText)
                    {
                        LogStream.Write(Encoding.UTF8.GetBytes($"{Text}\n"));
                    }
                    LogStream.Flush();
                    // 清空日志
                    LogText.Clear();
                }
            }
            catch (ThreadInterruptedException)
            {
                return;
            }
            catch (Exception ex)
            {
                if (ex is SecurityException)
                {
                    Console.WriteLine("错误：无法写入日志文件");
                }
            }
        }
    }
    public static void ExitLog()
    {
        if (LogThread is not null && LogThread.IsAlive) LogThread.Interrupt();
        if (LogStream is not null) LogStream.Dispose();
    }
    public static void Log(string message)
    {
        lock (LogOutputLock)
        {
            string output = $"[{getCurrentTime()}] | {message}";
            Console.WriteLine(output);
            LogText.Add(output);
        }
    }
    public static void Log(Exception ex, string message)
    {
        lock (LogOutputLock)
        {
            string output = $"[{getCurrentTime()}] " + GetExceptionDetail(ex, true);
            if (LogThread is null) StartLogWatcher();
            Console.WriteLine(output);
            LogText.Add(output);
        }
    }

    public static void ChangeDebugMode(){
        lock(LogOutputLock){
            DebugMode = !DebugMode;
            if(DebugMode) Log("[Logger] 已进入调试模式，这可能会导致性能下降，如无必要请勿开启！");
        }
    }

    private static string GetExceptionDetail(Exception? ex, bool showAllStacks = false)
    {
        if (ex == null)
        {
            return "无可用错误信息！";
        }


        Exception innerEx = ex;
        while (innerEx.InnerException is not null)
        {
            innerEx = innerEx.InnerException;
        }

        List<string> descList = new List<string>();
        bool isInner = false;
        while (ex != null)
        {
            descList.Add((isInner ? "→ " : "") + ex.Message.Replace("\n", "\r").Replace("\r\r", "\r").Replace("\r", Environment.NewLine));
            if (ex.StackTrace != null)
            {
                foreach (string stack in ex.StackTrace.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (showAllStacks || stack.Contains("SuikaiLauncher", StringComparison.OrdinalIgnoreCase))
                    {
                        descList.Add(stack.Replace("\r", string.Empty).Replace("\n", string.Empty));
                    }
                }
            }
            if (ex.GetType().FullName != "System.Exception")
            {
                descList.Add("   错误类型：" + ex.GetType().FullName); // Error Type:
            }
            ex = ex.InnerException;
            isInner = true;
        }


        string? commonReason = null;
        if (innerEx is TypeLoadException || innerEx is BadImageFormatException || innerEx is MissingMethodException || innerEx is NotImplementedException || innerEx is TypeInitializationException)
        {
            commonReason = "当前运行环境存在问题。请尝试重新安装 .NET 6.0 然后再试。若无法安装，请先卸载当前安装的 .NET 6.0，然后再尝试安装。";
        }
        else if (innerEx is UnauthorizedAccessException)
        {
            commonReason = "权限不足";
        }
        else if (innerEx is OutOfMemoryException)
        {
            commonReason = "你的电脑运行内存不足，导致 SuikaiLauncher.Core 无法继续运行。请在关闭一部分不需要的程序后再试。";
        }
        else if (innerEx is System.Runtime.InteropServices.COMException)
        {
            commonReason = "由于操作系统或显卡存在问题，导致出现错误。请尝试重启启动器。";
        }
        else if (new[] { "远程主机强迫关闭了", "远程方已关闭传输流", "未能解析此远程名称", "由于目标计算机积极拒绝",
                         "操作已超时", "操作超时", "服务器超时", "连接超时" }.Any(s => descList.Any(l => l.Contains(s))))
        {
            commonReason = "你的网络环境不佳，导致难以连接到服务器。请检查网络，多重试几次，或尝试使用 VPN。";
        }

        if (commonReason == null)
        {
            return string.Join(Environment.NewLine, descList);
        }
        else
        {
            return commonReason + Environment.NewLine + Environment.NewLine + "————————————" + Environment.NewLine + "详细错误信息：" + Environment.NewLine + string.Join(Environment.NewLine, descList);
        }
    }


}
