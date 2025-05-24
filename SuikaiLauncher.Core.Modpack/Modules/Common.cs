using SuikaiLauncher.Core.Minecraft;
using SuikaiLauncher.Core.Override;

namespace SuikaiLauncher.Core.Modpack{
    public class Common
    {
        public static bool ShouldAddToModpack(string Input, McVersion Version)
        {
            if (Input.EndsWith($"{Version.VersionName}.jar") || Input.EndsWith($"{Version.VersionName}.json")) return false;
            if (new[] { "account", "login", "session", "user", "secret", "log" }.Any(
                s => Input.ContainsF(s)
            )) return false;
            return true;
        }
    }
}