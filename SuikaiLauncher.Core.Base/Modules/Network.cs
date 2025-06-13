#pragma warning disable SYSLIB0014
using SuikaiLauncher.Core.Base;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using SuikaiLauncher.Core.Override;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;

// 这里是一堆和网络有关系的工具，包括网络请求，代理，Ping，域名解析
// 虽然看起来很乱然而我没空整理，就先这样吧（逃

namespace SuikaiLauncher.Core.Base
{   
    public class SocketConnect
    {
        public required Socket Socket;
    }
    public class HttpRequestBuilder
    {
        public static readonly Dictionary<string, SocketConnect> ConnectionPool = new();
        public static readonly object ConnectionLock = new object();
        public class SslInfomation
        {
            public required object RequestMessage;
            public required X509Certificate? Cert;
            public required X509Chain? Chain;
            public required SslPolicyErrors SslPolicyError;
        }
        private int timeout;
        public required HttpRequestMessage Req;
        public HttpResponseMessage? Resp;
        private static readonly HttpProxy RequestProxyFactory = new();
        private string? ConnectAddress;
        private int ConnectPort = 0;
        private static bool CheckSsl = true;
        public static readonly object HttpRequestBuilderPropertyChangeLock = new object();
        private static Func<SslInfomation, bool>? CustomSslValidateCallback;
        private static readonly SocketsHttpHandler SocketHandler = new()
        {
            UseProxy = true,
            Proxy = RequestProxyFactory,
            AllowAutoRedirect = false,
            AutomaticDecompression = DecompressionMethods.All,
            SslOptions = new SslClientAuthenticationOptions()
            {
                RemoteCertificateValidationCallback = (object httpRequestMessage, X509Certificate? cert, X509Chain? certChain, SslPolicyErrors sslPolicyError) =>
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
            },
            // 长连接实现
            ConnectCallback = async (SocketsHttpConnectionContext context, CancellationToken token) =>
            {
                var host = context.DnsEndPoint.Host;
                var port = context.DnsEndPoint.Port;
                SocketConnect? socketConnect = null;
                lock (ConnectionLock)
                {
                    if (ConnectionPool.TryGetValue($"{host}:{port}", out socketConnect))
                    {
                        if (socketConnect.Socket != null && socketConnect.Socket.Connected)
                        {
                            // 检查 socket 是否可写
                            try
                            {
                                bool poll = socketConnect.Socket.Poll(0, SelectMode.SelectWrite);
                                if (poll)
                                {
                                    // 返回现有连接
                                    return new NetworkStream(socketConnect.Socket, ownsSocket: false);
                                }
                            }
                            catch
                            {
                                // 连接失效，移除
                                ConnectionPool.Remove($"{host}:{port}");
                                socketConnect = null;
                            }
                        }
                        else
                        {
                            ConnectionPool.Remove($"{host}:{port}");
                            socketConnect = null;
                        }
                    }
                }

                // 新建连接
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    NoDelay = true
                };
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                await socket.ConnectAsync(host, port, token);

                lock (ConnectionLock)
                {
                    ConnectionPool[$"{host}:{port}"] = new SocketConnect() { Socket = socket };
                }

                return new NetworkStream(socket, ownsSocket: false);
            }
        };
        private static readonly HttpClient Client = new(SocketHandler);

