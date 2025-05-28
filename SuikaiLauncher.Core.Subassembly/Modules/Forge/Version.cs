using System.Threading.Tasks;
using SuikaiLauncher.Core.Base;
using SuikaiLauncher.Core.Override;
using SuikaiLauncher.Core.Subassembly.Forge.JsonModels;

//(?<=a href=""index_)[0-9.]+(_pre[0-9]?)?(?=.html)

namespace SuikaiLauncher.Core.Subassembly.Forge{
    
    public class ForgeVersionList{
        internal static string ForgeVersionBMCLAPI = "https://bmclapi2.bangbang93.com/forge/minecraft/";
        internal static string ForgeVersionOfficial = "https://files.minecraftforge.net/net/minecraftforge/forge/";
        public static bool PerferOfficial = false;
        public static Dictionary<string, Dictionary<string, string>> Versions = new();
        public static async Task GetForgeVersion(string MinecraftVersion)
        {
            if (!PerferOfficial)
            {
                List<int> Build = new();
                Dictionary<int, ForgeVersionBMCLAPI> VersionList = new();
                HttpResponseMessage Result = await Network.NetworkRequest(ForgeVersionBMCLAPI + MinecraftVersion);
                List<ForgeVersionBMCLAPI> Version = Json.GetJson<List<ForgeVersionBMCLAPI>>(await FileIO.ReadAsString(Result));
                foreach (var Ver in Version)
                {
                    Build.Add(Ver.build);
                    VersionList[Ver.build] = Ver;
                }
                Build.Sort();
            }
            HttpResponseMessage OfficialResult = await Network.NetworkRequest(ForgeVersionOfficial + $"/index_{MinecraftVersion}.html");
            string VersionHtml = await FileIO.ReadAsString(OfficialResult);
            List<string> OfficialVersion = VersionHtml.Regular("<a href=\"[^\"]*forge/[^/]+/forge-[^\"]+-installer.jar\"[^>]*>");
        }
    }
    
}