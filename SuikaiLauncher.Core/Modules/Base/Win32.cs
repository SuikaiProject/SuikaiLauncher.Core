using System.Runtime.InteropServices;

namespace SuikaiLauncher.Core.Base
{
    internal class Win32
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetForegroundWindow();

    }
}