        /// <summary>
        /// 创建 HttpRequestBuilder 的新实例
        /// </summary>
        /// <param name="url">目标服务器地址</param>
        /// <returns>HttpRequestBuilder</returns>
        public static HttpRequestBuilder Create(string url, HttpMethod method)
        {
            return new HttpRequestBuilder() { Req = new HttpRequestMessage(method,url) };
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
            return this;
        }
        /// <summary>
        /// 是否在默认验证逻辑中忽略 SSL 证书错误
        /// </summary>
        /// <returns>HttpRequestBuilder</returns>
        public HttpRequestBuilder IgnoreSslError()
        {
            if (!CheckSsl) return this;
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
            if (!string.IsNullOrWhiteSpace(Proxy))
            {
                lock (RequestProxyFactory.ProxyChangeLock)
                {
                    RequestProxyFactory.ProxyAddress = Proxy;
                    RequestProxyFactory.RequiredReloadProxyServer = true;
                }
            }
            return this;

        }
        /// <summary>
        /// 设置标头
        /// </summary>
        /// <param name="Name">标头</param>
        /// <param name="Value">值</param>
        /// <returns>HttpRequestBuilder</returns>
        public HttpRequestBuilder SetHeader(string Name, string Value)
        {
            if (Name.ContainsF("Content") && Req.Content is not null) Req.Content.Headers.Add(Name, Value);
            Req.Headers.Add(Name, Value);
            return this;
        }
        /// <summary>
        /// 发送网络请求并自动处理重定向
        /// </summary>
        /// <returns></returns>
        public async Task<HttpRequestBuilder> Invoke()
        {
            await (await this.SendRequest()).ResolveHttpRedirect();
            return this;
        }
        /// <summary>
        /// 获取响应
        /// </summary>
        /// <returns>HttpResponseMessage</returns>
        public HttpResponseMessage? GetResponse()
        {
            return this.Resp;
        }
        /// <summary>
        /// 发送单次网络请求，不处理重定向
        /// </summary>
        /// <returns>HttpRequestBuilder</returns>
        public async Task<HttpRequestBuilder> SendRequest()
        {
            using (CancellationTokenSource CTS = new(this.timeout))
            {
                this.Resp = await Client.SendAsync(this.Req,HttpCompletionOption.ResponseHeadersRead,CTS.Token);
                return this;
            }
        }
        /// <summary>
        /// 处理网络请求的重定向，直到响应码不处于 300~399 范围内（不包括 304）
        /// </summary>
        /// <returns></returns>
        public async Task<HttpRequestBuilder> ResolveHttpRedirect()
        {
            
            if (this.Resp!.StatusCode is > (HttpStatusCode)300 and < (HttpStatusCode)400 && this.Resp.StatusCode != (HttpStatusCode)304)
            {
                HttpRequestMessage RedirectReq = new();
                foreach(var Header in this.Req.Headers)
                {
                    Req.Headers.Add(Header.Key, Header.Value);
                }
                if (this.Req.Content is not null)
                {
                    MemoryStream ReqStream = new();
                    await (await this.Req.Content.ReadAsStreamAsync()).CopyToAsync(ReqStream);
                    if (ReqStream is null) goto SkipContent;
                    RedirectReq.Content = new ByteArrayContent(ReqStream.ToArray());
                    ReqStream.Dispose();
                    foreach (var Header in this.Req.Content.Headers)
                    {
                        RedirectReq.Content.Headers.Add(Header.Key, Header.Value);
                    }
                }
            SkipContent:
                RedirectReq.RequestUri = this.Req.Headers.GetValues("location").First().ToURI();
                this.Req.Dispose();
                this.Resp.Dispose();
                this.Req = RedirectReq;
                await this.Invoke();
            }
            return this;
        }
    }
    public class Localhost {
        public class PingResult
        {
            public object? CustomResult;
            public List<PingReply> Result { get { throw new InvalidOperationException("无法读取此属性"); } set
                {
                    TotalSend = value.Count;
                    value.Select<PingReply, PingReply?>(k =>
                    {
                        switch (k.Status)
                        {
                            case IPStatus.Success:
                                if (Fastest == -1) Fastest = k.RoundtripTime;
                                else if (Slowest == -1) Slowest = k.RoundtripTime;
                                else if (Slowest < k.RoundtripTime) Slowest = k.RoundtripTime;
                                else if (Fastest > k.RoundtripTime) Fastest = k.RoundtripTime;
                                TotalUsage += k.RoundtripTime;
                                return null;
                            case IPStatus.TimedOut:
                                Failed++;
                                Logger.Log($"[Network] Ping {k.Address} 失败：请求超时");
                                return null;
                            case IPStatus.DestinationHostUnreachable:
                                Failed++;
                                Logger.Log($"[Network] Ping {k.Address} 失败：此远程地址不可达");
                                return null;
                            case IPStatus.DestinationNetworkUnreachable:
                                Failed++;
                                Logger.Log($"[Network] Ping {k.Address} 失败：此远程地址所处的网络不可达");
                                return null;
                            case IPStatus.DestinationPortUnreachable:
                                Failed++;
                                Logger.Log($"[Network] Ping {k.Address} 失败：远程地址所指定的端口不可达");
                                return null;
                            case IPStatus.NoResources:
                                Failed++;
                                Logger.Log($"[Network] Ping {k.Address} 失败：网络资源不足");
                                return null;
                            
                        }
                        if (k.Status == IPStatus.Success) Success++;
                        else
                        {
                            if (k.Status == IPStatus.TimedOut)
                                Failed++;
                            return null;
                        }
                        return null;
                    });
                    Average = TotalUsage / TotalSend;
                }
            }
            public long Fastest = -1;
            public long Slowest = -1;
            public long Average = -1;
            public long TotalUsage = -1;
            public long Success = 0;
            public long Failed = 0;
            public int TotalSend = 0;
        }
        private static bool SupportIPv6;
        private static Ping ICMPClient = new();
        public class PingInfomation
        {
            public required string Address;
            public int port = 25565;
            public int MaxTry = 1;
            public int Timeout = 2500;
        }
        /// <summary>
        /// 并行发送多个 ICMP/TCP 包来测试本地网络到目标服务器的连通性
        /// </summary>
        /// <param name="Address">目标服务器地址</param>
        /// <param name="port">端口号（仅 Tcping 模式下可用）</param>
        /// <param name="UseTcping">是否使用 Tcping 模式</param>
        /// <param name="MaxTimeout">允许的最大超时</param>
        /// <param name="MaxTry">发送的 ICMP/TCP 包数量（并行发送过多包可能导致额外开销）</param>
        /// <param name="CustomResolver">自定义处理类</param>
        public async static Task<PingResult> Ping(string Address, int port = 80, bool UseTcping = false, int MaxTimeout = 2500, int MaxTry = 4,Func<PingInfomation,object>? CustomResolver = null)
        {
            if (CustomResolver is not null) return new PingResult() { CustomResult = CustomResolver(new PingInfomation() { Address = Address, port = port, Timeout = MaxTimeout, MaxTry = MaxTry }) };
            Logger.Log($"[Network] 开始 Ping {Address}（0.0.0.0），具有 32 字节的数据。");
            var Operation = new List<Task<PingReply>>();
            Operation.AddRange(Enumerable.Range(0, MaxTry).Select(avalue => ICMPClient.SendPingAsync(IPAddress.Parse(Address), MaxTimeout, new byte[32])));
            var ReplyResult = await Task.WhenAll(Operation);

            var Result = new PingResult()
            {
                Result = ReplyResult.ToList()
            };
            Logger.Log($"[Network] {Address}（0.0.0.0）的 Ping 统计结果：\n\n已发送：{Result.TotalSend} 已接收：{Result.Success} 丢包率：{Math.Round((double)(Result.Success / Result.TotalSend), 0)}% \n\n最长：{Result.Slowest}ms 最短：{Result.Fastest} 平均：{Result.Average}ms");
            return Result;
        }
        public static bool CheckIPv6Support()
        {
            foreach(NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (adapter.OperationalStatus != OperationalStatus.Up) continue;
                foreach (UnicastIPAddressInformation IP in adapter.GetIPProperties().UnicastAddresses)
                {
                    if (IP.Address.AddressFamily is AddressFamily.InterNetworkV6)
                    {
                        SupportIPv6 = true;
                        break;
                    }
                }
            }
        TestIPv6:
            PingResult Result = Ping("[2400:3200:baba::1]").GetAwaiter().GetResult();
        Finally:
            return SupportIPv6;
        }
    }
    /// <summary>
    /// 支持 DNS Over HTTPS 的域名解析查询类
    /// </summary>
    public class DNSResolver {
        private static readonly Dictionary<string, DNSResolveResult> DnsQueryCache = new();
        public class DNSResolveResult
        {
            public List<string?>? Address;
            public List<string>? IPAddress;
        }
        private static Dictionary<string, DNSResolveResult> DnsQueryResult = new();
        public static string? DOHServerAddress;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="RequestUrl"></param>
        /// <param name="ResolveTimeout"></param>
        /// <returns></returns>
        public async static Task<DNSResolveResult> GetResolveResultUsingLocalDns(string RequestUrl, int ResolveTimeout = 500)
        {
            try
            {
                // DNS 查询不像网络请求，过长的查询时间会让下载速度缓慢（尤其是从很多不同服务器下载文件，这种情况下 DNS 查询导致的缓慢会更加明显）
                using (CancellationTokenSource CTS = new(ResolveTimeout))
                {
                    IPHostEntry ResolveResult = await Dns.GetHostEntryAsync(RequestUrl);
                    return new DNSResolveResult()
                    {
                        IPAddress = ResolveResult.AddressList.Select(ip => ip.ToString()).ToList(),
                        Address = ResolveResult.Aliases.ToList()!
                    };
                }
            }
            catch (TaskCanceledException ex)
            {
                throw new TimeoutException("操作超时", ex);
            }
            catch (SocketException ex)
            {
                throw new TaskCanceledException($"未能解析此远程名称 {RequestUrl}", ex);
            }
            catch (ArgumentException)
            {
                throw new ArgumentException("此 URI 格式不正确或为空字符串");
            }
        }
        public async static Task<DNSResolveResult?> GetResolveResultUsingDOH(string RequestUrl, int ResolveTimeout = 500)
        {
            try
            {
                // DNS 查询不像网络请求，过长的查询时间会让下载速度缓慢（尤其是从很多不同服务器下载文件，这种情况下 DNS 查询导致的缓慢会更加明显）
                using (CancellationTokenSource CTS = new(ResolveTimeout))
                {
                    await HttpRequestBuilder
                        .Create(DOHServerAddress! + "?name=" + RequestUrl + "&type=A", HttpMethod.Get)
                        .SetSourceAddress(DNSResolver.GetResolveResultUsingLocalDns(RequestUrl).Result.IPAddress![0])
                        .SetConnectPort(443)
                        .SetHeader("Accept", "application/dns-json")
                        .UseProxy()
                        .Invoke();
                    return null;
                }
            }
            catch (TaskCanceledException ex)
            {
                throw new TimeoutException("操作超时", ex);
            }
            catch (SocketException ex)
            {
                throw new TaskCanceledException($"未能解析此远程名称 {RequestUrl}", ex);
            }
            catch (ArgumentException)
            {
                throw new ArgumentException("此 URI 格式不正确或为空字符串");
            }
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
                    var data = await Network.NetworkRequest(t.url, Token: token);
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