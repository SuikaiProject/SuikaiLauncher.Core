using Microsoft.VisualBasic;
using SuikaiLauncher.Core.Base;
namespace SuikaiLauncher.Core.Account{
    public class PlayerProfile{
        internal string? PlayerName;
        internal string? UUID;
    }
    public class YggdrasilAccount{
        internal async Task<string> GetAPILocation(string YggdrasilAPIAddress){
            try{
                Uri Address = new(YggdrasilAPIAddress);
                HttpResponseMessage? Result = await Network.NetworkRequest(Address.Scheme+"//"+Address.AbsoluteUri,method:"HEAD");
                if (Result is null) return string.Empty;
                bool Exist = Result.Content.Headers.TryGetValues("X-Authlib-Injector-Api-Location",out var Location);
                if(Exist && Location is not null) return Location.FirstOrDefault(string.Empty);
            }catch (Exception ex){
                Logger.Log(ex,"[Account] 获取 Yggdrasil 验证服务器根地址失败");
            }
            return string.Empty;
        }
        public async Task<List<PlayerProfile>?> Authorize(string YggdrasilAPIAddress,string User,string Password){
            string Address = await GetAPILocation((YggdrasilAPIAddress.StartsWith("http")? YggdrasilAPIAddress:"https://" + YggdrasilAPIAddress));
            if (Address.StartsWith("/")){
                Uri Host = new(YggdrasilAPIAddress);
                Address = Host.Scheme + "//" + Host.AbsoluteUri;
            }
            return null;
        }
        
    }
}