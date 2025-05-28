using System.Text.Json.Nodes;
using SuikaiLauncher.Core.Account.JsonModel;
using SuikaiLauncher.Core.Base;
using SuikaiLauncher.Core.Override;

namespace SuikaiLauncher.Core.Account{
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
                    MinecraftOfficialYggdrasil JsonData = Json.GetJson<MinecraftOfficialYggdrasil>(UserAuthResult);
                    return Tuple.Create(true,JsonData.accessToken);
                    
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
            Ownership UserAuthResult = Json.GetJson<Ownership>(await FileIO.ReadAsString(Response));
            return !(UserAuthResult.items.Count <= 0);
        }
        internal async static Task<MinecraftUserProfile?> GetMinecraftProfile(string AccessToken){
            try{
                HttpResponseMessage Response = await Network.NetworkRequest(
                    "https://api.minecraftservices.com/minecraft/profile",
                    new Dictionary<string, string>() {
                        ["Authorization"] = AccessToken
                    });
                MinecraftUserProfile PlayerProfile = Json.GetJson<MinecraftUserProfile>(await FileIO.ReadAsString(Response));
                return PlayerProfile;
            }catch (Exception ex){
                Logger.Log(ex,"[Account] 获取档案信息失败");
            }
            return null;
        }
    }
}