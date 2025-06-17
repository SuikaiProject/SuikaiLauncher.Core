using System.Net;

namespace SuikaiLauncher.Core.Account.Yggdrasil{
    public class YggdrasilServer{
        private static HttpListener Server = new();
        public static async Task StartHttpServer()
        {
            if (Server.IsListening) return;
            Server.Prefixes.Add("http://127.0.0.1:29997");
            Server.Start();
            
        }
        public static async Task RouteRequest(HttpListenerContext Context)
        {
            string? RequestPath = Context.Request.Url?.AbsolutePath;
            if (RequestPath is null) RequestPath = "/";
            while (RequestPath.Contains("//"))
            {
                RequestPath = RequestPath.Replace("//", "/");
            }
            switch (RequestPath)
            {
                case "/":

                    break;
            }
        }
    }
}