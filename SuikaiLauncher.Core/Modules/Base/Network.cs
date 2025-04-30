﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SuikaiLauncher.Core
{
    public class Network
    {
        private static readonly HttpClient client = new HttpClient();
        private static readonly HttpClientHandler clientHandler = new HttpClientHandler() { AllowAutoRedirect = false };
        private static readonly string LauncherUA = "SuikaiLauncher/0.0.2";
        private static readonly string SpacialBrowserUA = "SuikaiLauncher/0.0.2 Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:137.0) Gecko/20100101 Firefox/137.0";
        
        public async static Task<HttpResponseMessage?> NetworkRequest(string? url, Dictionary<string, string>? headers = null, string? data = null, byte[]? ByteData = null, int timeout = 10000, string method = "GET", bool UseBrowserUA = false, int retry = 5)
        {
            try
            {
                int redirect = 20;
                if (string.IsNullOrWhiteSpace(url)) return null;
                List<string> redirectHistory = new List<string> { url };
                client.Timeout = TimeSpan.FromMilliseconds(timeout);
                client.DefaultRequestHeaders.Clear();

                if (headers is not null)
                {
                    foreach (var header in headers)
                    {
                        if (header.Key.ToLower() == "user-agent")
                            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(header.Value));
                        else
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

                string UserAgent = UseBrowserUA ? SpacialBrowserUA : LauncherUA;
                if (client.DefaultRequestHeaders.UserAgent.Count == 0)
                    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("SuikaiLauncher", "0.0.2"));

                HttpRequestMessage Request = new HttpRequestMessage(RequestMethod, url);
                if (!string.IsNullOrWhiteSpace(data))
                    Request.Content = new StringContent(data);
                else if (ByteData is not null)
                    Request.Content = new ByteArrayContent(ByteData);

                while (retry > 0)
                {
                    try
                    {
                        HttpResponseMessage Response = await client.SendAsync(Request);
                        int status = (int)Response.StatusCode;

                        if (300 <= status && status <= 399)
                        {
                            if (redirect <= 0)
                            {
                                Logger.Log($"[Network] 重定向次数过多\n重定向历史：{string.Join("->", redirectHistory)}");
                                throw new Exception("重定向次数过多");
                            }
                            else if (Response.Headers.Location != null)
                            {
                                redirect--;
                                url = Response.Headers.Location.ToString();
                                redirectHistory.Add(url);
                                Request = new HttpRequestMessage(RequestMethod, url);
                                continue;
                            }
                        }
                        return Response;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex, "[Network] 请求失败");
                        if (--retry <= 0) throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex, "[Network] 请求失败");
                throw;
            }
            return null;
        }
    }

    public class Download
    {
        public class FileMetaData{
            public string? path {get;set;}
            public string? hash {get;set;}
            public string? algorithm {get;set;}
            public long? size {get;set;}
            public string? url {get;set;}
            public bool ValidatePathContains(string path){
                if (string.IsNullOrWhiteSpace(this.path) || string.IsNullOrWhiteSpace(path)) return false;
                return Path.GetFullPath(this.path).StartsWith(path);
            }

        }
        public static readonly object FileListLock = new object[1];
        public static long TotalFileCouht = 0;
        public static long CompleteFileCount = 0;
        public static void Start(List<FileMetaData> DlTasks, int MaxThreadCount = 64)
        {
            SemaphoreSlim semaphore = new SemaphoreSlim(MaxThreadCount);
            Task[] DlTask = new Task[DlTasks.Count];
            int TaskCount = 0;

            foreach (var task in DlTasks)
            {
                if (task is null) throw new ArgumentNullException("无效的参数");

                DlTask[TaskCount] = Task.Run(async () =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        string? FilePath = task.path;
                        if (string.IsNullOrWhiteSpace(FilePath))
                        {
                            throw new ArgumentNullException("文件路径无效");
                        }

                        HttpResponseMessage? Result = await Network.NetworkRequest(task.url);
                        if (Result is null) throw new Exception("下载时出现未知错误");

                        using (Stream Reader = await Result.Content.ReadAsStreamAsync())
                        using (Stream Writer = File.OpenWrite(FilePath))
                        {
                            if (Writer.CanWrite && Reader.CanRead)
                            {
                                byte[] buffer = new byte[8192];
                                int bytesRead;
                                while ((bytesRead = await Reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
                                {
                                    await Writer.WriteAsync(buffer, 0, bytesRead);
                                }
                            }
                            else
                            {
                                throw new IOException("无法读取或写入流");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex, "下载文件失败");
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                TaskCount++;
            }

            try
            {
                Task.WaitAll(DlTask);
            }
            catch (AggregateException ae)
            {
                foreach (var ex in ae.InnerExceptions)
                {
                    Logger.Log(ex, "任务执行失败");
                }
            }
        }
        /// <summary>
        /// 使用 HttpClient 获取目标文件的内容
        /// </summary>
        /// <param name="FileMeta">目标文件的元数据</param>
        /// <returns>byte[]</returns>
        public async static Task<byte[]> NetGetFileByClient(FileMetaData FileMeta){
            HttpResponseMessage? Response = await Network.NetworkRequest(FileMeta.url);
            if (Response is null) throw new WebException("下载文件失败");
            using (Stream Reader = await Response.Content.ReadAsStreamAsync()){
                byte[]? data = null;
                Reader.Read(data);
                if (data is null) throw new IOException("读取网络流失败");
                return data;
            }
        }
    }
}