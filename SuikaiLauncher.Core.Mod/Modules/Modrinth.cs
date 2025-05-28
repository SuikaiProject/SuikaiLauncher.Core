using System.ComponentModel.DataAnnotations;
using SuikaiLauncher.Core.Base;
using SuikaiLauncher.Core.Mod.JsonModels;
using SuikaiLauncher.Core.Override;

namespace SuikaiLauncher.Core.Mod{

    public class Modrinth
    {

        private static string ModrinthBaseAPI = "https://api.modrinth.com/v2";
        public static async Task<ModrinthSearchResult> SearchMod(string ModName, string? ModLoader, string? McVersion)
        {
            string SearchAPI = ModrinthBaseAPI + $"/search?query={ModName}&limit=100&facets=[[\"project_type:mod\"]";
            if (!ModLoader.IsNullOrWhiteSpaceF()) SearchAPI += $"[\"categories:{ModLoader}\"";
            if (!McVersion.IsNullOrWhiteSpaceF()) SearchAPI += $"[\"versions:{McVersion}\"]";
            SearchAPI += "]";
            HttpResponseMessage SearchResult = await Network.NetworkRequest(SearchAPI);
            ModrinthSearchResult Result = Json.GetJson<ModrinthSearchResult>(await FileIO.ReadAsString(SearchResult));
            int SearchCount = 0;
            foreach (var Project in Result.hits)
            {
                if (Project is null) continue;
                Project.ModI18nzhName = ModData.GetModTranslateName(Project.slug);
                Result.hits[SearchCount] = Project;
            }
            return Result;
        }
        
    }
}