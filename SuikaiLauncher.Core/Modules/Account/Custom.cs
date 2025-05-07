namespace SuikaiLauncher.Core.Account{
    private string MinecraftServiceAPI = "https://api.minecraftservices.com";
    public static string GetTranslateByCapeName(string Cape){
        switch (Cape.ToLower()){
            case "common":
                return "普通披风";
        }
    }
}