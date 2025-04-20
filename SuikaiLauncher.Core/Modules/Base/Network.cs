using System.Diagnostics.Contracts;
using System.Net.Http.Headers;

namespace SuikaiLauncher.Core{
    public class Network
    {
        private static readonly HttpClient client = new HttpClient();
        private static readonly HttpClientHandler clientHandler = new HttpClientHandler() { AllowAutoRedirect = false };
        private static readonly string LauncherUA = "SuikaiLauncher/0.0.2";
        private static readonly string SpacialBrowserUA = "SuikaiLauncher/0.0.2 Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:137.0) Gecko/20100101 Firefox/137.0";
        //private static readonly string BrowserUA = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:137.0) Gecko/20100101 Firefox/137.0";

        public async static Task<HttpResponseMessage> NetworkRequest(string url, Dictionary<String, String> headers, string? data = null, byte[]? ByteData = null, int timeout = 10000, string method = "GET", bool UseBrowserUA = false) {
        Retry:
            try {

                List<string> redirectHistory = new List<string>();
                client.Timeout = TimeSpan.FromMilliseconds(timeout);
                client.DefaultRequestHeaders.UserAgent.Clear();
                foreach (var header in headers) {
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
                HttpRequestMessage Request = new HttpRequestMessage(RequestMethod, url);
                string UserAgent = "";
                if (UseBrowserUA) UserAgent = SpacialBrowserUA;
                else UserAgent = LauncherUA;
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(UserAgent));
                if (!string.IsNullOrEmpty(data)) Request.Content = new StringContent(data);
                else if (ByteData is not null) Request.Content = new ByteArrayContent(ByteData);
                return await client.SendAsync(Request);
            }catch(Exception ex) {
                Logger.Log(ex, "发送 HTTP 请求失败");
                goto Retry
            }
        }
    }
}
