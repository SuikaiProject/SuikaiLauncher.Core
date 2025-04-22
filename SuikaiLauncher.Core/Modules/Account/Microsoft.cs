using System.Net;
using System.Text.Json;

namespace SuikaiLauncher.Core.Account {
    public class Microsoft
    {
        private static readonly string clientid = "";
        public static readonly string MicrosoftOAuthDevice = "https://login.microsoftonline.com/consumers/oauth2/v2.0/devicecode";
        private static readonly string MicrosoftOpenID = "https://login.microsoftonline.com/consumers/v2.0/.well-known/openid-configuration";
        
        public async static JsonDocument GetCodePair()
        {
            Dictionary<string, string> Headers = new Dictionary<string, string>();
            Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            string RequestData = $"client_id={clientid}&scope=Xbox.Live.Signin%20offline_access";
            HttpResponseMessage Response = await Network.NetworkRequest(MicrosoftOAuthDevice,Headers,RequestData);
            if (!Response.IsSuccessStatusCode){
                throw WebException;
            }
        }
    }
}