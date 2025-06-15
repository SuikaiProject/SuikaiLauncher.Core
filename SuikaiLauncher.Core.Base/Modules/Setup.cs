using System.Xml.Linq;
using System.Runtime.InteropServices;
using System.Data;
using SuikaiLauncher.Core.Override;

namespace SuikaiLauncher.Core.Base
{
    public class Environments
    {

        public static string MojangPath = $"{Environment.SpecialFolder.ApplicationData}/.minecraft";

        public static string ApplicationDataPath
        {
            get
            {
                string path = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "SuikaiLauncher",
                    "Core"
                );
                if (!path.IsAscii())
                {
                    if (OSType.Windows != SystemType)
                    {
                        path = $"/etc/SuikaiLauncher/{Environment.UserName}/Core";
                    }
                    path = Environment.GetEnvironmentVariable("SYSTEMDRIVE") + $"/ProgramData/SuikaiLauncher/{Environment.UserName}/Core";
                }

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                
                
                return path;
            }
        }


        public static string ConfigPath = ApplicationDataPath + "/Setup/config.xml";
        public static string ModDataPath = ApplicationDataPath + "/Setup/data.xml";

        public static readonly Architecture SystemArch = RuntimeInformation.OSArchitecture;

        public enum OSType
        {
            Windows = 0,
            Linux = 1,
            MacOS = 2,
            FreeBSD = 3,
            Unknown = 4
        }

        public static OSType SystemType
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return OSType.Windows;
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return OSType.Linux;
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return OSType.MacOS;
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD)) return OSType.FreeBSD;
                return OSType.Unknown;
            }
            set { throw new ReadOnlyException("尝试写入只读属性"); }
        }

    }
    internal class ModData
    {
        private static XDocument? ModDatabase = XDocument.Load(Environments.ModDataPath);
    }



    public static class Setup
    {
        
    }
}
