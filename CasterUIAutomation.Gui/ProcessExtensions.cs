using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CasterUIAutomation
{
    static class ProcessExtensions
    {
        private const int SW_SHOWNORMAL = 1;

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);
        
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetForegroundWindow(IntPtr hwnd);

        public static void SetForegroundWindow(this Process process)
        {
            ShowWindow(process.MainWindowHandle, SW_SHOWNORMAL);
            SetForegroundWindow(process.MainWindowHandle);
        }
    }
}
