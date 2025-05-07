using System.Xml.Linq;
using System.Runtime.InteropServices;
using System.Data;
using SuikaiLauncher.Core.Override;

namespace SuikaiLauncher.Core.Base
{
    public class Environments
    {

        public static string MojangPath = $"{Environment.SpecialFolder.ApplicationData}/.minecraft";

        public static string ApplicationDataPath = $"{Environment.SpecialFolder.ApplicationData}/SuikaiLauncher/Core";

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



    public class Config
    {
        private static XDocument XmlConfig;
        private static XDocument XmlBackupConfig;
        private static object SetupChangeLock = new object[1];
        private static bool SetupChanged;
        private static Thread SetupWatcher;

        public static void InitConfig()
        {
            if (File.Exists(Environments.ConfigPath))
            {
                throw new FieldAccessException($"目标文件已存在: {Environments.ConfigPath}");
            }

            string? DirectoryPath = Path.GetDirectoryName(Environments.ConfigPath);
            if (DirectoryPath.IsNullOrWhiteSpaceF() && !Directory.Exists(DirectoryPath))
            {
                Directory.CreateDirectory(DirectoryPath);
            }

            XDocument config = new XDocument(new XElement("Setup"));
            config.Root?.Add(new XElement("System"));
            config.Root?.Add(new XElement("Account"));
            config.Root?.Add(new XElement("Versions"));

            try
            {
                config.Save(Environments.ConfigPath);
                config.Save(Environments.ConfigPath.Replace(".xml", ".xml.backup"));
            }
            catch (Exception ex)
            {
                throw new Exception($"初始化配置文件时发生错误:",ex);
            }
        }
        public static void ReleaseSetup()
        {
            lock (SetupChangeLock)
            {
                SetupWatcher.Interrupt();
            }
        }
        public static void LoadConfig(bool ForceReload = false)
        {
            // 必须包括全部代码，以避免同一时间有多个线程尝试加载和修改值导致线程冲突
            lock (SetupChangeLock)
            {
                // 确保单例
                if (!ForceReload && XmlConfig is not null) return;
                if (File.Exists(Environments.ConfigPath))
                {
                    try
                    {
                        XmlConfig = XDocument.Load(Environments.ConfigPath);
                        SetupWatcher = new Thread(StartSetupWatcher);
                        SetupWatcher.Start();
                    }
                    catch (Exception ex)
                    {
                        XmlConfig = null;
                        throw new Exception($"加载配置文件时发生错误", ex);
                    }
                }
                else
                {
                    XmlConfig = null;
                    throw new FileNotFoundException($"配置文件不存在: {Environments.ConfigPath}");
                }

                if (File.Exists(Environments.ConfigPath.Replace(".xml", ".xml.backup")))
                {
                    try
                    {
                        XmlBackupConfig = XDocument.Load(Environments.ConfigPath.Replace(".xml", ".xml.backup"));
                    }
                    catch (Exception ex)
                    {
                        XmlBackupConfig = null;
                        throw new Exception($"加载备份配置文件时发生错误:", ex);
                    }
                }
                else
                {
                    XmlBackupConfig = null;
                }
            }
        }

        private static void StartSetupWatcher()
        {
            try
            {
                while (true)
                {
                    // 减轻同步锁造成的性能影响
                    Thread.Sleep(10000);
                    if (SetupChanged)
                    {
                        lock (SetupChangeLock)
                        {
                            XmlConfig.Save(Environments.ConfigPath);
                        }
                    }
                }
            }catch (ThreadInterruptedException)
            {
                if (SetupChanged)
                {
                    lock (SetupChangeLock)
                    {
                        XmlConfig.Save(Environments.ConfigPath);
                    }
                }
            }
        }

