using SuikaiLauncher.Core.Base;
using SuikaiLauncher.Core.Minecraft;
using SuikaiLauncher.Core.Override;
using System.IO.Compression;
using System.Text;
using System.Text.Json.Nodes;

namespace SuikaiLauncher.Core.Modpack
{
    public class MCBBSModpack
    {
        public string? OutputFolder;
        public string? Name;
        public string? Version;
        public string? Author;
        public McVersion? Game;
        public string? Description;
        public string? url;
        public int? MinMemory;
        public List<string>? LaunchArgument;
        public List<string>? JvmArgument;

        public async Task<bool> ExportModpack()
        {
            if (this.Game is null) throw new ArgumentNullException("指定的参数无效\n参数名：Game");

            bool Modable = Directory.EnumerateFiles(Game.InstallFolder + "/mods").Count() > 0;
            bool Resourcepack = Directory.EnumerateFiles(Game.InstallFolder + "/resourcepack").Count() > 0;
            using (FileStream Zip = new(this.Name + ".zip", FileMode.CreateNew))
            {
                using (ZipArchive archive = new(Zip, ZipArchiveMode.Create))
                {

                    JsonObject Packmeta = new()
                    {
                        ["manifestType"] = "minecraftModpack",
                        ["manifestVersion"] = 2,
                        ["name"] = this.Name,
                        ["version"] = this.Version,
                        ["description"] = this.Description?.ToString(),
                        ["fileApi"] = "",
                        ["url"] = this.url?.ToString(),
                        ["forceUpdate"] = false,
                        ["origin"] = new JsonArray(),
                    };
                    JsonArray Addons = new();
                    Addons.Add(new JsonObject()
                    {
                        ["id"] = "game",
                        ["version"] = this.Game.Version
                    });
                    Packmeta.Add("addons", Addons);
                    JsonArray libraries = new();
                    Packmeta.Add("libraries", libraries);
                    JsonArray array = new();
                    foreach (var Item in Directory.EnumerateFiles(this.Game.InstallFolder))
                    {
                        if (!Common.ShouldAddToModpack(Item, this.Game)) continue;
                        string hash = await FileIO.GetFileHashAsync(Item);
                        array.Add(new JsonObject()
                        {
                            ["path"] = Item.Replace(this.Game.InstallFolder + "/", ""),
                            ["hash"] = hash,
                            ["force"] = true,
                            ["type"] = "addon"
                        });
                        ZipArchiveEntry FileEntry = archive.CreateEntry(Item.Replace(this.Game.InstallFolder.TrimEnd('/') + "/", ""));
                        using (Stream FileEntryStream = FileEntry.Open())
                        {
                            using (FileStream WriteFileStream = new(Item, FileMode.Open, FileAccess.Read, FileShare.Read, 16384, true))
                            {
                                await WriteFileStream.CopyToAsync(FileEntryStream);
                            }
                        }
                    }
                    Packmeta.Add("files", array);
                    Packmeta.Add("settings", new JsonObject()
                    {
                        ["install_mod"] = Modable,
                        ["install_resourcepack"] = Resourcepack
                    });
                    Packmeta.Add("launchInfo", new JsonObject()
                    {
                        ["minMemory"] = this.MinMemory,
                        ["launchArgument"] = new JsonArray() { this.LaunchArgument },
                        ["javaArgument"] = new JsonArray() { this.JvmArgument }
                    });
                    ZipArchiveEntry Entry = archive.CreateEntry("mcbbs.packmeta");
                    using (Stream EntryStream = Entry.Open())
                    {
                        using (MemoryStream DataStream = new(Encoding.UTF8.GetBytes(Packmeta.ToJsonString())))
                        {
                            await DataStream.CopyToAsync(EntryStream);
                        }
                    }
                    return true;
                }
            }
        }
        public async Task<bool> ImportModpack()
        {
            return false;
        }

    }
}