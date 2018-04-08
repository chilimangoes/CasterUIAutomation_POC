using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace CasterUIAutomation.Win32
{
    public class Window
    {

        public Window(IntPtr hWnd)
        {
            HWnd = hWnd;
        }

        public IntPtr HWnd { get; private set; }

        public string GetClassName(bool throwError = false)
        {
            // Pre-allocate 256 characters, since this is the maximum class name length.
            StringBuilder buffer = new StringBuilder(256);
            int charCount = GetClassName(HWnd, buffer, buffer.Capacity);
            if (charCount != 0)
            {
                return buffer.ToString();
            }
            else if (throwError)
            {
                throw new Win32ErrorCodeException("GetClassName(hWnd=" + HWnd.ToString() + ")");
            }
            return "";
        }

        public string GetWindowText(bool throwError = false)
        {
            const int count = 256;
            StringBuilder buffer = new StringBuilder(count);
            if (GetWindowText(HWnd, buffer, count) > 0)
            {
                return buffer.ToString();
            }
            else if (throwError)
            {
                throw new Win32ErrorCodeException("GetWindowText(hWnd=" + HWnd.ToString() + ")");
            }
            return "";
        }

        public static Window GetForegroundWindowEx(bool throwError = true)
        {
            IntPtr handle = GetForegroundWindow();
            if (throwError && handle == IntPtr.Zero)
            {
                throw new Win32ErrorCodeException("GetForegroundWindow");
            }
            return new Window(handle);
        }




        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

    }
}
