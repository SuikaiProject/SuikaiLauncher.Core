#pragma warning disable CS1998

using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Broker;
using SuikaiLauncher.Core.Base;
using SuikaiLauncher.Core.Override;

namespace SuikaiLauncher.Core.Extensions.Account {
    /// <summary>
    /// 微软登录
    /// </summary>
    public class Microsoft
    {
        public static string ClientId = "";
        private static readonly BrokerOptions options = new(BrokerOptions.OperatingSystems.Windows) { Title = "SuikaiLauncher.Core 安全登录" };
        private static IPublicClientApplication? OAuthClient;
        private static readonly List<string> Scope = new() { "XboxLive.Signin", "offline_access" };
        private static string OriginId = "";
        private static readonly object MSOAuthLock = new object[1];
        // 设备代码流用
        public static string? UserCode;
        public static string? VerificationUrl;

        internal static void InitOAuthClient()
        {
            lock (MSOAuthLock)
            {
                if (ClientId.IsNullOrWhiteSpaceF()) throw new ArgumentNullException("Client ID 不能为空");
                OAuthClient = PublicClientApplicationBuilder
                    .Create(ClientId)
                    .WithDefaultRedirectUri()
                    .WithParentActivityOrWindow(Win32.GetForegroundWindow)
                    .WithBroker(options)
                    .Build();
            }
        }
        internal async static Task<Tuple<bool,AuthenticationResult?>> MSLoginWithWAM() {
            try {
                Logger.Log("[Account] Microsoft 登录开始（授权代码流登录）");
                if (ClientId != OriginId) OAuthClient = null; OriginId = ClientId;
                if (OAuthClient is null) InitOAuthClient();
                Logger.Log("[Account] 初始化 WAM 成功");
                AuthenticationResult? Result = await OAuthClient!
                    .AcquireTokenInteractive(Scope)
                                .ExecuteAsync();
                Logger.Log("[Account] Microsoft 登录成功");
                Logger.Log($"[Account] 令牌过期时间：{Result.ExpiresOn} 秒");
                
                return Tuple.Create<bool, AuthenticationResult?>(true,Result);
            } catch (MsalUiRequiredException ex) {
                Logger.Log(ex, "[Account] Microsoft 登录失败");
                return Tuple.Create<bool, AuthenticationResult?>(false, null);
            }
            catch (Exception ex)
            {
                Logger.Log(ex, "Microsoft 登录时发生未知错误");
                throw;
            }
        }
        internal async static Task MSLoginDeviceCallback(DeviceCodeResult Result)
        {
            UserCode = Result.UserCode;
            VerificationUrl = Result.VerificationUrl;
        }
        internal static Tuple<bool,AuthenticationResult?> MSLoginDevice()
        {
            Logger.Log("[Account] Microsoft 登录开始（设备代码流登录）");
            try
            {
                var Result = OAuthClient!
                    .AcquireTokenWithDeviceCode(Scope, MSLoginDeviceCallback)
                    .ExecuteAsync()
                    .GetAwaiter()
                    .GetResult();
                
                return Tuple.Create<bool,AuthenticationResult?>(true, Result);
            }catch(MsalException ex){
                Logger.Log(ex, "[Account] Microsoft 登录失败");
                return Tuple.Create<bool, AuthenticationResult?>(false, null);
            }catch(Exception ex)
            {
                Logger.Log(ex, "[Account] Microsoft 登录时发生未知错误");
                throw;
            }
        } 
        internal async static Task<AuthenticationResult?> MSLoginRefresh(string AccountID)
        {
            if (OAuthClient is null) InitOAuthClient();
            try{
                var Result = await GetLoginAccount();
                if (!Result.Item1 || Result.Item2 is null) return null;
                foreach(var account in Result.Item2){
                    if (!(account.HomeAccountId.ObjectId == AccountID))  continue;
                    return await OAuthClient!
                    .AcquireTokenSilent(Scope, account.HomeAccountId.ObjectId)
                    .ExecuteAsync();
                }
                return null;
            }catch(MsalUiRequiredException){
                var Result = await MSLoginWithWAM();
                if(Result.Item1) return Result.Item2;
                return null;
            }
            catch(Exception ex){
                Logger.Log(ex,"[Account] Microsoft 刷新登录失败");
                return null;
            }
            
        }

        internal async static Task<Tuple<bool,IEnumerable<IAccount>?>> GetLoginAccount()
        {
            try{
                if (OAuthClient is null) InitOAuthClient();
                return Tuple.Create<bool,IEnumerable<IAccount>?>(true,await OAuthClient!.GetAccountsAsync());
            }catch(Exception ex){
                Logger.Log(ex,"[Account] Microsoft 登录失败");
                return Tuple.Create<bool,IEnumerable<IAccount>?>(true,null);
            }
        }

        /// <summary>
        /// WIP:公开登录接口，返回账户的全局 UID
        /// </summary>
        /// <param name="PerferDeviceLogin">是否优先使用设备代码流登录</param>
        /// <returns>一个字符串形式的 GUID</returns>
        public async static Task<string?> MSALogin(bool PerferDeviceLogin)
        {
            Tuple<bool,AuthenticationResult?>? Result;
            if (PerferDeviceLogin) Result = MSLoginDevice();
            else Result = await MSLoginWithWAM();
            if(Result.Item1 && Result.Item2 is not null){
                return Result.Item2.Account.HomeAccountId.ObjectId;
            }
            return null;
        }

    }
}