using System.Text.Json.Nodes;
using SuikaiLauncher.Core.Account.JsonModel;
using SuikaiLauncher.Core.Base;
using SuikaiLauncher.Core.Override;

namespace SuikaiLauncher.Core.Account{
    public class MinecraftYggdrasil{
        internal async Task<Tuple<bool,string>> YggdrasilAuthorize(string XSTSToken,string UserHash){
            MinecraftRPLogin AuthData = new() { identityToken = $"XBL3.0 x={XSTSToken};{UserHash}" };
            string LoginData = Json.Serialize<MinecraftRPLogin>(AuthData);
            using (MemoryStream ReqData = new(LoginData.GetBytes()))
            {
                HttpResponseMessage Response = (await HttpRequestBuilder
                    .Create("https://api.minecraftservices.com/authentication/login_with_xbox", HttpMethod.Post)
                    .WithRequestData(ReqData)
                    .SetHeader("Content-Type", "application/json")
                    .SetHeader("Accept", "application/json")
                    .SetHeader("UserAgent", HttpRequestBuilder.UserAgent)
                    .UseProxy()
                    .Invoke()).GetResponse();

                
                    string UserAuthResult = await FileIO.ReadAsString(Response!);
                    if (!UserAuthResult.IsNullOrWhiteSpaceF())
                    {
                        MinecraftOfficialYggdrasil JsonData = Json.Deserialize<MinecraftOfficialYggdrasil>(UserAuthResult);
                        return Tuple.Create(true, JsonData.accessToken);
                }
            }
                return Tuple.Create(false, string.Empty);
            }
        internal async static Task<bool> ValidateOwnership(string AccessToken){
            HttpResponseMessage Response = (await HttpRequestBuilder
                .Create("https://api.minecraftservices.com/entitlements/mcstore", HttpMethod.Get)
                .SetHeader("Authorization", AccessToken)
                .SetHeader("User-Agent", HttpRequestBuilder.UserAgent)
                .UseProxy()
                .Invoke()).GetResponse();
            Ownership UserAuthResult = Json.Deserialize<Ownership>(await FileIO.ReadAsString(Response));
            return !(UserAuthResult.items.Count <= 0);
        }
        internal async static Task<MinecraftUserProfile?> GetMinecraftProfile(string AccessToken){
            try{
                HttpResponseMessage Response = (await HttpRequestBuilder
                .Create("https://api.minecraftservices.com/minecraft/profile", HttpMethod.Get)
                .SetHeader("Authorization", AccessToken)
                .SetHeader("User-Agent", HttpRequestBuilder.UserAgent)
                .UseProxy()
                .Invoke()).GetResponse();
                
                MinecraftUserProfile PlayerProfile = Json.Deserialize<MinecraftUserProfile>(await FileIO.ReadAsString(Response));
                return PlayerProfile;
            }catch (Exception ex){
                Logger.Log(ex,"[Account] 获取档案信息失败");
            }
            return null;
        }
    }
}