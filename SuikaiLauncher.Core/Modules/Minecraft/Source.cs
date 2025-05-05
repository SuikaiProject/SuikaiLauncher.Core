using SuikaiLauncher.Core.Minecraft;
using SuikaiLauncher.Core.Override;

namespace SuikaiLauncher.Core.Modules.Minecraft
{
    public class Source
    {
        internal static readonly Random Selector = new();
        public static bool PerferOfficial = true;
        public static string GetResourceDownloadSource(string hash)
        {
            if (PerferOfficial) return MinecraftVersionListSource.Mojang.resource + $"/{hash.Substring(0, 2)}/{hash}";
            double RandomCount = Selector.NextDouble();
            if (RandomCount < 0.5)
            {
                return MinecraftVersionListSource.BMCLAPI.resource + $"/{hash.Substring(0, 2)}/{hash}";
            }
            
            return MinecraftVersionListSource.Mojang.resource + $"/{hash.Substring(0, 2)}/{hash}";
            
        }
        public static string GetLibraryDownloadSource(string url)
        {
            if (PerferOfficial && url.ContainsF("mojang")) return url;
            double RandomCount = Selector.NextDouble();
            if (RandomCount < 0.5)
            {
                return url.
                    Replace("http:", "https:").
                    Replace("piston-meta.mojang.com", "bcmalpi2.bangbang93.com").
                    Replace("launcher.mojang.com", "bmclapi2.bangbang93.com").
                    Replace("launchermeta.mojang.com", "bmclapi2.bangbang93.com");
            }
            return url.Replace("bmclapi2.bangbang93.com", "piston-meta.mojang.com");

        }
    }
}
