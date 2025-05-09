using System.Text.Json.Nodes;
using SuikaiLauncher.Core.Base;
using SuikaiLauncher.Core.Override;

namespace SuikaiLauncher.Core.Account{
    public class MinecraftProfile{
        public string PlayerName = "";
        public string PlayerUUID = "";
        public string PlayerSkin = "";
        public List<string>? PlayerCapes;
    }
    public class MinecraftYggdrasil{
        internal async Task<Tuple<bool,string>> YggdrasilAuthorize(string XSTSToken,string UserHash){
            JsonObject ReqData = new() {
                ["JsonProperty"] = $"XBL3.0 x={XSTSToken};{UserHash}"
            };
            HttpResponseMessage Response = await Network.NetworkRequest(
                "https://api.minecraftservices.com/authentication/login_with_xbox",
                new Dictionary<string, string>() { ["Content-Type"] = "application/json"},
                ReqData.ToJsonString(),
                method:"POST"
            );
            if(Response.IsSuccessStatusCode){
                string UserAuthResult = await FileIO.ReadAsString(Response);
                if(!UserAuthResult.IsNullOrWhiteSpaceF()){
                    var JsonData = Json.GetJson(UserAuthResult);
                    if (JsonData is not null) {
                        var AccessToken = JsonData["access_token"];
                        if (AccessToken is not null)
                        return Tuple.Create(true,AccessToken.ToString());
                    }
                }
            }
            return Tuple.Create(false,string.Empty);
        }
        internal async static Task<bool> ValidateOwnership(string AccessToken){
            HttpResponseMessage Response = await Network.NetworkRequest(
                "https://api.minecraftservices.com/entitlements/mcstore",
                new Dictionary<string, string>(){
                    ["Authorization"] = AccessToken
                }
            ) ;
            JsonNode UserAuthResult = Json.GetJson(await FileIO.ReadAsString(Response));
            return !(Json.GetJsonArray(UserAuthResult["items"]).Count <= 0);
        }
        internal async static Task<MinecraftProfile?> GetMinecraftProfile(string AccessToken){
            try{
                HttpResponseMessage Response = await Network.NetworkRequest(
                    "https://api.minecraftservices.com/minecraft/profile",
                    new Dictionary<string, string>() {
                        ["Authorization"] = AccessToken
                    });
                JsonNode PlayerProfile = Json.GetJson(await FileIO.ReadAsString(Response));
                
            }catch (Exception ex){
                Logger.Log(ex,"[Account] 获取档案信息失败");
            }
            return null;
        }
    }
}