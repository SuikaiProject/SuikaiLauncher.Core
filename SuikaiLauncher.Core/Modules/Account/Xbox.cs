#pragma warning disable CS8619

using System.Text.Json.Nodes;
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
            JsonNode Result = Json.GetJson((await FileIO.ReadAsString(UserAuthResult)));
            string? Token = Result["Token"]?.ToString();
            JsonNode? DisplayClaims = Result["DisplayClaims"];
            if (DisplayClaims is not null){
                JsonNode? Xui = DisplayClaims["xui"];
                if (Xui is not null) {
                    if(Xui.AsArray()[0] is not null){
                        string? XboxUserHash = Xui.AsArray()[0]?["uhs"]?.ToString();
                        if(Token.IsNullOrWhiteSpaceF() || XboxUserHash.IsNullOrWhiteSpaceF()) return Tuple.Create(true,Token,XboxUserHash);
                    }
                }
            }
            return Tuple.Create(false,string.Empty,string.Empty);
        }
        internal async static Task<Tuple<bool,string?>> XSTSAuthorize(string AccessToken){
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
            JsonNode Result = Json.GetJson(await FileIO.ReadAsString(UserAuthResult));
            string? Token = Result["Token"]?.ToString();
            if (Token.IsNullOrWhiteSpaceF()) return Tuple.Create(true,Token);
            return Tuple.Create<bool,string?>(false,string.Empty);
        }
    }
}