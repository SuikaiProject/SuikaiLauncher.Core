namespace SuikaiLauncher.Core.Base{
    public class MavenClient{
        /// <summary>
        /// 生成 Maven 文件对应的下载链接
        /// </summary>
        /// <param name="MavenServer">Maven 服务器</param>
        /// <param name="MavenPackage">Maven 包名</param>
        /// <returns><see langword="string"/></returns>
        public static string ResolveMavenUrl(string MavenServer,string MavenPackage){
            string Url = MavenServer;
            string[] PackagePath = MavenPackage.Split(":");
            foreach (string MavenPath in PackagePath){
                Url += MavenPath;
            }
            return Url + $"{PackagePath[PackagePath.Length -2]}-{PackagePath[PackagePath.Length -1]}.jar";
        }
    }
}