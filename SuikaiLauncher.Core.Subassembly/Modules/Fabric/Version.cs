namespace SuikaiLauncher.Core.Subassembly.Fabric{
    public class FabricVersion{
        internal static string FabricAPIOfficial = "https://meta.fabricmc.net/v2";
        internal static string FabricAPIBMCLAPI = "https://bmclapi2.bangbang93.com/fabric-meta";
        
        public static void GetFabricVersion(bool PerferMirror = false){
            
        }
    }
    public class FabricMeta{
        public string? LoaderVersion;
        public string? InheritsVersion;
        public bool Release;
        public string? InstallerUrl;
    }
}