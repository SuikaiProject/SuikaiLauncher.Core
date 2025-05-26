using SuikaiLauncher.Core.Base;
using SuikaiLauncher.Core.Override;
using System.Net;

namespace SuikaiLauncher.Core
{
    public class Network
    {
        private static readonly HttpClientHandler clientHandler = new HttpClientHandler() { AllowAutoRedirect = false };
        private static readonly HttpClient Client = new HttpClient(clientHandler);
        private const string LauncherUA = "SuikaiLauncher.Core/0.0.2";
        private const string BrowserUA = "SuikaiLauncher.Core/0.0.2 Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:137.0) Gecko/20100101 Firefox/137.0";

        public async static Task<HttpResponseMessage> NetworkRequest(
            string url,
            Dictionary<string, string>? headers = null,
            string? data = null,
            byte[]? byteData = null,
            int timeout = 10000,
            string method = "GET",
            bool useBrowserUA = false,
            int retry = 5,
            CancellationToken? Token = null)
        {
            int redirectLimit = 20;
            List<string> redirectHistory = new() { url };
            HttpClient httpClient = Client;

            HttpRequestMessage MakeRequest(string reqUrl)
            {
                var req = new HttpRequestMessage(new HttpMethod(method.ToUpper()), reqUrl);
                req.Headers.UserAgent.ParseAdd(useBrowserUA ? BrowserUA : LauncherUA);

                if (headers is not null)
                {
                    foreach (var header in headers)
                    {
                        if (!req.Headers.TryAddWithoutValidation(header.Key, header.Value))
                        {
                            req.Content ??= new StringContent(""); // for content headers fallback
                            req.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                        }
                    }
                }

                if (!data.IsNullOrWhiteSpaceF())
                    req.Content = new StringContent(data);
                else if (byteData is not null)
                    req.Content = new ByteArrayContent(byteData);

                return req;
            }

            string currentUrl = url;
            HttpRequestMessage request = MakeRequest(currentUrl);
            CancellationToken cts;
            if (Token is not null)
            {
                cts = Token.Value;
            }
            else
            {
                cts = new CancellationTokenSource(timeout).Token;
            }
            for (int attempt = 0; attempt <= retry; attempt++)
            {
                try
                {
                    
                        var response = await httpClient.SendAsync(request, cts);

                    int status = (int)response.StatusCode;
                    if (status >= 300 && status < 400 && response.Headers.Location != null)
                    {
                        if (redirectLimit-- <= 0)
                        {
                            Logger.Log($"[Network] 重定向次数过多\n重定向历史：{string.Join("->", redirectHistory)}");
                            throw new TaskCanceledException("重定向次数过多");
                        }
                        currentUrl = response.Headers.Location.IsAbsoluteUri
                            ? response.Headers.Location.ToString()
                            : new Uri(new Uri(currentUrl), response.Headers.Location).ToString();
                        redirectHistory.Add(currentUrl);
                        request.Dispose();
                        request = MakeRequest(currentUrl);
                        response.Dispose();
                        continue;
                    }

                    return response;
                }
                catch (TaskCanceledException ex)
                {
                    Logger.Log(ex, "[Network] 由于远程服务器未能正确处理响应，连接已中止");
                    if (attempt == retry) throw;
                }
                catch (Exception ex)
                {
                    Logger.Log(ex, "[Network] 请求失败");
                    if (attempt == retry) throw;
                }

                // Prepare for retry
                request.Dispose();
                request = MakeRequest(currentUrl);
            }

            throw new TaskCanceledException("发送请求失败");
        }
    }

    public class Download
    {
        internal static SemaphoreSlim MaxDownloadThread = new SemaphoreSlim(64);

        public static int MaxThread
        {
            set
            {
                if (value > 384) throw new ArgumentException("给定的线程数过多");
                var old = MaxDownloadThread;
                MaxDownloadThread = new SemaphoreSlim(value);
                old?.Dispose();
            }
            get => MaxDownloadThread.CurrentCount;
        }

        public static WebProxy? ProxyServer = null;
        public static bool ParallelDownload = true;

        public class FileMetaData
        {
            public string? path { get; set; }
            public string? hash { get; set; }
            public string? algorithm { get; set; }
            public long? size { get; set; }
            public string? url { get; set; }
            public long Start;
            public bool ValidatePathContains(string path)
            {
                if (this.path.IsNullOrWhiteSpaceF() || path.IsNullOrWhiteSpaceF()) return false;
                return Path.GetFullPath(this.path).StartsWith(path);
            }
        }

        public static readonly object FileListLock = new object[1];
        public static long TotalFileCount = 0;
        public static long CompleteFileCount = 0;

        public static async Task NetCopyFileAsync(List<FileMetaData> DlTasks, CancellationToken? Token = null, int MaxThreadCount = 64)
        {
            var token = Token ?? CancellationToken.None;
            SemaphoreSlim semaphore = MaxDownloadThread;

            lock (FileListLock)
            {
                TotalFileCount += DlTasks.Count;
            }

            var tasks = DlTasks.Select(async t =>
            {
                await semaphore.WaitAsync(token);
                try
                {
                    if (t.url is null || t.path is null) Logger.Crash();
                    Logger.Log($"[Network] 直接下载文件：{t.url}");
                    var data = await Network.NetworkRequest(t.url,Token:token);
                    await FileIO.WriteData(data, t.path, token);
                    lock (FileListLock)
                    {
                        CompleteFileCount++;
                    }
                }
                catch (OperationCanceledException ex)
                {
                    Logger.Log(ex, "[Network] 下载已取消");
                }
                catch (Exception ex)
                {
                    Logger.Log(ex, "[Network] 下载文件失败");
                    throw;
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);
        }
    }
}
