using System.Text.Json.Nodes;
using SuikaiLauncher.Core.Override;
using SuikaiLauncher.Core.Base;
using SuikaiLauncher.Core.Account.JsonModel;

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
            var UserAuthData = await FileIO.ReadAsString(UserAuthResult);
            if (!UserAuthResult.IsSuccessStatusCode){
                YggdrasilAuthError Result = Json.Deserialize<YggdrasilAuthError>(UserAuthData);
                Logger.Log($"[Account] 第三方登录失败：远程服务器返回错误：{UserAuthResult.StatusCode}");
                List<string> ErrorReason = new ();
                try
                {
                    ErrorReason.AddRange(Result.errorMessage);
                }
                catch
                {
                    try
                    {
                        ErrorReason.Add(Result.errorMessage);
                    }
                    catch
                    {
                        
                    }
                }
                try
                {
                    ErrorReason.AddRange(Result.Message);
                }
                catch
                {
                    try
                    {
                        ErrorReason.Add(Result.Message);
                    }
                    catch
                    {
                        
                    }
                }
                ErrorReason.RemoveAll(string.IsNullOrWhiteSpace);
                Logger.Log($"[Account] 已获取到 {ErrorReason.Count} 条错误原因");
                if (ErrorReason.Count <= 0) {
                    ErrorReason.Add("账号或密码错误。");
                    ErrorReason.Add("只注册了账号，而没有创建角色。");
                    ErrorReason.Add("账号已被封禁，或者登录过于频繁导致被临时封禁。");
                }
                return Tuple.Create(false, string.Empty, new List<PlayerProfile>() {new PlayerProfile{
                    ErrorMessage = string.Join("\n",ErrorReason)
                }});
                }
            YggdrasilUserAuth AuthResult = Json.GetJson<YggdrasilUserAuth>(UserAuthData);
            if (AuthResult.availableProfiles is null || AuthResult.availableProfiles.Count <= 0) throw new ArgumentException("无可用角色");
            List<Profile?> AvailableProfiles = AuthResult.availableProfiles;
            Profile? SelectedProfile = AuthResult.selectedProfile;
            
            
            if (SelectedProfile is not null && SelectedProfile.id is not null){
                string? SkinUrl = string.Empty;
                string? CapeUrl = string.Empty;
                foreach (PlayerProperties Property in SelectedProfile.Properties){
                    if (!Property.name.ContainsF("texture")) continue;
                    Texture Textures = Json.GetJson<Texture>(Property.value.ToString().Base64Decode());
                    SkinUrl = Textures.textures.Skin?.url.ToString();
                    CapeUrl = Textures.textures.Cape?.url.ToString();
                }
                return Tuple.Create(true,AuthResult.accessToken, new List<PlayerProfile>() { new PlayerProfile(){
                    PlayerName = SelectedProfile.name,
                    UUID = SelectedProfile.id,
                    SkinUrl = SkinUrl,
                    CapeUrl = CapeUrl
                } });
            }
            
            List<PlayerProfile> AllProfiles = new();
            foreach (var Profile in AvailableProfiles)
            {
                if (Profile is null) continue;
                string? SkinUrl = string.Empty;
                string? CapeUrl = string.Empty;
                foreach (var Property in Profile.Properties)
                {
                    if (!Property.name.ContainsF("texture")) continue;
                    Texture PlayerTextures = Json.GetJson<Texture>(Property.value.Base64Decode());
                    SkinUrl = PlayerTextures.textures.Skin?.url.ToString();
                    CapeUrl = PlayerTextures.textures.Cape?.url.ToString();
                }
                AllProfiles.Add(new PlayerProfile()
                {
                    PlayerName = Profile.name.ToString(),
                    UUID = Profile.id.ToString(),
                    SkinUrl = SkinUrl,
                    CapeUrl = CapeUrl
                });
            }
            return Tuple.Create(true,AuthResult.accessToken,AllProfiles);
            
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
                YggdrasilUserAuth UserProfile = Json.GetJson<YggdrasilUserAuth>(await FileIO.ReadAsString(RefreshResult));
                string? SkinUrl = string.Empty;
                string? CapeUrl = string.Empty;
                if (UserProfile.selectedProfile is null) throw new ArgumentException("选定档案信息无效");
                foreach (var Property in UserProfile.selectedProfile.Properties)
                {
                    if (!Property.name.ContainsF("texture")) continue;
                    Texture UserTexture = Json.GetJson<Texture>(Property.value.ToString().Base64Decode());
                    SkinUrl = UserTexture.textures.Skin?.url.ToString();
                    CapeUrl = UserTexture.textures.Cape?.url.ToString();
                }
                return Tuple.Create(true,UserProfile.accessToken,new PlayerProfile(){
                    PlayerName = UserProfile.selectedProfile.name.ToString(),
                    UUID = UserProfile.selectedProfile.id.ToString(),
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