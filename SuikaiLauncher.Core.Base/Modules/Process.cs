using SuikaiLauncher.Core.Override;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SuikaiLauncher.Core.Base
{
    public class ProcessBuilder
    {
        private Action<TaskCanceledException>? CrashCallback;
        private Action<TaskCanceledException>? ExitCallback;
        private CancellationTokenSource CTS = new();
        private List<string?> Arguments = [];
        private static readonly string LogPath = Environments.ApplicationDataPath + "SuikaiLauncher/Core/ProcessBuilder/Logs/" + DateTime.Now.ToString("yyyy-MM-dd") + ".log"
        private FileStream OutputStream = new(LogPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read, 8192, true);
        private Process process = new();
        private MemoryStream buffer = new();
        private readonly object StreamLock = new object[1];
        private bool HasStart = false;
        public static ProcessBuilder Create()
        {
            return new ProcessBuilder();
        }
        public ProcessBuilder Executable(string exec)
        {
            this.process.StartInfo.FileName = exec;
            return this;
        }
        public ProcessBuilder RequireEncoding(Encoding? encoding = null)
        {
            this.process.StartInfo.StandardErrorEncoding = this.process.StartInfo.StandardOutputEncoding = encoding ?? Encoding.UTF8;
            return this;
        }
        public ProcessBuilder WithArgument(string Argument)
        {
            this.process.StartInfo.ArgumentList.Add(Argument);
            return this;
        }
        public ProcessBuilder RunAsAdmin()
        {
            this.process.StartInfo.Verb = "runas";
            return this;
        }
        public ProcessBuilder UseShell()
        {
            this.process.StartInfo.UseShellExecute = true;
            return this;
        }
        public ProcessBuilder Invoke()
        {
            if (this.HasStart) throw new InvalidOperationException("不可启动已经启动的 ProcessBuilder 对象");
            this.HasStart = true;
            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data is null) return;
                this.buffer.Write(e.Data.GetBytes());
            };
            process.Start();
            Task.Run(async () =>
            {
                while (!process.HasExited)
                {
                    await Task.Delay(TimeSpan.FromSeconds(10));
                    lock (StreamLock)
                    {
                        Task.Run(async () =>
                        {
                            await this.buffer.CopyToAsync(this.OutputStream);
                        });
                    }
                    await this.OutputStream.FlushAsync();
                    // 清空 Buffer 防止内存爆炸
                    this.buffer.SetLength(0);
                    if (CTS.Token.IsCancellationRequested) throw new TaskCanceledException("进程监控已退出");
                }
                TaskCanceledException TaskEX = new();
                TaskEX.Data["RawOutput"] = LogPath;
                if (process.ExitCode != 0 && this.CrashCallback is not null) this.CrashCallback(TaskEX);
                else if(this.ExitCallback is not null) this.ExitCallback();
            });
            return this;
        }
        public void SetCustomProcessCrashCallback(Action<TaskCanceledException> Callback)
        {
            this.CrashCallback = Callback;
        }
        public void SetCustomProcessExitCallback(Action<TaskCanceledException> Callback)
        {
            this.ExitCallback = Callback;
        }
    }
}
