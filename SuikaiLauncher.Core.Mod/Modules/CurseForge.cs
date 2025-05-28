using SuikaiLauncher.Core.Override;
using SuikaiLauncher.Core.Mod.JsonModels;
using SuikaiLauncher.Core.Base;

namespace SuikaiLauncher.Core.Mod{
    public class CurseForge{
        public static string CurseForgeAPIKey = "";
        private static readonly string CurseForgeBaseAPI = "https://api.curseforge.com";
        public async static Task SearchMod(string ModName, int RequiredPerPage, int Offset, string? GameVersion, LoaderType ModLoader = LoaderType.All)
        {
            string SearchAPI = CurseForgeBaseAPI + $"/v1/mods/search?gameId=432&classId=6&index={Offset}&sortField=2&sortOrder=desc&pageSize={RequiredPerPage}&searchFilter=" + ModName;
            if (!(ModLoader == LoaderType.All)) SearchAPI += $"&modLoaderType{(int)ModLoader}";
            if (!GameVersion.IsNullOrWhiteSpaceF()) SearchAPI += $"&gameVersion={GameVersion}";
            HttpResponseMessage SearchResult = await Network.NetworkRequest(
                SearchAPI,
                new Dictionary<string, string>
                {
                    ["x-api-key"] = CurseForgeAPIKey
                }
            );
            CFSearchResult Result = Json.GetJson<CFSearchResult>(await FileIO.ReadAsString(SearchResult));
        }
        
    }
}