using System.Xml.Linq;
using System.Runtime.InteropServices;
using System.Data;

namespace SuikaiLauncher.Core.Base
{
    internal class Environments
    {
        
        public static string MojangPath = $"{Environment.SpecialFolder.ApplicationData}/.minecraft";
       
        public static string ApplicationDataPath = $"{Environment.SpecialFolder.ApplicationData}/SuikaiLauncher/Core";

        public static string ConfigPath = ApplicationDataPath + "/Setup/config.xml";
        public static string ModDataPath = ApplicationDataPath + "/Setup/data.xml";

        public static readonly Architecture SystemArch = RuntimeInformation.OSArchitecture;

        public enum OSType{
            Windows = 0,
            Linux = 1,
            MacOS = 2,
            FreeBSD = 3,
            Unknown = 4
        }

        public static OSType SystemType {get {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return OSType.Windows;
            else if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return OSType.Linux;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return OSType.MacOS;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD)) return OSType.FreeBSD;
            return OSType.Unknown;
        }
        set {throw new ReadOnlyException("尝试写入只读属性");}
        }

    }
    internal class ModData{
        private static XDocument? ModDatabase = XDocument.Load(Environments.ModDataPath);
    }
    internal class Config{
        private static XDocument XmlConfig = XDocument.Load(Environments.ConfigPath);
        
        /// <summary>
        /// 设置命名空间内某个设置项的值
        /// </summary>
        /// <param name="key">设置项名称</param>
        /// <param name="value">值</param>
        /// <param name="XmlNameSpace">主命名空间</param>
        /// <param name="SubNameSpace">子命名空间</param>
        public static void Set(string key,string value,string XmlNameSpace = "System",string SubNameSpace = ""){
            if (string.IsNullOrWhiteSpace(SubNameSpace)) {
                XmlConfig.Root.Element(key).Value = value;
                return;
            }
            XmlConfig.Root.Element(XmlNameSpace).Element(SubNameSpace).Element(key).Value = value;

        }
        /// <summary>
        /// 获取命名空间内某个设置项的值
        /// </summary>
        /// <param name="key">设置项名称</param>
        /// <param name="XmlNameSpace">主命名空间</param>
        /// <param name="SubNameSpace">子命名空间</param>
        /// <returns>object</returns>
        public static object? Get(string key,string XmlNameSpace = "System",string SubNameSpace = ""){
            return "";
        }
        /// <summary>
        /// 重置某个设置项或命名空间
        /// </summary>
        /// <param name="key">设置项名称</param>
        /// <param name="XmlNameSpace">主命名空间</param>
        /// <param name="SubNameSpace">子命名空间</param>
        public static void Reset(string key,string XmlNameSpace = "System",string SubNameSpace = ""){

        }
        /// <summary>
        /// 删除某个设置项
        /// </summary>
        /// <param name="key">设置项名称</param>
        /// <param name="XmlNameSpace">设置项所处的主命名空间</param>
        /// <param name="SubNameSpace">设置项所处的子命名空间</param>
        public static void Delete(string key,string XmlNameSpace = "System",string SubNameSpace = ""){

        }
        /// <summary>
        /// 类似 Delete，但会将设置项设置为空字符串
        /// </summary>
        /// <param name="key">设置项名称</param>
        /// <param name="XmlNameSpace">设置项所处的命名空间</param>
        /// <param name="SubNameSpace">设置项所处的子命名空间</param>
        public static void Clean(string key,string XmlNameSpace = "System",string SubNameSpace = ""){

        }
        /// <summary>
        /// 创建设置项命名空间
        /// </summary>
        /// <param name="XmlNameSpace">主命名空间</param>
        /// <param name="SubNameSpace">子命名空间</param>
        public static void CreateXmlNameSpace(string XmlNameSpace,string SubNameSpace){

        }
        /// <summary>
        /// 删除设置项命名空间
        /// </summary>
        /// <param name="XmlNameSpace">主命名空间</param>
        /// <param name="SubNameSpace">子命名空间</param>
        public static void DeleteXmlNameSpace(string XmlNameSpace,string SubNameSpace){

        }
    }
}
