using System.Net.Http.Headers;

namespace SuikaiLauncher.Core.Account{
    public class UserCustom{
        private string MinecraftServiceAPI = "https://api.minecraftservices.com";
        public static string GetTranslateByCapeName(string Cape){
            switch (Cape.ToLower()){
                case "migrator":
                    return "迁移者披风";
                case "home":
                    return "家园披风";
                case "common":
                    return "普通披风";
                default:
                    return Cape;
            }
        }
    }
}