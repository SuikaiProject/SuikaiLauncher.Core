using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Broker;
using SuikaiLauncher.Core.Modules.Base;
using SuikaiLauncher.Core.Override;

namespace SuikaiLauncher.Core.Account {
    public class Microsoft
    {
        
        public static string ClientId = "";
        private static readonly BrokerOptions options = new(BrokerOptions.OperatingSystems.Windows) { Title = "SuikaiLauncher.Core 安全登录" };
        private static IPublicClientApplication? OAuthClient;
        private static readonly List<string> Scope = new() { "XboxLive.Signin", "offline_access" };
        private static string OriginId = "";
        private static readonly object MSOAuthLock = new object[1];
        // 设备代码流登录用
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
                    .WithParentActivityOrWindow(Window.GetForegroundWindow)
                    .WithBroker(options)
                    .Build();
            }
        }
        public static Tuple<bool,AuthenticationResult?> MSLoginWithWAM() {
            try {
                Logger.Log("[Account] Microsoft 登录开始（授权代码流登录）");
                if (ClientId != OriginId) OAuthClient = null; OriginId = ClientId;
                if (OAuthClient is null ) InitOAuthClient();
                Logger.Log("[Account] 初始化 WAM 成功");
                AuthenticationResult? Result = OAuthClient
                    .AcquireTokenInteractive(Scope)
                                .ExecuteAsync()
                                .GetAwaiter()
                                .GetResult();
                Logger.Log("[Account] Microsoft 登录成功");
                Logger.Log($"[Account] 令牌过期时间：{Result.ExpiresOn}");
                
                return Tuple.Create<bool, AuthenticationResult?>(true,Result);
            } catch (MsalUiRequiredException ex) {
                Logger.Log(ex, "[Account] Microsoft 登录失败");
                return Tuple.Create<bool, AuthenticationResult?>(false, null);
            }
            catch (Exception ex)
            {
                Logger.Log(ex, "Microsoft 登录失败");
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
                var Result = OAuthClient
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
                Logger.Log(ex, "[Account] Microsoft 登录过程中出现未知错误");
                throw;
            }
        } 
        internal static AuthenticationResult? MSLoginRefresh()
        {
            return null;
        }
        internal async static Task<IEnumerable<IAccount>> GetLoginAccount()
        {
            if (OAuthClient is null) InitOAuthClient();
            return await OAuthClient.GetAccountsAsync();
        }
        public static int MSALogin(bool PerferDeviceLogin)
        {
            Tuple<bool,AuthenticationResult?>? Result;
            if (PerferDeviceLogin) Result = MSLoginDevice();
            else Result = MSLoginWithWAM();
                return 0;
        }
    }
}