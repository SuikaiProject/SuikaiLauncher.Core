using System.Text.Json.Nodes;
using SuikaiLauncher.Core.Override;
using SuikaiLauncher.Core.Base;

namespace SuikaiLauncher.Core.Account{
    public class PlayerProfile{
        public string? ErrorMessage;
        public string? PlayerName;
        public string? UUID;
        public string? SkinUrl;
        public string? CapeUrl;
    }
    public class YggdrasilAccount{
        private static readonly string NideAuthServer = "https://auth.mc-user.com:233";
        /// <summary>
        /// 登录类型
        /// </summary>
        public enum LoginType{
            /// <summary>
            /// Authlib-Injector
            /// </summary>
            Auth = 0,
            /// <summary>
            /// 统一通行证
            /// </summary>
            Nide = 1
        }
        public async static Task<string> GetAPILocation(string YggdrasilAPIAddress,LoginType Account = LoginType.Auth){
            try{
                if (Account == LoginType.Nide) return $"{NideAuthServer}/{YggdrasilAPIAddress}";
                Uri Address = new(YggdrasilAPIAddress);
                HttpResponseMessage? Result = await Network.NetworkRequest(Address.Scheme+"//"+Address.AbsoluteUri,method:"HEAD");
                if (Result is null) return string.Empty;
                bool Exist = Result.Content.Headers.TryGetValues("X-Authlib-Injector-Api-Location",out var Location);
                if(Exist && Location is not null) { 
                    string YggdrasilAddress = Location.FirstOrDefault(string.Empty);
                    if (YggdrasilAddress.StartsWith("/")) return $"{Address.Scheme}//{Address.Host}{YggdrasilAddress}";
                    else if (!YggdrasilAddress.IsNullOrWhiteSpaceF()) return $"{Address.Scheme}//{Address.AbsoluteUri.TrimEnd('/')}/{YggdrasilAddress.Replace("./","/")}";
                }
            }catch (Exception ex){
                Logger.Log(ex,"[Account] 获取 Yggdrasil 验证服务器根地址失败");
            }
            return string.Empty;
        }
        public async static Task<Tuple<bool,string,List<PlayerProfile>>?> Authorize(string YggdrasilAPIAddressOrNideId,string User,string Password,LoginType AccountType = LoginType.Auth){
            string Address = await GetAPILocation((YggdrasilAPIAddressOrNideId.StartsWith("http")? YggdrasilAPIAddressOrNideId:"https://" + YggdrasilAPIAddressOrNideId),AccountType);;
            Logger.Log($"[Account] 第三方登录开始 （{((AccountType == LoginType.Auth)? "Authlib-Injector":"统一通行证")}，常规登录）");
            JsonObject ReqData = new(){
                ["username"] = User,
                ["Password"] = Password,
                ["requestUser"] = true,
                ["agent"] = new JsonObject(){
                    ["name"] = "minecraft",
                    ["version"] = 1
                }
            };
            HttpResponseMessage UserAuthResult = await Network.NetworkRequest(
                $"{Address}/authserver/authenticate",
                new Dictionary<string, string>(){
                    ["Content-Type"] = "application/json"
                },
                ReqData.ToJsonString(),
                method:"POST"
            );
            JsonNode Result = Json.GetJson(await FileIO.ReadAsString(UserAuthResult));
            if (!UserAuthResult.IsSuccessStatusCode){
                Logger.Log($"[Account] 第三方登录失败：远程服务器返回错误：{UserAuthResult.StatusCode}");
                JsonNode? Message = Result["message"];
                JsonNode? ErrorMessage = Result["errorMessage"];
                List<string> ErrorReason = new ();
                ErrorReason.AddRange(Json.GetJsonArray(Message));
                ErrorReason.AddRange(Json.GetJsonArray(ErrorMessage));
                ErrorReason.RemoveAll(string.IsNullOrWhiteSpace);
                Logger.Log($"[Account] 已获取到 {ErrorReason.Count} 条错误原因");
                if (ErrorReason.Count <= 0){
                    ErrorReason.Add("账号或密码错误。");
                    ErrorReason.Add("只注册了账号，而没有创建角色。");
                    ErrorReason.Add("账号已被封禁，或者登录过于频繁导致被临时封禁。");
                }
                return Tuple.Create(false,string.Empty,new List<PlayerProfile>() {new PlayerProfile{
                    ErrorMessage = string.Join("\n",ErrorReason)
                }});
            }
            JsonNode? AvailableProfiles = Result["availableProfiles"];
            JsonNode? SelectedProfile = Result["selectedProfile"];
            
            
            if (SelectedProfile?["id"] is not null){
                List<string> Properties = Json.GetJsonArray(SelectedProfile?["properties"]);
                string? SkinUrl = string.Empty;
                string? CapeUrl = string.Empty;
                foreach (var Property in Properties){
                    if (!Property.ContainsF("texture")) continue;
                    JsonNode Texture = Json.GetJson(Property);
                    JsonNode Textures = Json.GetJson(Texture["value"]?.ToString().Base64Decode());
                    SkinUrl = Textures["textures"]?["skin"]?["url"]?.ToString();
                    CapeUrl = Textures["textures"]?["cape"]?["url"]?.ToString();
                }
                return Tuple.Create(true,$"{Result["accessToken"]?.ToString()}", new List<PlayerProfile>() { new PlayerProfile(){
                    PlayerName = SelectedProfile?["name"]?.ToString(),
                    UUID = SelectedProfile?["id"]?.ToString(),
                    SkinUrl = SkinUrl,
                    CapeUrl = CapeUrl
                } });
            }
            List<PlayerProfile> AllProfiles = new();
            List<string> Profiles = Json.GetJsonArray(AvailableProfiles);
            foreach(var Profile in Profiles){
                JsonNode _Profile = Json.GetJson(Profile);
                List<string> Properties = Json.GetJsonArray(_Profile?["properties"]);
                string? SkinUrl = string.Empty;
                string? CapeUrl = string.Empty;
                foreach (var Property in Properties){
                    if (!Property.ContainsF("texture")) continue;
                    JsonNode Texture = Json.GetJson(Property);
                    JsonNode Textures = Json.GetJson(Texture["value"]?.ToString().Base64Decode());
                    SkinUrl = Textures["textures"]?["skin"]?["url"]?.ToString();
                    CapeUrl = Textures["textures"]?["cape"]?["url"]?.ToString();
                }
                AllProfiles.Add(new PlayerProfile(){
                    PlayerName = _Profile?["name"]?.ToString(),
                    UUID = _Profile?["id"]?.ToString(),
                    SkinUrl = SkinUrl,
                    CapeUrl = CapeUrl
                });
            }
            return Tuple.Create(true,$"{Result["accessToken"]?.ToString()}",AllProfiles);
            
        }
        public async static Task<Tuple<bool,string,PlayerProfile>> Refresh(string YggdrasilAPIAddressOrNideId,string AccessToken,PlayerProfile Profile,LoginType AccountType){
            string Address = await GetAPILocation(YggdrasilAPIAddressOrNideId,AccountType);
            Logger.Log($"[Account] 第三方登录开始 （{((AccountType == LoginType.Auth)? "Authlib-Injector":"统一通行证")}，刷新登录）");
            JsonObject ReqData = new(){
                ["accessToken"] = "",
                ["selectedProfile"] = new JsonObject(){
                    ["name"] = Profile.PlayerName,
                    ["id"] = Profile.UUID,
                }
            };
            HttpResponseMessage RefreshResult = await Network.NetworkRequest(
                Address + "/authserver/refresh",
                new Dictionary<string, string>(){
                    ["Content-Type"] = "application/json"
                },
                ReqData.ToJsonString(),
                method:"POST"
            );
            if (RefreshResult.IsSuccessStatusCode){
                JsonNode UserProfile = Json.GetJson(await FileIO.ReadAsString(RefreshResult));
                List<string> Properties = Json.GetJsonArray(UserProfile["selectedProfile"]?["properties"]);
                string? SkinUrl = string.Empty;
                string? CapeUrl = string.Empty;
                foreach(var Property in Properties){
                    if (!Property.ContainsF("texture")) continue;
                    JsonNode? Texture = Json.GetJson(Property)["value"]?.ToString().Base64Decode();
                    SkinUrl = Texture?["textures"]?["skin"]?["url"]?.ToString();
                    CapeUrl = Texture?["textures"]?["cape"]?["url"]?.ToString();
                }
                return Tuple.Create(true,$"{UserProfile["accessToken"]}",new PlayerProfile(){
                    PlayerName = UserProfile["selectedProfile"]?["name"]?.ToString(),
                    UUID = UserProfile["selectedProfile"]?["id"]?.ToString(),
                    SkinUrl = SkinUrl,
                    CapeUrl = CapeUrl
                });
                
            }
            return Tuple.Create(false,string.Empty,new PlayerProfile());
        }
        public async static Task<bool> Validate(string YggdrasilAPIAddressOrNideId,string AccessToken,LoginType AccountType = LoginType.Auth){
            string Address = await GetAPILocation(YggdrasilAPIAddressOrNideId,AccountType);
            Logger.Log($"[Account] 第三方登录开始 （{((AccountType == LoginType.Auth)? "Authlib-Injector":"统一通行证")}，验证登录）");
            JsonObject ReqData = new(){
                ["accessToken"] = AccessToken
            };
            HttpResponseMessage ValidateResult = await Network.NetworkRequest(
                Address + "/authserver/validate",
                new Dictionary<string, string>(){
                    ["Content-Type"] = "application/json"
                },
                ReqData.ToJsonString(),
                method:"POST"
            );
            if (ValidateResult.IsSuccessStatusCode) {
                Logger.Log("[Account] 令牌有效性验证通过，登录结束");
                return true;
                }
            return false;
        }
        
    }
}