using System.Text.Json.Nodes;
using System.Text;
using SuikaiLauncher.Core.Runtime.Java;
using SuikaiLauncher.Core.Override;
using SuikaiLauncher.Core.Base;

namespace SuikaiLauncher.Core.Minecraft {
    /// <summary>
    /// 加载器类型
    /// </summary>
    
    /// <summary>
    /// 版本类型
    /// </summary>
    public enum VersionType
    {
        Release = 0,
        Snapshot = 1,
        Special = 2,
        Old = 3
    }
    public class Subassembly
    {
        public string? ForgeVersion { get; set; }
        public string? FabricVersion { get; set; }
        public string? NeoForgeVersion { get; set; }
        public string? QuiltVersion { get; set; }
        public string? CleanroomVersion { get; set; }
        public string? OptiFineVersion { get; set; }
        public string? LiteLoaderVersion { get; set; }
    }
     
    /// <summary>
    /// 版本源数据
    /// </summary>
    public class McVersion
    {
        /// <summary>
        /// Minecraft 版本
        /// </summary>
        public string? Version { get; set; }
        /// <summary>
        /// 版本 Json 的下载地址
        /// </summary>
        public string? JsonUrl { get; set; }
        /// <summary>
        /// 所需的 Java
        /// </summary>
        public JavaProperty? RequireJava { get; set; }
        /// <summary>
        /// 本地名称
        /// </summary>
        public string? VersionName { get; set; }
        /// <summary>
        /// 附加组件信息
        /// </summary>
        public Subassembly? SubassemblyMeta { get ; set; }
        /// <summary>
        /// 安装此版本的 Minecraft 文件夹
        /// </summary>
        public string? MinecraftFolder {get;set;}
        /// <summary>
        /// 版本 Json 文件 Hash
        /// </summary>
        public string? JsonHash { get; set; }
        /// <summary>
        /// 版本发行时间
        /// </summary>
        public DateTime? ReleaseTime { get; set; }

        public VersionType Type { get; set; }
        /// <summary>
        /// 安装目录
        /// </summary>
        public string? InstallFolder;
        /// <summary>
        /// 根据版本号查找对应的版本
        /// </summary>
        /// <returns>一个 bool 用于指示是否查找到对应版本</returns>
        public bool Lookup()
        {
            return false;
        }
        /// <summary>
        /// 根据版本名称查找本地版本
        /// </summary>
        /// <returns>一个 bool 用于指示是否在本地找到此版本</returns>
        public bool LocalLookup(){
            return false;
        }

    }
    /// <summary>
    /// 安装客户端
    /// </summary>
    public class Client
    {
        