        /// <summary>
        /// 设置命名空间内某个设置项的值
        /// </summary>
        /// <param name="key">设置项名称</param>
        /// <param name="value">值</param>
        /// <param name="XmlNameSpace">主命名空间</param>
        /// <param name="SubNameSpace">子命名空间</param>
        public static void Set(string key, string value, string XmlNameSpace = "System", string SubNameSpace = "")
        {
            if (XmlConfig is null) LoadConfig();
            if (XmlConfig?.Root == null)
            {
                throw new InvalidOperationException("配置文件未加载或根节点不存在。");
            }

            if (key.IsNullOrWhiteSpaceF())
            {
                throw new ArgumentNullException(nameof(key), "设置项名称不能为空。");
            }

            if (XmlNameSpace.IsNullOrWhiteSpaceF())
            {
                throw new ArgumentNullException("给定关键字不存在于设置项中", nameof(XmlNameSpace));
            }

            XElement namespaceElement = XmlConfig.Root.Element(XmlNameSpace);
            if (namespaceElement == null)
            {
                throw new ArgumentException("给定关键字不存在于设置项中", nameof(XmlNameSpace));
            }

            if (SubNameSpace.IsNullOrWhiteSpaceF())
            {
                XElement keyElement = namespaceElement.Element(key);
                if (keyElement == null)
                {
                    lock (SetupChangeLock) 
                    {
                        namespaceElement.Add(new XElement(key, value));
                    }
                }
                else
                {
                    lock (SetupChangeLock)
                    {
                        keyElement.Value = value;
                    }
                }
                SaveConfig();
                return;
            }

            XElement subNamespaceElement = namespaceElement.Element(SubNameSpace);
            if (subNamespaceElement == null)
            {
                lock (SetupChangeLock)
                {
                    namespaceElement.Add(new XElement(SubNameSpace, new XElement(key, value)));
                }
            }
            else
            {
                XElement keyElement = subNamespaceElement.Element(key);
                if (keyElement == null)
                {
                    lock (SetupChangeLock)
                    {
                        subNamespaceElement.Add(new XElement(key, value));
                    }
                }
                else
                {
                    lock (SetupChangeLock)
                    {
                        keyElement.Value = value;
                    }
                }
            }
            SaveConfig();
        }

        /// <summary>
        /// 获取命名空间内某个设置项的值
        /// </summary>
        /// <param name="key">设置项名称</param>
        /// <param name="XmlNameSpace">主命名空间</param>
        /// <param name="SubNameSpace">子命名空间</param>
        /// <returns>object</returns>
        public static object? Get(string key, string XmlNameSpace = "System", string SubNameSpace = "")
        {
            if (XmlConfig is null) LoadConfig();
            if (XmlConfig?.Root == null)
            {
                throw new InvalidOperationException("配置文件未加载或根节点不存在。");
            }

            if (key.IsNullOrWhiteSpaceF())
            {
                throw new ArgumentNullException(nameof(key), "设置项名称不能为空。");
            }

            if (XmlNameSpace.IsNullOrWhiteSpaceF())
            {
                throw new ArgumentNullException("给定关键字不存在于设置项中", nameof(XmlNameSpace));
            }

            XElement namespaceElement = XmlConfig.Root.Element(XmlNameSpace);
            if (namespaceElement == null)
            {
                throw new ArgumentException("给定关键字不存在于设置项中", nameof(XmlNameSpace));
            }

            if (SubNameSpace.IsNullOrWhiteSpaceF())
            {
                return namespaceElement.Element(key)?.Value;
            }

            XElement subNamespaceElement = namespaceElement.Element(SubNameSpace);
            if (subNamespaceElement == null)
            {
                throw new ArgumentException("给定关键字不存在于设置项中", nameof(SubNameSpace));
            }

            return subNamespaceElement.Element(key)?.Value;
        }

        /// <summary>
        /// 重置某个设置项
        /// </summary>
        /// <param name="key">设置项名称</param>
        /// <param name="XmlNameSpace">主命名空间</param>
        /// <param name="SubNameSpace">子命名空间</param>
        public static void Reset(string key, string XmlNameSpace = "System", string SubNameSpace = "")
        {
            if (XmlConfig is null) LoadConfig();
            if (XmlConfig?.Root == null)
            {
                throw new InvalidOperationException("配置文件未加载或根节点不存在。");
            }

            if (key.IsNullOrWhiteSpaceF())
            {
                throw new ArgumentNullException(nameof(key), "设置项名称不能为空。");
            }

            if (XmlNameSpace.IsNullOrWhiteSpaceF())
            {
                throw new ArgumentNullException("给定关键字不存在于设置项中", nameof(XmlNameSpace));
            }

            XElement namespaceElement = XmlConfig.Root.Element(XmlNameSpace);
            if (namespaceElement == null)
            {
                throw new ArgumentException("给定关键字不存在于设置项中", nameof(XmlNameSpace));
            }

            if (SubNameSpace.IsNullOrWhiteSpaceF())
            {
                if (namespaceElement.Element(key) == null)
                {
                    throw new ArgumentException("给定关键字不存在于设置项中", nameof(key));
                }
                namespaceElement.Element(key)?.Remove();
                SaveConfig();
                return;
            }

            XElement subNamespaceElement = namespaceElement.Element(SubNameSpace);
            if (subNamespaceElement == null)
            {
                throw new ArgumentException("给定关键字不存在于设置项中", nameof(SubNameSpace));
            }
            if (subNamespaceElement.Element(key) == null)
            {
                throw new ArgumentException("给定关键字不存在于设置项中", nameof(key));
            }
            subNamespaceElement.Element(key)?.Remove();
            SaveConfig();
        }


