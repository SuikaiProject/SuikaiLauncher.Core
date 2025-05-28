#pragma warning disable CS1998

using System.Text.Json.Nodes;
using System.Text;
using SuikaiLauncher.Core.Runtime.Java;
using SuikaiLauncher.Core.Override;
using SuikaiLauncher.Core.Base;
using SuikaiLauncher.Core.Minecraft.JsonModels;
using SuikaiLauncher.Core.Time;

namespace SuikaiLauncher.Core.Minecraft
{
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
        public Subassembly? SubassemblyMeta { get; set; }
        /// <summary>
        /// 安装此版本的 Minecraft 文件夹
        /// </summary>
        public required string MinecraftFolder { get; set; }
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
        public static McVersion Lookup()
        {
            return new McVersion()
            {
                Version = "",
                VersionName = "",
                JsonUrl = "",
                MinecraftFolder = "",
                InstallFolder = ""
            };
        }
        /// <summary>
        /// 根据版本名称查找本地版本
        /// </summary>
        /// <returns>McVersion</returns>
        public static McVersion LocalLookup()
        {
            return new McVersion()
            {
                Version = "",
                VersionName = "",
                JsonUrl = "",
                MinecraftFolder = "",
                InstallFolder = ""
            };
        }
        public void CorrectionPath()
        {
            this.MinecraftFolder = this.MinecraftFolder.Replace("\\", "/");
            this.InstallFolder = this.InstallFolder.Replace("\\", "/");
        }

    }
    /// <summary>
    /// 安装客户端
    /// </summary>
    public class Client
    {
        /// <summary>
        /// 安装请求，用于下载和安装 Minecraft 客户端。
        /// </summary>
        /// <param name="Version">要安装的 Minecraft 版本信息。</param>
        /// <exception cref="TaskCanceledException">当版本已存在时抛出。</exception>
        /// <exception cref="InvalidDataException">当版本 Json 结构无效时抛出。</exception>
        public async static Task Install(McVersion Version)
        {
            var t1 = Time.Time.getCurrentTime();
            // 基本参数检查
            if (Directory.Exists(Version.InstallFolder) && File.Exists($"{Version.InstallFolder}/{Version.VersionName}.json"))
            {
                throw new TaskCanceledException("版本已存在");
            }
            Version.CorrectionPath();
            if (Version.MinecraftFolder.EndsWith("/"))
            {
                Version.MinecraftFolder = Version.MinecraftFolder.TrimEnd('/');
            }
            Logger.Log($"[Minecraft] 开始安装 Minecraft {Version.Version}");
            Logger.Log("[Minecraft] ========== 核心元数据 ==========");
            Logger.Log($"[Minecraft] 核心名称：{Version.VersionName}");
            Logger.Log($"[Minecraft] 版本：{Version.Version}");
            Logger.Log($"[Minecraft] 版本类型：{Enum.GetName(Version.Type)}");
            Logger.Log($"[Minecraft] 可安装 Mod： {(Version.SubassemblyMeta is not null ? "是" : "否")}");
            Logger.Log($"[Minecraft] 要求的 Java 版本： {((Version.RequireJava is not null) ? $"Java {Version.RequireJava.MojarVersion}" : "未指定")}");
            Logger.Log($"[Minecraft] 安装路径：{Version.InstallFolder}");
            Logger.Log("[Minecraft] ================================");

            string rawJson = await DownloadVersionJson(Version);



            VersionJson versionJson = Json.GetJson<VersionJson>(rawJson);
            if (versionJson is null)
            {
                throw new InvalidDataException("版本 Json 结构无效");
            }

            List<Download.FileMetaData> downloadList = new();

            // 获取 Minecraft 库文件下载列表
            Tuple<List<Download.FileMetaData>, List<Download.FileMetaData>> libMetaData = await GetMinecraftLib(versionJson, Version, rawJson.ContainsF("classifiers"));
            downloadList.AddRange(libMetaData.Item1); // Natives libraries
            downloadList.AddRange(libMetaData.Item2); // Common libraries

            // 获取 Minecraft 资源文件下载列表
            downloadList.AddRange(await GetMinecraftAssets(versionJson, Version, Version.Type == VersionType.Old));

            // 添加客户端 Jar 文件下载
            if (!versionJson.downloads.ToString().IsNullOrWhiteSpaceF() && versionJson.downloads["client"] != null)
            {
                downloadList.Add(new Download.FileMetaData()
                {
                    url = versionJson.downloads["client"].url,
                    path = $"{Version.InstallFolder}/{Version.VersionName}.jar",
                    hash = versionJson.downloads["client"].sha1,
                    algorithm = "sha1",
                    size = versionJson.downloads["client"].size
                });
            }

            await Download.NetCopyFileAsync(downloadList);
            Logger.Log($"[Minecraft] 安装结束：耗时 {Time.Time.getCurrentTime() - t1}");
        }

        /// <summary>
        /// 下载版本 Json 文件。
        /// </summary>
        /// <param name="Version">Minecraft 版本信息。</param>
        /// <param name="RequireOutputOnly">是否只返回 Json 内容而不写入文件。</param>
        /// <returns>下载的 Json 字符串内容。</returns>
        public async static Task<string> DownloadVersionJson(McVersion Version, bool RequireOutputOnly = false)
        {
            Directory.CreateDirectory(Version.InstallFolder);
            HttpResponseMessage result = await Network.NetworkRequest(Version.JsonUrl);
            string dlResult = await FileIO.ReadAsString(result);

            if (!RequireOutputOnly)
            {
                await FileIO.WriteData(new MemoryStream(Encoding.UTF8.GetBytes(dlResult)), $"{Version.InstallFolder}/{Version.VersionName}.json", false);
            }
            return dlResult;
        }

        /// <summary>
        /// 获取 Minecraft 库文件下载列表。
        /// </summary>
        /// <param name="VersionJson">版本 Json 对象。</param>
        /// <param name="Version">Minecraft 版本信息。</param>
        /// <param name="OldVersionInstallMethod">是否使用旧版本安装方法（基于 classifiers 字段）。</param>
        /// <returns>包含 CommonLib 和 NativesLib 的元数据列表的元组。</returns>
        /// <exception cref="InvalidDataException">当 Json 结构无效时抛出。</exception>
        /// <exception cref="NotImplementedException">当遇到不支持的平台时抛出。</exception>
        /// <exception cref="Exception">其他获取支持库列表失败的异常。</exception>
        public async static Task<Tuple<List<Download.FileMetaData>, List<Download.FileMetaData>>> GetMinecraftLib(VersionJson VersionJson, McVersion Version, bool OldVersionInstallMethod = false)
        {
        
            List<Download.FileMetaData> commonLib = new();
            List<Download.FileMetaData> nativesLib = new();
            try
            {
                if (OldVersionInstallMethod)
                {
                    if (VersionJson is null) Logger.Crash();
                    foreach (var library in VersionJson!.libraries)
                    {
                        Logger.Log((library is null).ToString());
                        Logger.Log((library is null).ToString());
                        if (library?.downloads.artifact is null) continue; // 确保artifact存在

                        var artifact = library.downloads.artifact;
                        var classifiers = library.downloads.classifiers;

                        commonLib.Add(new Download.FileMetaData()
                        {
                            url = artifact.url,
                            hash = artifact.sha1,
                            algorithm = "sha1",
                            path = $"{Version.MinecraftFolder}/libraries/{artifact.path}".Replace("\\", "/"),
                            size = artifact.size
                        });

                        if (classifiers != null)
                        {
                            // 根据不同操作系统决定要下载的支持库
                            string? nativeUrl = null;
                            string? nativeHash = null;
                            long nativeSize = 0;
                            string? nativePath = null;

                            switch (Environments.SystemType)
                            {
                                case Environments.OSType.Windows:


                                    if (classifiers.ToString().ContainsF("natives-windows"))
                                    {
                                        if (classifiers.ContainsKey("natives-windows-32") && Environments.SystemArch == System.Runtime.InteropServices.Architecture.X86)
                                        {
                                            var nativeWin = classifiers["natives-windows-32"];
                                            nativeUrl = nativeWin.url;
                                            nativeHash = nativeWin.sha1;
                                            nativeSize = nativeWin.size;
                                            nativePath = nativeWin.path;

                                        }
                                        else if (classifiers.ContainsKey("natives-windows-64") && Environments.SystemArch == System.Runtime.InteropServices.Architecture.X64)
                                        {
                                            var nativeWin = classifiers["natives-windows-64"];
                                            nativeUrl = nativeWin.url;
                                            nativeHash = nativeWin.sha1;
                                            nativeSize = nativeWin.size;
                                            nativePath = nativeWin.path;
                                        }
                                        else
                                        {
                                            var nativeWin = classifiers["natives-windows"];
                                            nativeUrl = nativeWin.url;
                                            nativeHash = nativeWin.sha1;
                                            nativeSize = nativeWin.size;
                                            nativePath = nativeWin.path;
                                        }
                                            
                                    }
                                    
                                    break;
                                case Environments.OSType.Linux:
                                    if (classifiers.ContainsKey("natives-linux"))
                                    {
                                        var nativeLnx = classifiers["natives-linux"];
                                        nativeUrl = nativeLnx.url;
                                        nativeHash = nativeLnx.sha1;
                                        nativeSize = nativeLnx.size;
                                        nativePath = nativeLnx.path;
                                    }
                                    break;
                                case Environments.OSType.MacOS:
                                    if (classifiers.ContainsKey("natives-osx"))
                                    {
                                        var nativeOsx = classifiers["natives-osx"];
                                        nativeUrl = nativeOsx.url;
                                        nativeHash = nativeOsx.sha1;
                                        nativeSize = nativeOsx.size;
                                        nativePath = nativeOsx.path;
                                    }
                                    break;
                                // FreeBSD 以及其它操作系统
                                case Environments.OSType.FreeBSD | Environments.OSType.Unknown:
                                    throw new NotImplementedException("暂不支持此平台");
                            }

                            if (nativeUrl != null && nativeHash != null && nativePath != null)
                            {
                                nativesLib.Add(new Download.FileMetaData()
                                {
                                    url = nativeUrl,
                                    hash = nativeHash,
                                    algorithm = "sha1",
                                    path = $"{Version.MinecraftFolder}/libraries/{nativePath}",
                                    size = nativeSize
                                });
                            }
                        }
                    }
                }
                else // 新版本安装方法
                {
                    foreach (var library in VersionJson.libraries)
                    {
                        if (library?.downloads.artifact == null) throw new InvalidDataException("Json 结构无效: library 缺少 artifact 信息");
                        var artifact = library.downloads.artifact;

                        // 排除架构错误的支持库
                        if (artifact.url.ContainsF("arm") && (Environments.SystemArch != System.Runtime.InteropServices.Architecture.Arm && Environments.SystemArch != System.Runtime.InteropServices.Architecture.Arm64))
                        {
                            continue;
                        }

                        // 根据 url 判断支持库适用的操作系统，并添加到 nativesLib 或 commonLib
                        if ((artifact.url.ContainsF("windows") && Environments.OSType.Windows == Environments.SystemType) ||
                            (artifact.url.ContainsF("linux") && Environments.OSType.Linux == Environments.SystemType) ||
                            ((artifact.url.ContainsF("osx") || artifact.url.ContainsF("macos")) && Environments.OSType.MacOS == Environments.SystemType))
                        {
                            nativesLib.Add(new Download.FileMetaData()
                            {
                                url = artifact.url,
                                path = $"{Version.MinecraftFolder}/libraries/{artifact.path}",
                                hash = artifact.sha1,
                                size = artifact.size,
                                algorithm = "sha1"
                            });
                        }
                        else // 认为是非特定平台的通用库
                        {
                            commonLib.Add(new Download.FileMetaData()
                            {
                                url = artifact.url,
                                path = $"{Version.MinecraftFolder}/libraries/{artifact.path}",
                                hash = artifact.sha1,
                                size = artifact.size,
                                algorithm = "sha1"
                            });
                        }
                    }
                }
                commonLib.Select(k =>
                {
                    Logger.Log(k.url!);
                    return k;
                });
                nativesLib.Select(k =>
                {
                    Logger.Log(k.url!);
                    return k;
                });
                return Tuple.Create(nativesLib, commonLib);
            }
            catch (TaskCanceledException)
            {
                Logger.Log("[Minecraft] 安装操作已取消");
                throw;
            }
            catch (Exception ex)
            {
                Logger.Log(ex, "获取支持库列表失败");
                throw;
            }
        }

        /// <summary>
        /// 获取 Minecraft 资源文件下载列表。
        /// </summary>
        /// <param name="VersionJsonFile">版本 Json 对象。</param>
        /// <param name="Version">Minecraft 版本信息。</param>
        /// <param name="CopyToResource">是否将资源文件复制到旧版本的 resource 文件夹。</param>
        /// <returns>资源文件元数据列表。</returns>
        /// <exception cref="ArgumentException">当版本 Json 文件存在问题或无效安装元数据时抛出。</exception>
        public async static Task<List<Download.FileMetaData>> GetMinecraftAssets(VersionJson VersionJsonFile, McVersion Version, bool CopyToResource)
        {
            List<Download.FileMetaData> downloadList = new();

            string? versionAssetsUrl = VersionJsonFile.assetIndex?.url;
            string? versionAssetsHash = VersionJsonFile.assetIndex?.sha1;
            

            if (versionAssetsUrl.IsNullOrWhiteSpaceF() || versionAssetsHash.IsNullOrWhiteSpaceF())
            {
                throw new ArgumentException("无效的安装元数据：assetsIndex URL 或 Hash 为空。");
            }

            string result = await FileIO.ReadAsString((await Network.NetworkRequest(versionAssetsUrl)));
            await FileIO.WriteData(new MemoryStream(Encoding.UTF8.GetBytes(result)), Version.MinecraftFolder + $"/assets/indexes/{VersionJsonFile.assetIndex!.id}.json");
            Objects? assetsJson = Json.GetJson<Objects>(result);
            var assetsObject = assetsJson?.objects;

            if (assetsObject is not null)
            {
                foreach (var resource in assetsObject)
                {
                    string resHash = resource.Value.hash;
                    long size = resource.Value.size;
                    downloadList.Add(new Download.FileMetaData()
                    {
                        url = Source.GetResourceDownloadSource(resHash),
                        path = (CopyToResource) ? $"{Version.MinecraftFolder}/resource/{resource.Key}" : $"{Version.MinecraftFolder}/assets/objects/{resHash.Substring(0, 2)}/{resHash}",
                        hash = resHash,
                        algorithm = "sha1",
                        size = size
                    });
                }
                return downloadList;
            }
            throw new ArgumentException("版本 Json 文件存在问题，无法安装资源文件！");
        }
    }

    public class Server
    {
        public async static Task DownloadServerCore(string Version)
        {

        }
    }
}