        public async static void InstallRequest(McVersion Version)
        {
            string? RawJson;
            JsonNode? VersionJson;
            List<Download.FileMetaData> DownloadList =new();
            // 基本参数检查（版本已存在或者关键参数为空或 null）
            if (Version.VersionName.IsNullOrWhiteSpaceF()) Version.VersionName = Version.Version;
            if (Directory.Exists($"{Version.MinecraftFolder}/versions/{Version.VersionName}") && File.Exists($"{Version.MinecraftFolder}/versions/{Version.VersionName}/{Version.VersionName}.json")) throw new OperationCanceledException("版本已存在");
            if (Version.MinecraftFolder.IsNullOrWhiteSpaceF() || Version.JsonUrl.IsNullOrWhiteSpaceF()) throw new NullReferenceException("指定参数中有一项或多项为空或 null");
            if (Version.MinecraftFolder.EndsWith("/")) Version.MinecraftFolder = Version.MinecraftFolder.TrimEnd('/');
            Logger.Log($"[Minecraft] 开始安装 Minecraft {Version.Version}");
            Logger.Log("[Minecraft] ========== 核心元数据 ==========");
            Logger.Log($"[Minecraft] 核心名称：{Version.VersionName}");
            Logger.Log($"[Minecraft] 版本：{Version.Version}");
            Logger.Log($"[Minecraft] 版本类型：{Enum.GetName(Version.Type)}");
            Logger.Log($"[Minecraft] 可安装 Mod： {Version.SubassemblyMeta is not null}");
            Logger.Log($"[Minecraft] 要求的 Java 版本： {( (Version.RequireJava is not null) ? $"Java {Version.RequireJava.MojarVersion}":"未指定" )}");
            Logger.Log($"[Minecraft] 安装路径：{Version.InstallFolder}");
            Logger.Log("[Minecraft] ===========================");
            
            RawJson = await DownloadVersionJson(Version,$"{Version.MinecraftFolder}/versions/{Version.VersionName}/{Version.VersionName}.json");
            VersionJson = Json.GetJson(RawJson);
            if (VersionJson is null) throw new InvalidDataException();
            Tuple<List<Download.FileMetaData>,List<Download.FileMetaData>> MetaData = await GetMinecraftLib(VersionJson,Version,RawJson.ContainsF("classifiers"));
            DownloadList.AddRange(MetaData.Item1);
            DownloadList.AddRange(MetaData.Item2);
            DownloadList.AddRange(await GetMinecraftAssets(VersionJson,Version,Version.Type == VersionType.Old));
            if (!VersionJson["downloads"].ToString().IsNullOrWhiteSpaceF())
            {
                DownloadList.Add(new Download.FileMetaData()
                {
                    url = VersionJson["downloads"]?["client"]?["url"]?.ToString(),
                    path = $"{Version.InstallFolder}/{Version.VersionName}.jar",
                    hash = VersionJson["downloads"]?["client"]?["sha1"]?.ToString(),
                    algorithm = "sha1",
                    size = (long?)VersionJson["downloads"]?["client"]?["size"]
                });
            }
            // Download.Start(DownloadList);
            
        }
        public async static Task<string>? DownloadVersionJson(McVersion version, string path,bool RequireOutputOnly = false)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                
                string Result = await Download.NetGetFileByClient(new Download.FileMetaData(){
                    url = version.JsonUrl,
                    hash =  version.JsonHash
                });
                if (Result != null)
                {
                    
                    if (!RequireOutputOnly)
                    {
                        File.WriteAllBytes(path,Encoding.UTF8.GetBytes(Result));
                    }
                    return Result;
                }
                else
                {
                    throw new ArgumentException("下载文件失败");
                }
            }
            throw new FileNotFoundException("指定的路径已存在");
        }
        public async static Task<Tuple<List<Download.FileMetaData>,List<Download.FileMetaData>>> GetMinecraftLib(JsonNode VersionJson,McVersion Version,bool OldVersionInstallMethod = false) 
        {
            List<Download.FileMetaData> CommonLib = new();
            List<Download.FileMetaData> NativesLib = new();
            try
            {
                // 旧版本的 classifiers 键新版本没有，为了避免合并安装方法导致的 Bug 和额外工作量，所以拎出来单独写
                if (OldVersionInstallMethod)
                {
                    foreach(JsonNode libaray in VersionJson["libraries"]?.GetValue<List<JsonNode>>())
                    {
                        if (libaray is null) throw new InvalidDataException("Json 结构无效");
                        JsonNode? artifact = libaray["downloads"]?["artifact"];
                        JsonNode? classifier = libaray["downloads"]?["classifiers"];
                        if (artifact is not null) {
                            Download.FileMetaData MetaData = new(){
                                url = (string?)artifact["url"],
                                hash = (string?)artifact["sha1"],
                                algorithm = "sha1",
                                path = $"{Version.MinecraftFolder}/libraries/{artifact["path"]}",
                                size = (long?)artifact["size"]
                            };
                            CommonLib.Add(MetaData);
                        }
                        if (classifier is not null){
                            // 根据不同操作系统决定要下载的支持库
                            switch (Environments.SystemType){
                                case Environments.OSType.Windows:
                                    Download.FileMetaData WinLibMetaData = new(){
                                        url = (string?)classifier["natives-windows"]?["url"],
                                        hash = (string?)classifier["natives-windows"]?["sha1"],
                                        size = (long?)classifier["natives-windows"]?["size"],
                                        path = $"{Version.MinecraftFolder}/libraries/{(string?)classifier["natives-windows"]?["path"]}",
                                        algorithm = "sha1"
                                    };
                                    NativesLib.Add(WinLibMetaData);
                                    break;
                                case Environments.OSType.Linux:
                                    Download.FileMetaData LnxLibMetaData = new(){
                                        url = (string?)classifier["natives-linux"]?["url"],
                                        hash = (string?)classifier["natives-linux"]?["sha1"],
                                        size = (long?)classifier["natives-linux"]?["size"],
                                        path = $"{Version.MinecraftFolder}/libraries/{(string?)classifier["natives-linux"]?["path"]}",
                                        algorithm = "sha1"
                                    };
                                    NativesLib.Add(LnxLibMetaData);
                                    break;
                                case Environments.OSType.MacOS:
                                    Download.FileMetaData OsxLibMetaData = new(){
                                        url = (string?)classifier["natives-osx"]?["url"],
                                        hash = (string?)classifier["natives-osx"]?["sha1"],
                                        size = (long?)classifier["natives-osx"]?["size"],
                                        path = $"{Version.MinecraftFolder}/libaraies/{(string?)classifier["natives-osx"]?["path"]}",
                                        algorithm = "sha1"
                                    };
                                    NativesLib.Add(OsxLibMetaData);
                                    break;
                                    // FreeBSD 以及其它操作系统
                                    // 根本不知道要下什么库
                                case Environments.OSType.FreeBSD | Environments.OSType.Unknown:
                                    throw new NotImplementedException("暂不支持此平台");
                            }
                        }

                    }
                    return Tuple.Create(CommonLib,NativesLib);
                }
                // 新版本安装方法
                foreach (JsonNode? library in VersionJson["libraries"].GetValue<List<JsonNode>>()){
                    if (library is null) throw new InvalidDataException("Json 结构无效");
                    JsonNode? artifact = library["downloads"];
                    // natives 文件
                    if (artifact is not null && artifact["url"] is not null && artifact["url"].ToString().ContainsF("native")){
                        // 排除架构错误的支持库
                        if (artifact["url"].ToString().ContainsF("arm") && (Environments.SystemArch != System.Runtime.InteropServices.Architecture.Arm || Environments.SystemArch != System.Runtime.InteropServices.Architecture.Arm64)) continue;
                        // 根据 url 判断支持库适用的操作系统
                        if (artifact["url"].ToString().ContainsF("windows") && Environments.OSType.Windows == Environments.SystemType){
                            Download.FileMetaData NativeLib = new(){
                                url = (string?)artifact["url"],
                                path = $"{Version.MinecraftFolder}/libraries/{(string?)artifact["path"]}",
                                hash = (string?)artifact["sha1"],
                                size = (long?)artifact["size"],
                                algorithm = "sha1"
                            };
                            NativesLib.Add(NativeLib);
                        }
                        else if (artifact["url"].ToString().ContainsF("linux") && Environments.OSType.Linux == Environments.SystemType){
                            Download.FileMetaData NativeLib = new(){
                                url = (string?)artifact["url"],
                                path = $"{Version.MinecraftFolder}/libraries/{(string?)artifact["path"]}",
                                hash = (string?)artifact["sha1"],
                                size = (long?)artifact["size"],
                                algorithm = "sha1"
                            };
                            NativesLib.Add(NativeLib);
                        }
                        else if ((artifact["url"].ToString().ContainsF("osx") ||artifact["url"].ToString().ContainsF("macos")) && Environments.OSType.MacOS == Environments.SystemType){
                            Download.FileMetaData NativeLib = new(){
                                url = (string?)artifact["url"],
                                path = $"{Version.MinecraftFolder}/libraries/{(string?)artifact["path"]}",
                                hash = (string?)artifact["sha1"],
                                size = (long?)artifact["size"],
                                algorithm = "sha1"
                            };
                            NativesLib.Add(NativeLib);
                        }
                        // 虽然不知道这是什么操作系统不过加了再说
                        else {
                            Download.FileMetaData NativeLib = new(){
                                url = (string?)artifact["url"],
                                path = $"{Version.MinecraftFolder}/libraries/{(string?)artifact["path"]}",
                                hash = (string?)artifact["sha1"],
                                size = (long?)artifact["size"],
                                algorithm = "sha1"
                            };
                            NativesLib.Add(NativeLib);
                        }
                    }
                    CommonLib.Add(new Download.FileMetaData(){
                        url = (string?)artifact["url"],
                        path = $"{Version.MinecraftFolder}/libraries/{(string?)artifact["path"]}",
                        hash = (string?)artifact["sha1"],
                        size = (long?)artifact["size"],
                        algorithm = "sha1"
                    });
                }
                return Tuple.Create(NativesLib,CommonLib);
            }
            catch (TaskCanceledException)
            {
                Logger.Log("安装操作已取消");
            }
            catch(Exception ex)
            {
                Logger.Log(ex, "获取支持库列表失败");
                throw;
            }
            
                throw new Exception("未知错误");
        }
        public async static Task<List<Download.FileMetaData>> GetMinecraftAssets(JsonNode VersionJson,McVersion Version,bool CopyToResource) {
            List<Download.FileMetaData> DownloadList = new();
            string? VersionAssetsUrl = VersionJson["assetsIndex"]?["url"]?.ToString();
            int? VersionAssetsSize = (int?)VersionJson["assetsIndex"]?["size"];
            string? VersionAssetsHash = VersionJson["assetsIndex"]?["sha1"]?.ToString();
            string? VersionAssetsIndex = VersionJson["assetsIndex"]?["id"]?.ToString();
            if (VersionAssetsUrl.IsNullOrWhiteSpaceF() || VersionAssetsSize is not null || VersionAssetsHash.IsNullOrWhiteSpaceF()) throw new ArgumentException("无效的安装元数据");
            string Result = await Download.NetGetFileByClient(new Download.FileMetaData()
            {
                url = VersionAssetsUrl,
                size = VersionAssetsSize,
                hash = VersionAssetsHash,
                algorithm = "sha1",
                path = $"{Version.MinecraftFolder}/assets/indexes/{VersionAssetsIndex}.json"
            });
            JsonNode? AssetsJson = Json.GetJson(Result);
            JsonNode? AssetsObject = AssetsJson?["objects"];
            if (AssetsObject is not null)
            {
                foreach (var Resource in AssetsObject as JsonObject)
                {
                    string ResHash = Resource.Value["sha1"]?.ToString();
                    DownloadList.Add(new Download.FileMetaData()
                    {
                        url = Source.GetResourceDownloadSource(Resource.Value["sha1"]?.ToString()),
                        path = (CopyToResource) ? $"{Version.MinecraftFolder}/resource/{Resource.Key}" : $"{Version.MinecraftFolder}/assets/{ResHash.Substring(0, 2)}/{ResHash}",
                        hash = ResHash,
                        algorithm = "sha1",
                        size = (long?)Resource.Value["size"]
                    });
                }
                return DownloadList;
            }
            throw new ArgumentException("版本 Json 文件存在问题，无法安装！");
        }
            
    }
    public class Server
    {
        public async static Task DownloadServerCore(string Version)
        {
            
        }
    }
}