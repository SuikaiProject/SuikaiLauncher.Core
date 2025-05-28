#pragma warning disable CS8619

using System.Text.Json.Nodes;
using SuikaiLauncher.Core.Account.JsonModel;
using SuikaiLauncher.Core.Base;
using SuikaiLauncher.Core.Override;

namespace SuikaiLauncher.Core.Account{
    public class XboxClient{
        internal async static Task<Tuple<bool,string,string>> XboxLiveAuthorize(string AccessToken){
            JsonObject ReqData = new(){
                ["Properties"] = new JsonObject(){
                    ["AuthMethod"] = "RPS",
                    ["SiteName"] = "user.auth.xboxlive.com",
                    ["RpsTicket"] = AccessToken
                },
                ["RelyingParty"] = "http://auth.xboxlive.com",
                ["TokenType"] = "JWT"
            };
            HttpResponseMessage UserAuthResult = await Network.NetworkRequest(
                "https://user.auth.xboxlive.com/user/authenticate",
                new Dictionary<string, string>(){
                    ["Content-Type"] = "application/json"
                },ReqData.ToJsonString(),
                method:"POST"
            );
            XboxLiveAuth Result = Json.GetJson<XboxLiveAuth>((await FileIO.ReadAsString(UserAuthResult)));
            return Tuple.Create(true,Result.Token,Result.DisplayClaims.xui[0].uhs);
        }
        internal async static Task<Tuple<bool,string>> XSTSAuthorize(string AccessToken){
            JsonObject ReqData = new(){
                ["Properties"] = new JsonObject(){
                    ["SandboxId"] = "RETAIL",
                    ["UserTokens"] = new JsonArray(){
                        AccessToken
                    }
                },
                ["RelyingParty"] = "rp://api.minecraftservices.com",
                ["TokenType"] = "JWT"
            };
            HttpResponseMessage UserAuthResult = await Network.NetworkRequest(
                "https://xsts.auth.xboxlive.com/xsts/authorize",
                new Dictionary<string, string>(){
                    ["Content-Type"] = "application/json"
                },
                ReqData.ToJsonString(),
                method:"POST"
                );
            XboxLiveAuth Result = Json.GetJson<XboxLiveAuth>(await FileIO.ReadAsString(UserAuthResult));
            return Tuple.Create(true,Result.Token);
        }
    }
}