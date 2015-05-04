using System;
using System.Runtime.InteropServices;

namespace BackupEssentials.Utils{
    internal class NativeMethods{
        [DllImport("USER32.DLL")]
        [return: MarshalAs(UnmanagedType.Bool)]
        protected internal static extern bool SetForegroundWindow(IntPtr hwnd);
    }
}
