﻿using Downloader;
using SuikaiLauncher.Core.Override;

namespace SuikaiLauncher.Core
{
    public class Network
    {
        private static readonly HttpClientHandler clientHandler = new HttpClientHandler() { AllowAutoRedirect = false };
        private static readonly HttpClient client = new HttpClient(clientHandler);
        private static readonly HttpClient Spacialclient = new HttpClient(clientHandler);


        private static readonly string LauncherUA = "SuikaiLauncher.Core/0.0.2";
        private static readonly string SpacialBrowserUA = "SuikaiLauncher.Core/0.0.2 Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:137.0) Gecko/20100101 Firefox/137.0";

        public async static Task<HttpResponseMessage> NetworkRequest(string url, Dictionary<string, string>? headers = null, string? data = null, byte[]? ByteData = null, int timeout = 10000, string method = "GET", bool UseBrowserUA = false, int retry = 5)
        {
            try
            {
                int redirect = 20;
                List<string> redirectHistory = new() { url };
                client.DefaultRequestHeaders.Clear();

                if (headers is not null)
                {
                    foreach (var header in headers)
                    {
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                }

                var RequestMethod = HttpMethod.Get;
                switch (method.ToUpper())
                {
                    case "POST":
                        RequestMethod = HttpMethod.Post;
                        break;
                    case "PUT":
                        RequestMethod = HttpMethod.Put;
                        break;
                    case "DELETE":
                        RequestMethod = HttpMethod.Delete;
                        break;
                    case "HEAD":
                        RequestMethod = HttpMethod.Head;
                        break;
                }


                HttpRequestMessage Request = new HttpRequestMessage(RequestMethod, url);
                if (!data.IsNullOrWhiteSpaceF())
                    Request.Content = new StringContent(data);
                else if (ByteData is not null)
                    Request.Content = new ByteArrayContent(ByteData);

                while (retry >= 0)
                {
                    try
                    {
                        CancellationTokenSource Token = new(timeout);
                        HttpResponseMessage Response = (UseBrowserUA) ? await Spacialclient.SendAsync(Request, Token.Token) : await client.SendAsync(Request, Token.Token);
                        int status = (int)Response.StatusCode;
                        if (300 <= status && status <= 399)
                        {
                            if (redirect <= 0)
                            {
                                Logger.Log($"[Network] 重定向次数过多\n重定向历史：{string.Join("->", redirectHistory)}");
                                throw new TaskCanceledException("重定向次数过多");
                            }
                            else if (Response.Headers.Location != null)
                            {
                                redirect--;
                                url = Response.Headers.Location.ToString();
                                redirectHistory.Add(url);
                                Request = new HttpRequestMessage(RequestMethod, url);
                                if (!data.IsNullOrWhiteSpaceF())
                                    Request.Content = new StringContent(data);
                                else if (ByteData is not null)
                                    Request.Content = new ByteArrayContent(ByteData);


                            }
                            Response.Dispose();
                            continue;
                        }
                        return Response;

                    }
                    catch (TaskCanceledException ex)
                    {
                        Logger.Log(ex, "[Network] 由于远程服务器未能正确处理响应，连接已中止");
                        throw;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex, "[Network] 请求失败");
                        if (retry <= 0) throw;
                        Request = new HttpRequestMessage(RequestMethod, url);
                        if (!data.IsNullOrWhiteSpaceF())
                            Request.Content = new StringContent(data);
                        else if (ByteData is not null)
                            Request.Content = new ByteArrayContent(ByteData);
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex, "[Network] 请求失败");
                throw;
            }
            throw new TaskCanceledException("发送请求失败");
        }
    }

    public class Download
    {
        public class FileMetaData
        {
            public string? path { get; set; }
            public string? hash { get; set; }
            public string? algorithm { get; set; }
            public long? size { get; set; }
            public string? url { get; set; }
            public bool ValidatePathContains(string path)
            {
                if (this.path.IsNullOrWhiteSpaceF() || path.IsNullOrWhiteSpaceF()) return false;
                return Path.GetFullPath(this.path).StartsWith(path);
            }

        }
        private static DownloadConfiguration DlOpt = new()
        {
            ChunkCount = 64,
            MaximumMemoryBufferBytes = 25 * 1024 * 1024,
            ParallelDownload = true,
            Timeout = 10000,
            RangeDownload = true,
            MinimumSizeOfChunking = 1024,
            RequestConfiguration = {
                UserAgent = "SuikaiLauncher.Core/0.0.1"
            }
        };
        private static DownloadConfiguration DlOpt0 = new()
        {
            ChunkCount = 64,
            MaximumMemoryBufferBytes = 25 * 1024 * 1024,
            Timeout = 10000,
            RangeDownload = true,
            MinimumSizeOfChunking = 1024,
            RequestConfiguration = {
                UserAgent = "SuikaiLauncher.Core/0.0.1"
            }
        };
        private static void ProgressChangeCallback() {
            
        }
        private static DownloadService MuiltDl = new(DlOpt);
        private static DownloadService SingleDl = new(DlOpt0);
        public static readonly object FileListLock = new object[1];
        public static long TotalFileCount = 0;
        public static long CompleteFileCount = 0;
        
        public static void Start(List<FileMetaData> DlTasks, CancellationToken? Token = null, int MaxThreadCount = 64)
        {
            CancellationTokenSource TokenSource = new();
            var tasks = MuiltDl.DownloadFileTaskAsync(
                    DlTasks[0].url,
                    DlTasks[0].path
                );


            Task.WaitAll(tasks);
        }

    }
}