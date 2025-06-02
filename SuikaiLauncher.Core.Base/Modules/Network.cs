#pragma warning disable SYSLIB0014
using SuikaiLauncher.Core.Override;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace SuikaiLauncher.Core.Base
{
    public class HttpRequestBuilder
    {
        public class SslInfomation
        {
            public required object RequestMessage;
            public required X509Certificate? Cert;
            public required X509Chain? Chain;
            public required SslPolicyErrors SslPolicyError;
        }
        private string? _RequestUri;
        private static readonly HttpProxy RequestProxyFactory = new();
        private string? ConnectAddress;
        private int ConnectPort = 0;
        private static readonly HttpClient Client = new();
        private bool CheckSsl = true;
        public static readonly object HttpRequestBuilderPropertyChangeLock = new object[1];
        private static Func<SslInfomation, bool>? CustomSslValidateCallback;
        private static readonly SocketsHttpHandler Handler = new()
        {
            UseProxy = true,
            Proxy = RequestProxyFactory,
            AllowAutoRedirect = false,
            SslOptions = new SslClientAuthenticationOptions()
            {
                RemoteCertificateValidationCallback = (object httpRequestMessage,X509Certificate? cert,X509Chain? certChain,SslPolicyErrors sslPolicyError) =>
                {
                    if (CustomSslValidateCallback is not null)
                    {
                        return CustomSslValidateCallback(new SslInfomation()
                        {
                            RequestMessage = httpRequestMessage,
                            Cert = cert,
                            Chain = certChain,
                            SslPolicyError = sslPolicyError
                        });
                    }
                    if (sslPolicyError != SslPolicyErrors.None && CheckSsl)
                    {
                        return false;
                    }
                    return true;
                }
            }
        };
        public required string RequestUri
        {
            get {
                if (_RequestUri.IsNullOrWhiteSpaceF()) return string.Empty;
                return _RequestUri;
            }
            set {
                _RequestUri = value;
            }
        }
        /// <summary>
        /// 创建 HttpRequestBuilder 的新实例
        /// </summary>
        /// <param name="url">目标服务器地址</param>
        /// <returns>HttpRequestBuilder</returns>
        public static HttpRequestBuilder Create(string url)
        {
            return new HttpRequestBuilder() { RequestUri = url };
        }
        /// <summary>
        /// 设置默认证书验证函数
        /// </summary>
        /// <param name="CustomCallback">证书验证函数</param>
        public static void SetCustomSslValidateCallback(Func<SslInfomation, bool> CustomCallback)
        {
            lock (HttpRequestBuilderPropertyChangeLock)
            {
                CustomSslValidateCallback = CustomCallback;
            }
        }
        /// <summary>
        /// 设置请求的 IP 地址，用于覆盖默认查询结果
        /// </summary>
        /// <param name="Address">IP 地址</param>
        /// <returns>HttpRequestBuilder</returns>
        public HttpRequestBuilder SetSourceAddress(string Address)
        {
            return this;
        }
        /// <summary>
        /// 设置请求端口，用于覆盖默认端口
        /// </summary>
        /// <param name="Port">端口号</param>
        /// <returns>HttpRequestBuilder</returns>
        public HttpRequestBuilder SetConnectPort(int Port)
        {
            return this
        }
        /// <summary>
        /// 是否在默认验证逻辑中忽略 SSL 证书错误
        /// </summary>
        /// <returns>HttpRequestBuilder</returns>
        public HttpRequestBuilder IgnoreSslError()
        {
            lock (HttpRequestBuilderPropertyChangeLock)
            {
                CheckSsl = true;
            }
            return this;
        }
        /// <summary>
        /// 设置请求使用的代理
        /// </summary>
        /// <param name="Proxy">代理服务器</param>
        /// <returns>HttpRequestBuilder</returns>
        public HttpRequestBuilder UseProxy(string? Proxy = null)
        {
            if (!Proxy.IsNullOrWhiteSpaceF()) {
                lock (RequestProxyFactory.ProxyChangeLock) {
                    RequestProxyFactory.ProxyAddress = Proxy;
                    RequestProxyFactory.RequiredReloadProxyServer = true;
                   }
                }
            return this;

        }
        public HttpRequestBuilder SetHeader() 
        {
            return this;
        }
        public HttpRequestBuilder SetHanders()
        {
            return this;
        }
    }
    
    public class HttpProxy : IWebProxy
    {
        public ICredentials? Credentials { get; set; }
        private IWebProxy SystemProxy = HttpClient.DefaultProxy;
        private WebProxy? CurrentProxy;
        public object ProxyChangeLock = new object[1];
        public bool RequiredReloadProxyServer;
        public bool UseSystemProxy = true;
        public string? ProxyAddress;
        public Uri? GetProxy(Uri RequestHost)
        {
            return GetProxy(RequestHost.AbsoluteUri)?.Address;
        }
        public WebProxy? GetProxy(string Host)
        {
            try
            {
                Logger.Log("Success!");
                WebProxy CurrentSystemProxy = new WebProxy(SystemProxy.GetProxy(new Uri(Host)), true);
                if (CurrentProxy is not null && !RequiredReloadProxyServer) return CurrentProxy;
                if (RequiredReloadProxyServer)
                {
                    Logger.Log("[Network] 已要求刷新代理配置，开始重载代理配置");
                    if (UseSystemProxy && ProxyAddress.IsNullOrWhiteSpaceF())
                    {
                        Logger.Log("[Network] 当前代理配置：跟随系统代理设置");
                        lock (ProxyChangeLock)
                        {
                            CurrentProxy = CurrentSystemProxy;
                            RequiredReloadProxyServer = false;
                        }
                    }
                    else if (!UseSystemProxy && !ProxyAddress.IsNullOrWhiteSpaceF())
                    {
                        Logger.Log("[Network] 当前代理配置：自定义");
                        lock (ProxyChangeLock)
                        {
                            CurrentProxy = new WebProxy(ProxyAddress, true);
                            RequiredReloadProxyServer = false;
                        }
                    }
                    else
                    {
                        // 直接返回
                        Logger.Log("[Network] 当前代理配置：禁用");
                        return null;
                    }
                    return CurrentProxy;
                }
                return null;
            }
            catch (UriFormatException)
            {
                Logger.Log("[Network] 检测到可能错误的配置，已清空自定义代理配置并使用默认值。");
                ProxyAddress = null;
                return CurrentProxy;
            }
        }
        public bool IsBypassed(Uri RequestUri)
        {
            return CurrentProxy?.GetProxy(RequestUri) == RequestUri;
        }
    }
    public class Network
    {
        public static readonly HttpProxy Proxy = new();
        private static readonly HttpClientHandler clientHandler = new HttpClientHandler()
        {
            AllowAutoRedirect = SetupServicePoint(),
            ServerCertificateCustomValidationCallback = ,
            UseProxy = true,
            Proxy = Proxy,
            AutomaticDecompression = DecompressionMethods.All
        };
        private static readonly HttpClient Client = new HttpClient(clientHandler);
        private const string LauncherUA = "SuikaiLauncher.Core/0.0.2";
        private const string BrowserUA = "SuikaiLauncher.Core/0.0.2 Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:137.0) Gecko/20100101 Firefox/137.0";
        public static bool IgnoreSslError = false;
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
        private static bool SetupServicePoint()
        {
            ServicePointManager.DefaultConnectionLimit = int.MaxValue;
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
            {
                if (sslPolicyErrors != System.Net.Security.SslPolicyErrors.None && !IgnoreSslError)
                {
                    return false;
                }
                return true;
            };
            return false;
        }
        public async static Task<WebResponse> Request(
            string url,
            Dictionary<string, string>? Headers = null,
            string Method = "GET",
            byte[]? ReqData = null,
            int timeout = 250000,
            string UserAgent = "",
            string ContentType = "application/json",
            string Accept = "*/*",
            bool useBrowserUA = false,
            int Retry = 5,
            CancellationToken? Token = null
            )
        {
        Retry:
            try
            {
                if (Token is not null) Token.Value.Register(() => throw new TaskCanceledException("操作已取消"));
                HttpWebRequest Request = (HttpWebRequest)HttpWebRequest.Create(url);
                Request.KeepAlive = true;
                if (Headers is not null)
                {
                    foreach (var Header in Headers)
                    {
                        Request.Headers.Add(Header.Key, Header.Value);
                    }
                }
                Request.ProtocolVersion = HttpVersion.Version11;
                Request.Method = Method.ToUpper();
                Request.Timeout = timeout;
                Request.Proxy = Proxy.GetProxy(url);
                if (ReqData is not null)
                {
                    Request.ContentType = ContentType;
                    Request.Accept = Accept;
                    using (Stream ReqStream = Request.GetRequestStream())
                    {
                        await ReqStream.WriteAsync(ReqData, 0, ReqData.Length);
                    }
                    Request.ContentLength = ReqData.Length;
                }
                if (useBrowserUA) Request.UserAgent = BrowserUA;
                else if (!UserAgent.IsNullOrWhiteSpaceF()) Request.UserAgent = UserAgent;
                else Request.UserAgent = LauncherUA;
                return Request.GetResponse();
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.Timeout && Retry > 0)
                {
                    Retry--;
                    goto Retry;
                }
                if (ex.Response is null) throw;
                return ex.Response;
            }
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