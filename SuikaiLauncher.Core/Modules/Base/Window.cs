using System.Runtime.InteropServices;

namespace SuikaiLauncher.Core.Modules.Base
{
    internal class Window
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetForegroundWindow();

        public static IntPtr GetWindowIntptr()
        {
            return GetForegroundWindow();    
        }
    }
}
