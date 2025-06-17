using SuikaiLauncher.Core.Account.JsonModel;
using SuikaiLauncher.Core.Base;
using SuikaiLauncher.Core.Override;
using System.Runtime.CompilerServices;


namespace SuikaiLauncher.Core.Account.Modules
{
    public class Microsoft
    {
        public static Action<Dictionary<string, string>>? Callback;
        public static string?ClientId;
        public async static Task GetDeviceCode()
        {
            using(MemoryStream ReqData = new($"client_id={ClientId}&scope=XboxLive.Signin%20offline_access".GetBytes()))
            {
                HttpResponseMessage? Resp = (await HttpRequestBuilder
                .Create("https://login.microsoftonline.com/consumers/oauth2/v2.0/devicecode", HttpMethod.Post)
                .WithRequestData(ReqData)
                .SetHeader("Content-Type", "application/x-www-form-urlencoded")
                .SetHeader("Accept","application/json")
                .SetHeader("User-Agent",HttpRequestBuilder.UserAgent)
                .UseProxy()
                .Invoke()).GetResponse();
                OAuthDeviceCode DeviceCode = Json.Deserialize<OAuthDeviceCode>(await FileIO.ReadAsString(Resp));
                if (Callback is not null) Callback(new Dictionary<string, string>()
                {
                    ["user_code"] = DeviceCode.UserCode,
                    ["verification_uri"] = DeviceCode.Verification,
                    ["verification_uri_complete"] = DeviceCode.VerificationComplete ?? string.Empty
                });

            }

            return;
        }
        public async static Task ValidateUserAuthorizeResult(OAuthDeviceCode DeviceCode)
        {
            using (MemoryStream ReqData = new("device_code=&client_id=&".GetBytes()))
            {
                HttpResponseMessage? Resp = (await HttpRequestBuilder
                        .Create("https://login.microsoftonline.com/consumers/oauth2/v2.0/token", HttpMethod.Post)
                        .WithRequestData(ReqData)
                        .SetHeader("Content-Type", "application/x-www-form-urlencoded")
                        .SetHeader("Accept", "application/json")
                        .SetHeader("User-Agent", HttpRequestBuilder.UserAgent)
                        .Invoke()).GetResponse(false);
            }
        }
    }
}
