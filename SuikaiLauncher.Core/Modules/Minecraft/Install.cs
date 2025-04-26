using System.Text.Json.Nodes;
using SuikaiLauncher.Core.Base.Tasks;
using SuikaiLauncher.Core.Base.ThreadSafe;
using System.Text;
using SuikaiLauncher.Core.Runtime.Java;
using SuikaiLauncher.Core.Override;
using Exceptions;

namespace SuikaiLauncher.Core.Minecraft {
    /// <summary>
    /// 加载器类型
    /// </summary>
    public enum ModLoaderType{
        Unavailable = 0,
        Forge = 1,
        Fabric = 2,
        OptiFine = 3,
        NeoForge = 4,
        Quilt = 5,
        LiteLoader = 6,
        Mixin = 7
    }
    /// <summary>
    /// 版本类型
    /// </summary>
    public enum VersionType{

    }
    /// <summary>
    /// 版本源数据
    /// </summary>
    public class McVersion
    {
        public string Version { get; set; }
        public string JsonUrl { get; set; }
        public JavaProperty RequireJava { get; set; }
        public bool Modable;
        public ModLoaderType ModLoader;

        public bool Lookup()
        {
            return false;
        }

    }
    /// <summary>
    /// 安装客户端
    /// </summary>
    public class Client
    {
        private static LoaderTask InstallTask = new()
        {
            Expired = 0,
        };
        private static async Task<TSafeDictionary<string,object>> InstallFunction(TSafeDictionary<string,object> Input)
        {
            object InstallLock = new object[1];
            object? Inp = null;
            Input.TryGetValue("McVersion",out Inp);
            McVersion? MinecraftVersion = Inp as McVersion;
            JsonNode VersionJson;
            string RawJson = DownloadVersionJson(MinecraftVersion,"").Result;
            VersionJson = Json.GetJson(RawJson);
            await GetMinecraftLib(VersionJson,RawJson.ContainsF("classifier"));
            await GetMinecraftAssets(VersionJson);
            return new TSafeDictionary<string, object>();
        }
        public static void InstallRequest(McVersion Version)
        {
            TSafeDictionary<string, object> Input = new TSafeDictionary<string, object>();
            Input.Set("RequireVersion", Version);
            InstallTask.Input = Input;
            InstallTask.Startup(async(TSafeDictionary<string,object> Input) => { return await InstallFunction(Input); });
            if (InstallTask.Status == LoaderState.Complete) InstallTask = null;
        }
        public async static Task<string>? DownloadVersionJson(McVersion version, string path,bool RequireOutputOnly = false)
        {
            if (Directory.Exists(path))
            {
                byte[] Result = await Download.NetGetFileByClient(version.JsonUrl);
                if (Result != null)
                {
                    string RawJson = Encoding.UTF8.GetString(Result);
                    if (!RequireOutputOnly)
                    {

                    }
                    return RawJson;
                }
            }
            throw new FileNotFoundException("指定的路径不存在");
        }
        public async static Task GetMinecraftLib(JsonNode VersionJson,bool OldVersionInstallMethod = false) 
        {
            try
            {
                List<TSafeDictionary<string,object>> CommonLib = new();
                List<TSafeDictionary<string, object>> NativesLib = new();
                ;
                // 旧版本安装方法
                if (OldVersionInstallMethod)
                {
                    foreach(var libaray in VersionJson["libraries"]?.GetValue<List<JsonNode>>())
                    {
                        if (libaray is null) throw new InvalidDataException("Json 数据无效");
                        JsonNode? artifact = libaray["artifact"];
                        JsonNode? classifier = libaray["classifiers"];
                        if (artifact is not null) {
                            TSafeDictionary<string, object> FileMeta = new();
                            FileMeta.Set("path","");
                        }
                    }
                }
                // 新版本安装方法
            }
            catch (TaskCanceledException)
            {
                Logger.Log("安装操作已取消");
            }catch(Exception ex)
            {
                Logger.Log(ex, "获取支持库列表失败");
            }
        }
        public async static Task GetMinecraftAssets(JsonNode VersionJson) { }

    }
    public class Server
    {
        public async static Task DownloadServerCore(string Version)
        {
            
        }
    }
}