using System.Runtime.InteropServices;

namespace SuikaiLauncher.Core.Media
{
    public class Audio{
        // 当前音频设备
        public static object? CurrentDevice;

        // 同步锁
        private static readonly object DeviceChangeLock = new object[1];

        private static bool IsDeviceChange = false;

        private static Thread? AudioDeviceWatcherThread;

        
        
        public static void InitOpenAL(){
            Logger.Log("[System] 开始初始化 OpenAL 音频组件");

        }
        // Audio Watcher
        public static void AudioDeviceWatcher(){
            if (AudioDeviceWatcherThread is not null) AudioDeviceWatcherThread.Interrupt();
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)){
                AudioDeviceWatcherThread = new Thread(WindowsAudioDeviceWatcher);
                AudioDeviceWatcherThread.Start();

            }
        }
        private static void WindowsAudioDeviceWatcher(){
            try{
                while (true){
                    Thread.Sleep(500);
                    if (IsDeviceChange){
                        Logger.Log("[Auido] 检测到音频设备更改，重新初始化音频组件。");
                        lock(DeviceChangeLock){
                            InitOpenAL();
                        }
                    }
                }
        }catch(ThreadInterruptedException){
            Logger.Log("[Audio] 已终止设备轮询检测");
            }
        }
        private static void ALSAAudioDeviceWatcher(){

        }
        [ComImport]
        [Guid("7991EEC9-7E89-4D85-8390-6C703CEC60C0")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IMMNotificationClient
    {
        void OnDeviceStateChanged([MarshalAs(UnmanagedType.LPWStr)] string pwstrDeviceId, uint dwNewState);
        void OnDeviceAdded([MarshalAs(UnmanagedType.LPWStr)] string pwstrDeviceId);
        void OnDeviceRemoved([MarshalAs(UnmanagedType.LPWStr)] string pwstrDeviceId);
        void OnDefaultDeviceChanged(DataFlow flow, Role role, [MarshalAs(UnmanagedType.LPWStr)] string pwstrDefaultDeviceId);
        void OnPropertyValueChanged([MarshalAs(UnmanagedType.LPWStr)] string pwstrDeviceId, ref PropertyKey key);
    }

    // 枚举数据流方向
    enum DataFlow
    {
        Render = 0,
        Capture = 1,
        All = 2
    }

    // 枚举设备角色
    enum Role
    {
        Console = 0,
        Multimedia = 1,
        Communications = 2
    }

    // 定义 PropertyKey 结构
    [StructLayout(LayoutKind.Sequential)]
    struct PropertyKey
    {
        public Guid fmtid;
        public uint pid;
    }

    // 定义 MMDeviceEnumerator COM 类
    [ComImport]
    [Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
    class MMDeviceEnumerator
    {
    }

    class AudioDeviceNotificationClient : IMMNotificationClient
    {
            public void OnDeviceStateChanged(string pwstrDeviceId, uint dwNewState)
            {
                Logger.Log("");
            }

            public void OnDeviceAdded(string pwstrDeviceId)
            {
                
            }

            public void OnDeviceRemoved(string pwstrDeviceId)
            {
                
            }

            public void OnDefaultDeviceChanged(DataFlow flow, Role role, string pwstrDefaultDeviceId)
            {
                lock(DeviceChangeLock){
                    IsDeviceChange = true;
                }
            }

            public void OnPropertyValueChanged(string pwstrDeviceId, ref PropertyKey key)
            {
                Console.WriteLine($"设备属性更改: 设备 ID = {pwstrDeviceId}, 属性 = {key.fmtid}, {key.pid}");
            }
        }

    }
}