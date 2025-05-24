using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using SuikaiLauncher.Core.Base;
using SuikaiLauncher.Core.Override;

namespace SuikaiLauncher.Core.Subassembly.Forge{
    public class ForgeVersion
    {
        public required string MinecraftFolder;
        public required string InstallFolder;
        public required string Version;
        public required string VersionName;
        public string? InstallerUrl;
    }
    public class ForgeClient
    {
        public static string InstallerFolder = Environments.ApplicationDataPath + "/Cache/ForgeInstaller.jar";
        public async static Task ReleaseInstaller()
        {
            using (Stream? ManifestStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SuikaiLauncher.Core.Modules.Subassembly.Forge.Resources.Forge"))
            {
                if (ManifestStream is null) throw new FileNotFoundException("未能在清单内容中获取到 ForgeInstaller");
                await FileIO.WriteData(ManifestStream, InstallerFolder);
            }
        }
        public async static Task OutputLauncherProfile(ForgeVersion forgeVersion)
        {
            JsonObject Result = new()
            {
                ["SuikaiLauncher.Core"] = new JsonObject()
                {
                    ["icon"] = "Grass",
                    ["name"] = "SuikaiLauncher.Core",
                    ["latestVersionId"] = "latest-release",
                    ["type"] = "latest-release",
                    ["lastUsed"] = DateTime.Now.ToString("yyyy-MM-dd") + "T" + DateTime.Now.ToString("hh:mm:ss") + ".0000Z"
                }
                ["selectedProfile"] = "LuoTianyi",
                ["clientToken"] = "66ccff66ccff66ccff66ccff20120712"
            };
            await FileIO.WriteData(new MemoryStream(Encoding.UTF8.GetBytes(Result.ToJsonString())), forgeVersion.MinecraftFolder + "/launcher_profiles.json");
        }
        public async static Task Install(ForgeVersion forgeVersion)
        {
            if (forgeVersion.InstallerUrl.IsNullOrWhiteSpaceF()) throw new ArgumentNullException("无效的安装文件");
            
        }
    }
}