namespace SuikaiLauncher.Core.Runtime.Java{
    public class JavaProperty{
        /// <summary>
        /// Java 大版本号，例如 Java 21
        /// </summary>
        public int MojarVersion;
        /// <summary>
        /// 次级版本号，一般为中间部分，例如 Java 17.0.8 的 0
        /// </summary>
        public int MinorVersion;
        /// <summary>
        /// 最小版本号，例如 Java 8u51 的 51，Java 21.0.3 的 3
        /// </summary>
        public int PatchVersion;
        /// <summary>
        /// 是否为 JRE
        /// </summary>
        public bool IsJre;
        /// <summary>
        /// java.exe 的可执行文件路径
        /// </summary>
        public string? JavaExecutable;
        /// <summary>
        /// javaw.exe 可执行文件路径
        /// </summary>
        public string? JavawExecutable;
        /// <summary>
        /// bin 目录
        /// </summary>
        public string? JavaHomePath;
        /// <summary>
        /// 是否由用户禁用，如果为 true 则不会列入可使用列表 
        /// </summary>
        public bool Disable;
        /// <summary>
        /// 是否为手动导入的 Java
        /// </summary>
        public bool UserImport;
        /// <summary>
        /// 是否为 32 位 Java
        /// </summary>
        public bool Is32Bits;

        public static string GetJsonUrlByName(string name){
            return "";
        }
    }
}