using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using SuikaiLauncher.Core.Base;
using SuikaiLauncher.Core.Override;
using SuikaiLauncher.Core.Subassembly.Forge.JsonModels;

namespace SuikaiLauncher.Core.Subassembly.Forge{
    public class ForgeVersion
    {
        public required string MinecraftFolder;
        public required string InstallFolder;
        public required string Version;
        public required string VersionName;
        public string? InstallerUrl;
        public string? InstallerPath;
        public string? JsonPath;
    }
    public class ForgeClient
    {
        public static string InstallerFolder = Environments.ApplicationDataPath + "/Cache/ForgeInstaller.jar";
        public async static Task ReleaseInstaller()
        {
            using (Stream? ManifestStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SuikaiLauncher.Core.Subassembly.Modules.Forge.Resources.ForgeInstaller.jar"))
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
                ["clientToken"] = "LuoTianyi"
            };
            await FileIO.WriteData(new MemoryStream(Encoding.UTF8.GetBytes(Result.ToJsonString())), forgeVersion.MinecraftFolder + "/launcher_profiles.json");
        }
        public async static Task Install(ForgeVersion forgeVersion)
        {
            forgeVersion.InstallerPath = forgeVersion.InstallFolder + "/installer.jar";
            forgeVersion.JsonPath = forgeVersion.InstallFolder + "/" + forgeVersion.VersionName + ".json";
            if (forgeVersion.InstallerUrl.IsNullOrWhiteSpaceF()) throw new ArgumentNullException("无效的安装文件");
            await FileIO.WriteData(await Network.NetworkRequest(forgeVersion.InstallerUrl, useBrowserUA: true), forgeVersion.InstallerPath);
            using (FileStream DataStream = new(forgeVersion.InstallFolder + "/installer.jar",FileMode.Open,FileAccess.Read,FileShare.Read,8192,true)) {
                using (ZipArchive ZipFile = new(DataStream))
                {
                    ZipArchiveEntry Entry = ZipFile.CreateEntry("version.json");
                    await FileIO.WriteData(Entry.Open(), forgeVersion.JsonPath);
                    Json.GetJson<ForgeInstallProfile>(await File.ReadAllTextAsync(forgeVersion.JsonPath));
                }
            }
        }
    }
}