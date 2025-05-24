#pragma warning disable CS1998

using System.Text.Json.Nodes;
using System.Text;
using SuikaiLauncher.Core.Runtime.Java;
using SuikaiLauncher.Core.Override;
using SuikaiLauncher.Core.Base;
using SuikaiLauncher.Core.Minecraft.JsonModels;

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
        public required string Version { get; set; }
        /// <summary>
        /// 版本 Json 的下载地址
        /// </summary>
        public required string JsonUrl { get; set; }
        /// <summary>
        /// 所需的 Java
        /// </summary>
        public JavaProperty? RequireJava { get; set; }
        /// <summary>
        /// 本地名称
        /// </summary>
        public required string VersionName { get; set; }
        /// <summary>
        /// 附加组件信息
        /// </summary>
        public Subassembly? SubassemblyMeta { get ; set; }
        /// <summary>
        /// 安装此版本的 Minecraft 文件夹
        /// </summary>
        public required string MinecraftFolder {get;set;}
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
        public required string InstallFolder;
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
            VersionJson VersionJson;
            List<Download.FileMetaData> DownloadList =new();
            // 基本参数检查（版本已存在或者关键参数为空或 null）
            if (Version.VersionName.IsNullOrWhiteSpaceF()) Version.VersionName = Version.Version;
            if (Directory.Exists($"{Version.MinecraftFolder}/versions/{Version.VersionName}") && File.Exists($"{Version.MinecraftFolder}/versions/{Version.VersionName}/{Version.VersionName}.json")) throw new TaskCanceledException("版本已存在");
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
            
            RawJson = await DownloadVersionJson(Version);
            VersionJson = Json.GetJson<VersionJson>(RawJson);
            if (VersionJson is null) throw new InvalidDataException();
            Tuple<List<Download.FileMetaData>,List<Download.FileMetaData>> MetaData = await GetMinecraftLib(VersionJson,Version,RawJson.ContainsF("classifiers"));
            DownloadList.AddRange(MetaData.Item1);
            DownloadList.AddRange(MetaData.Item2);
            DownloadList.AddRange(await GetMinecraftAssets(VersionJson,Version,Version.Type == VersionType.Old));
            if (!VersionJson.downloads.ToString().IsNullOrWhiteSpaceF())
            {
                DownloadList.Add(new Download.FileMetaData()
                {
                    url = VersionJson.downloads.client.url,
                    path = $"{Version.InstallFolder}/{Version.VersionName}.jar",
                    hash = VersionJson.downloads.client.sha1,
                    algorithm = "sha1",
                    size = VersionJson.downloads.client.size
                });
            }
            // Download.Start(DownloadList);
            
        }
        public async static Task<string?> DownloadVersionJson(McVersion Version,bool RequireOutputOnly = false)
        {
            Directory.CreateDirectory(Version.InstallFolder + $"{Version.VersionName}.json");
            HttpResponseMessage Result = await Network.NetworkRequest(Version.JsonUrl);
            string DlResult = await FileIO.ReadAsString(Result);
            if (!RequireOutputOnly)
            {
                    await FileIO.WriteData(new MemoryStream(Encoding.UTF8.GetBytes(DlResult)),Version.InstallFolder + $"/{Version.VersionName}.json");
            }
            return DlResult;
            
        }
        public async static Task<Tuple<List<Download.FileMetaData>,List<Download.FileMetaData>>> GetMinecraftLib(VersionJson VersionJson,McVersion Version,bool OldVersionInstallMethod = false) 
        {
            List<Download.FileMetaData> CommonLib = new();
            List<Download.FileMetaData> NativesLib = new();
            try
            {
                // 旧版本的 classifiers 键新版本没有，为了避免合并安装方法导致的 Bug 和额外工作量，所以拎出来单独写
                if (OldVersionInstallMethod)
                {
                    foreach(Downloads libaray in VersionJson.libraries)
                    {
                        var artifact = libaray.artifact;
                        var classifier = libaray.classifiers;

                        Download.FileMetaData MetaData = new()
                        {
                            url = artifact.url,
                            hash = artifact.sha1,
                            algorithm = "sha1",
                            path = $"{Version.MinecraftFolder}/libraries/{artifact.path}".Replace("\\", "/"),
                            size = artifact.size
                        };
                        if (classifier is not null){
                            // 根据不同操作系统决定要下载的支持库
                            switch (Environments.SystemType){
                                case Environments.OSType.Windows:
                                    if (Environments.SystemArch == System.Runtime.InteropServices.Architecture.X86)
                                    {
                                        Download.FileMetaData WinLibMetaData = new()
                                        {
                                            url = classifier,
                                            hash = classifier["native-windows"].sha1,
                                            size = classifier["native-windows"].size,
                                            path = $"{Version.MinecraftFolder}/libraries/{classifier["native-windows"].path}",
                                            algorithm = "sha1"
                                        };
                                        NativesLib.Add(WinLibMetaData);
                                    }
                                    break;
                                case Environments.OSType.Linux:
                                    Download.FileMetaData LnxLibMetaData = new(){
                                        url = classifier["natives-linux"].url,
                                        hash = classifier["natives-linux"].sha1,
                                        size = classifier["natives-linux"].size,
                                        path = $"{Version.MinecraftFolder}/libraries/{(string?)classifier["natives-linux"].path}",
                                        algorithm = "sha1"
                                    };
                                    NativesLib.Add(LnxLibMetaData);
                                    break;
                                case Environments.OSType.MacOS:
                                    Download.FileMetaData OsxLibMetaData = new(){
                                        url = classifier["natives-osx"].url,
                                        hash = classifier["natives-osx"].sha1,
                                        size = classifier["natives-osx"].size,
                                        path = $"{Version.MinecraftFolder}/libaraies/{classifier["natives-osx"].path}",
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
                foreach (Downloads? library in VersionJson.libraries)
                {
                    if (library is null) throw new InvalidDataException("Json 结构无效");
                    var artifact = library.artifact;
                    // natives 文件

                    // 排除架构错误的支持库
                    if (artifact.url.ContainsF("arm") && (Environments.SystemArch != System.Runtime.InteropServices.Architecture.Arm || Environments.SystemArch != System.Runtime.InteropServices.Architecture.Arm64)) continue;
                    // 根据 url 判断支持库适用的操作系统
                    if (artifact.url.ContainsF("windows") && Environments.OSType.Windows == Environments.SystemType)
                    {
                        Download.FileMetaData NativeLib = new()
                        {
                            url = artifact.url,
                            path = $"{Version.MinecraftFolder}/libraries/{artifact.path}",
                            hash = artifact.sha1,
                            size = artifact.size,
                            algorithm = "sha1"
                        };
                        NativesLib.Add(NativeLib);
                    }
                    else if (artifact.url.ContainsF("linux") && Environments.OSType.Linux == Environments.SystemType)
                    {
                        Download.FileMetaData NativeLib = new()
                        {
                            url = artifact.url,
                            path = $"{Version.MinecraftFolder}/libraries/{artifact.path}",
                            hash = artifact.sha1,
                            size = artifact.size,
                            algorithm = "sha1"
                        };
                        NativesLib.Add(NativeLib);
                    }
                    else if ((artifact.url.ContainsF("osx") || artifact.url.ContainsF("macos")) && Environments.OSType.MacOS == Environments.SystemType)
                    {
                        Download.FileMetaData NativeLib = new()
                        {
                            url = artifact.url,
                            path = $"{Version.MinecraftFolder}/libraries/{artifact.path}",
                            hash = artifact.sha1,
                            size = artifact.size,
                            algorithm = "sha1"
                        };
                        NativesLib.Add(NativeLib);
                    }
                    // 虽然不知道这是什么操作系统不过加了再说
                    else
                    {
                        Download.FileMetaData NativeLib = new()
                        {
                            url = artifact.url,
                            path = $"{Version.MinecraftFolder}/libraries/{artifact.path}",
                            hash = artifact.sha1,
                            size = artifact.size,
                            algorithm = "sha1"
                        };
                        NativesLib.Add(NativeLib);
                    }
                        CommonLib.Add(new Download.FileMetaData(){
                        url = artifact.url,
                        path = $"{Version.MinecraftFolder}/libraries/{artifact.path}",
                        hash = artifact.sha1,
                        size = artifact.size,
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
        public async static Task<List<Download.FileMetaData>> GetMinecraftAssets(VersionJson VersionJsonFile,McVersion Version,bool CopyToResource) {
            List<Download.FileMetaData> DownloadList = new();
            string? VersionAssetsUrl = VersionJsonFile.assetsIndex.url;
            long VersionAssetsSize = VersionJsonFile.assetsIndex.size;
            string? VersionAssetsHash = VersionJsonFile.assetsIndex.sha1;
            string? VersionAssetsIndex = VersionJsonFile.assetsIndex.id;
            if (VersionAssetsUrl.IsNullOrWhiteSpaceF() || VersionAssetsHash.IsNullOrWhiteSpaceF()) throw new ArgumentException("无效的安装元数据");
            string Result = await FileIO.ReadAsString((await Network.NetworkRequest(VersionAssetsUrl)));
            Objects AssetsJson = Json.GetJson<Objects>(Result);
            var AssetsObject = AssetsJson.objects;
            if (AssetsObject is not null)
            {
                foreach (var Resource in AssetsObject)
                {
                    string ResHash = Resource.Value.hash;
                    long size = Resource.Value.size;
                    DownloadList.Add(new Download.FileMetaData()
                    {
                        url = Source.GetResourceDownloadSource(ResHash),
                        path = (CopyToResource) ? $"{Version.MinecraftFolder}/resource/{Resource.Key}" : $"{Version.MinecraftFolder}/assets/{ResHash.Substring(0, 2)}/{ResHash}",
                        hash = ResHash,
                        algorithm = "sha1",
                        size = size
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