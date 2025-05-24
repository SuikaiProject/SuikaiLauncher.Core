using SuikaiLauncher.Core.Runtime.Java;
using System.Runtime.CompilerServices;

namespace SuikaiLauncher.Core.Minecraft{
    public class MinecraftVersionListSource{
        public string? McVersion;


        internal static readonly DownloadSource Mojang = new DownloadSource() { ClientMetaV1 = "https://piston-meta.mojang.com/mc/game/version_manifest.json",ClientMetaV2 = "https://piston-meta.mojang.com/mc/game/version_manifest_v2.json",resource = "https://resources.download.minecraft.net",JavaList = "https://piston-meta.mojang.com/v1/products/java-runtime/2ec0cc96c44e5a76b9c8b7c39df7210883d12871/all.json",RepositoryName = "Mojang"};
        internal static readonly DownloadSource BMCLAPI = new DownloadSource() { ClientMetaV1 = "https://piston-meta.mojang.com/mc/game/version_manifest.json",ClientMetaV2 = "https://bmclapi2.bangbang93.com/mc/game/version_manifest_v2.json",resource = "https://bmclapi2.bangbang93.com/assets",JavaList = "https://bmclapi2.bangbang93.com/v1/products/java-runtime/2ec0cc96c44e5a76b9c8b7c39df7210883d12871/all.json",RepositoryName = "BMCLAPI"};
        /// <summary>
        /// 用户自定义下载源，请注意如果添加过多下载源可能导致拉取版本列表/安装版本时下载缓慢。
        /// </summary>
        public static List<DownloadSource>? UserCustom;
        

        //public async static List<TSafeDictionary<>>
        public async static void DownloadClientList(){
            
        }

    }
    public class DownloadSource{
        /// <summary>
        /// 资源下载地址，可选提供。
        /// </summary>
        public string? resource;
        /// <summary>
        /// v1 版本列表，条件可选（v2 不为空）
        /// </summary>
        public string? ClientMetaV1;
        /// <summary>
        /// v2 版本列表，条件可选（v1 不为空）
        /// </summary>
        public string? ClientMetaV2;
        /// <summary>
        /// Java 版本列表，可选提供
        /// </summary>
        public string? JavaList;
        /// <summary>
        /// 下载源名称，必须提供
        /// </summary>
        public string? RepositoryName;
    }
}