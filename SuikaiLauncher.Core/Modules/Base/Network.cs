using System.Threading.Tasks;
using System.Threading;
using System.Net.Http.Headers;


namespace SuikaiLauncher.Core{
    public class Network
    {
        private static readonly HttpClient client = new HttpClient();
        private static readonly HttpClientHandler clientHandler = new HttpClientHandler() { AllowAutoRedirect = false };
        private static readonly string LauncherUA = "SuikaiLauncher/0.0.2";
        private static readonly string SpacialBrowserUA = "SuikaiLauncher/0.0.2 Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:137.0) Gecko/20100101 Firefox/137.0";
        //private static readonly string BrowserUA = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:137.0) Gecko/20100101 Firefox/137.0";

        public async static Task<HttpResponseMessage?> NetworkRequest(string? url, Dictionary<String, String>? headers = null, string? data = null, byte[]? ByteData = null, int timeout = 10000, string method = "GET", bool UseBrowserUA = false,int retry = 5) {
            try {
                int redirect = 20;
                if (string.IsNullOrWhiteSpace(url)) return null;
                List<string> redirectHistory = new List<string>();
                redirectHistory.Add(url);
                client.Timeout = TimeSpan.FromMilliseconds(timeout);
                client.DefaultRequestHeaders.UserAgent.Clear();
                client.DefaultRequestHeaders.Accept.Clear();
                if(headers is not null) foreach (var header in headers) {
                    if (header.Key.ToLower() == "user-agent") client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(header.Value));
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
                var RequestMethod = HttpMethod.Get;
                switch (method.ToUpper())
                {
                    case "GET":
                        break;
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
                string UserAgent = "";
                if (UseBrowserUA) UserAgent = SpacialBrowserUA;
                else UserAgent = LauncherUA;
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(UserAgent));
                if (client.DefaultRequestHeaders.Accept.Count() <=0) client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpRequestMessage Request = new HttpRequestMessage(RequestMethod, url);
                if (!string.IsNullOrWhiteSpace(data)) Request.Content = new StringContent(data);
                else if (ByteData is not null && string.IsNullOrWhiteSpace(data)) Request.Content = new ByteArrayContent(ByteData);
                while (retry <= 0 || redirect <= 0)
                {
                    try {
                        HttpResponseMessage Response = await client.SendAsync(Request);
                        int status = (int)Response.StatusCode;
                        if (300 <= status && status <= 399)
                        {
                            if (redirect <= 0)
                            {
                                Logger.Log($"[Network] 发送 HTTP 请求失败：目标服务器重定向的次数过多\n重定向历史：{string.Join("->", redirectHistory)}");
                            }
                            else if (Response.Headers.Location is not null)
                            {
                                redirect--;
                                url = Response.Headers.Location.ToString();
                                continue;
                            }
                            throw new ArgumentNullException("无效的重定向响应");
                        }
                        return Response;
                        }catch(Exception ex)
                    {
                        Logger.Log(ex, "[Network] 发送 HTTP 请求失败");
                        if (retry <= 0 || ex is ArgumentNullException) throw;
                        retry--;
                    }
                    }
            }catch(Exception ex) {
                Logger.Log(ex, "[Network] 发送 HTTP 请求失败");
                throw;
            }
            return null;
        }
    }
    public class Download{
        public static void Start(List<Dictionary<string,object>> DlTasks,int MaxThreadCount = 64){
            SemaphoreSlim semaphore = new SemaphoreSlim(MaxThreadCount);
            Task[] DlTask = new Task[DlTasks.Count()];
            int TaskCount =0;
            foreach (var task in DlTasks){
                if (task is null) continue;
                DlTask[TaskCount] = Task.Run(async () => {
                    HttpResponseMessage? Result = await Network.NetworkRequest(task.GetValueOrDefault("url",null)?.ToString());
                    if (Result is null) throw new Exception("下载时出现未知错误");
                    using (Stream Reader = Result.Content.ReadAsStream())
                    {

                    }
                });
                TaskCount ++;
            }
        }
    }
}