        /// <summary>
        /// 清空设置项
        /// </summary>
        /// <param name="key">设置项名称</param>
        /// <param name="XmlNameSpace">设置项所处的命名空间</param>
        /// <param name="SubNameSpace">设置项所处的子命名空间</param>
        public static void Clean(string key, string XmlNameSpace = "System", string SubNameSpace = "")
        {
            Set(key, string.Empty, XmlNameSpace, SubNameSpace);
        }

        /// <summary>
        /// 创建设置项命名空间
        /// </summary>
        /// <param name="XmlNameSpace">主命名空间</param>
        /// <param name="SubNameSpace">子命名空间</param>
        public static void CreateXmlNameSpace(string XmlNameSpace, string SubNameSpace = "")
        {
            if (XmlConfig is null) LoadConfig();
            if (XmlConfig?.Root == null)
            {
                throw new InvalidOperationException("配置文件未加载或根节点不存在。");
            }

            if (XmlNameSpace.IsNullOrWhiteSpaceF())
            {
                throw new ArgumentNullException("给定关键字不存在于设置项中", nameof(XmlNameSpace));
            }

            XElement namespaceElement = XmlConfig.Root.Element(XmlNameSpace);
            if (namespaceElement == null)
            {
                XmlConfig.Root.Add(new XElement(XmlNameSpace));
                namespaceElement = XmlConfig.Root.Element(XmlNameSpace);
            }

            if (!SubNameSpace.IsNullOrWhiteSpaceF() && namespaceElement != null && namespaceElement.Element(SubNameSpace) == null)
            {
                namespaceElement.Add(new XElement(SubNameSpace));
            }
            SaveConfig();
        }

        /// <summary>
        /// 删除设置项命名空间
        /// </summary>
        /// <param name="XmlNameSpace">主命名空间</param>
        /// <param name="SubNameSpace">子命名空间</param>
        public static void DeleteXmlNameSpace(string XmlNameSpace, string SubNameSpace = "")
        {
            if (XmlConfig is null) LoadConfig(); 
            if (XmlConfig?.Root == null)
            {
                throw new InvalidOperationException("配置文件未加载或根节点不存在。");
            }

            if (XmlNameSpace.IsNullOrWhiteSpaceF())
            {
                throw new ArgumentNullException("给定关键字不存在于设置项中", nameof(XmlNameSpace));
            }

            if (SubNameSpace.IsNullOrWhiteSpaceF())
            {
                if (XmlConfig.Root.Element(XmlNameSpace) == null)
                {
                    throw new ArgumentException("给定关键字不存在于设置项中", nameof(XmlNameSpace));
                }
                XmlConfig.Root.Element(XmlNameSpace)?.Remove();
            }
            else
            {
                XElement namespaceElement = XmlConfig.Root.Element(XmlNameSpace);
                if (namespaceElement == null)
                {
                    throw new ArgumentException("给定关键字不存在于设置项中", nameof(XmlNameSpace));
                }
                if (namespaceElement.Element(SubNameSpace) == null)
                {
                    throw new ArgumentException("给定关键字不存在于设置项中", nameof(SubNameSpace));
                }
                namespaceElement.Element(SubNameSpace)?.Remove();
            }
            SaveConfig();
        }

        private static void SaveConfig()
        {
            try
            {
                if (SetupChanged) return;
                lock (SetupChangeLock) 
                {
                    SetupChanged = true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"保存配置文件时发生错误",ex);
            }
        }
    }
}
