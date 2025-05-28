using System.Reflection;
using SuikaiLauncher.Core.Base;
using SuikaiLauncher.Core.Override;

namespace SuikaiLauncher.Core.Mod{
    public enum ModSource{
        CurseForge = 0,
        Modrinth = 1,
        Mixin = 2
    }
    public enum LoaderType
    {
        All = 0,
        Forge = 1,
        LiteLoader = 3,
        Fabric = 4,
        Quilt = 5,
        NeoForge = 6
    }
    public class ModMetaData
    {
        public string ModRealName = "";
        public int ModWikiId;
        public string? CFProjectSlug;
        public string? MRProjectSlug;

        public string? ModTranslateName;
        public ModSource Source;
    }

    public class ModData
    {
        public static Dictionary<string, int> ModInfomationMapping = new();
        public static List<ModMetaData> ModTranslate = new();
        public async static Task<List<string>> GetModSearchKey(string Input)
        {
            try
            {
                List<string> SearchKey = new();
                if (ModTranslate is null) await LoadModData();

                foreach (var name in ModTranslate!)
                {
                    if (name.ModTranslateName.IsNullOrWhiteSpaceF() && name.ModTranslateName!.ContainsF(Input)) SearchKey.Add(name.ModRealName);
                }
                return SearchKey;
            }
            catch (Exception ex)
            {
                Logger.Log(ex, "获取工程列表搜索文本失败");
                throw;

            }
        }
        public async static Task LoadModData()
        {
            if (ModTranslate.Count <= 0) return;
            var Data = Assembly.GetExecutingAssembly();
            string ModDataBase = "";
            using (Stream? DataStream = Data.GetManifestResourceStream("SuikaiLauncher.Core.Modules.Mod.Resources.ModData.txt"))
            {
                if (DataStream is null) throw new ArgumentException("无法加载 ModData");
                ModDataBase = await FileIO.ReadAsString(DataStream);
            }
            int WikiId = 0,ModInfoId = 0;
            foreach (var TranslateData in ModDataBase.Replace("\r", "").Split("\n"))
            {
                WikiId++;
                string Translate = TranslateData.ToString();
                if (Translate.IsNullOrWhiteSpaceF()) continue;
                string[] SplitedLine = Translate.Split("|");
                ModMetaData ModInfomation = new();
                ModInfomation.ModWikiId = WikiId;
                if (SplitedLine[0].StartsWith("@"))
                {
                    ModInfomation.Source = ModSource.Modrinth;
                    ModInfomation.MRProjectSlug = SplitedLine[0].TrimStart('@');
                }
                else if (SplitedLine[0].EndsWith("@"))
                {
                    ModInfomation.Source = ModSource.Mixin;
                    ModInfomation.CFProjectSlug = ModInfomation.MRProjectSlug = SplitedLine[0].TrimEnd('@');

                }
                else if (SplitedLine[0].ContainsF("@"))
                {
                    ModInfomation.Source = ModSource.Mixin;
                    ModInfomation.CFProjectSlug = SplitedLine[0].Split("@")[0];
                    ModInfomation.MRProjectSlug = SplitedLine[0].Split("@")[1];

                }
                else
                {
                    ModInfomation.Source = ModSource.CurseForge;
                    ModInfomation.CFProjectSlug = SplitedLine[0].Split("@")[0];
                }
                if (SplitedLine.Count() >= 2)
                {
                    ModInfomation.ModTranslateName = SplitedLine[1].Replace("*", "");
                }
                ModInfomation.ModRealName = (ModInfomation.CFProjectSlug is not null) ? ModInfomation.CFProjectSlug.Replace("-", " ").Capitalize() : ModInfomation.MRProjectSlug!.Capitalize();
                string ModProjectSlug = (ModInfomation.CFProjectSlug is not null) ? ModInfomation.CFProjectSlug! : ModInfomation.MRProjectSlug!;
                ModInfomationMapping[ModProjectSlug] = ModInfoId;
                ModInfoId++;
                ModTranslate.Add(ModInfomation);
            }
        }
        public static string? GetModTranslateName(string ModSlug)
        {
            try
            {
                return ModTranslate[ModInfomationMapping[ModSlug]].ModTranslateName;
            }
            catch (Exception ex)
            {
                Logger.Log(ex, "[Mod] 获取中文译名失败");
                return string.Empty;
            }
        }

    }
}