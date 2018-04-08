using CasterUIAutomation.Win32.NativeTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace CasterUIAutomation.Win32
{
    public class Desktop
    {

        public static string[] GetWindowStationNames()
        {
            IList<string> list = new List<string>();
            EnumWindowStationsDelegate callback = new EnumWindowStationsDelegate((string windowStation, IntPtr lParam) => {
                list.Add(windowStation);
                return true;
            });

            if (!EnumWindowStations(callback, IntPtr.Zero))
            {
                throw new Win32ErrorCodeException("EnumWindowStations");
            }

            
            return list.ToArray();
        }



        public static string[] GetDesktopNames(string winStationName)
        {
            IntPtr stationHandle = OpenWindowStation(winStationName, true, WinStationAccess.WINSTA_ENUMDESKTOPS | WinStationAccess.WINSTA_ENUMERATE);
            if (stationHandle == IntPtr.Zero)
            {
                throw new Win32ErrorCodeException("OpenWindowStation('" + winStationName + "')");
            }

            try
            {
                IList<string> list = new List<string>();
                EnumDesktopsDelegate callback = new EnumDesktopsDelegate((string desktopName, IntPtr lParam) => {
                    list.Add(desktopName);
                    return true;
                });

                if (!EnumDesktops(stationHandle, callback, IntPtr.Zero))
                {
                    throw new Win32ErrorCodeException("EnumDesktops('" + winStationName + "')");
                }
                
                return list.ToArray();
            }
            finally
            {
                CloseWindowStation(stationHandle);
            }
        }

        public static Window[] GetWindows(string desktopName)
        {
            IntPtr desktopHandle = OpenDesktop(desktopName, 0, true, WinStationAccess.GENERIC_ALL);
            if (desktopHandle == IntPtr.Zero)
            {
                throw Errors.GetWin32Exception("OpenDesktop('" + desktopName + "')");
            }

            
            //EnumDesktopWindowsDelegate windowDelegate = new EnumDesktopWindowsDelegate(HandleWindowEntry);
            //EnumDesktopWindows(desktopHandle, windowDelegate, IntPtr.Zero);

            //string[] windows = new string[tArrayList.Count];
            //tArrayList.CopyTo(windows);
            //tArrayList.Clear();

            //CloseWindowStation(desktopHandle);


            try
            {
                IList<Window> list = new List<Window>();
                EnumDesktopWindowsDelegate callback = new EnumDesktopWindowsDelegate((IntPtr hWnd, IntPtr lParam) => {
                    list.Add(new Window(hWnd));
                    return true;
                });

                if (!EnumDesktopWindows(desktopHandle, callback, IntPtr.Zero))
                {
                    throw new Win32ErrorCodeException("EnumDesktopWindows('" + desktopName + "')");
                }

                return list.ToArray();
            }
            finally
            {
                CloseDesktop(desktopHandle);
            }

        }


        public static string DumpWinLogonDesktop()
        {
            string message = "";
            foreach (var station in GetWindowStationNames())
            {
                message += "Station: " + station + "\r\n";
                foreach (var desktop in GetDesktopNames(station))
                {
                    if (desktop.ToLower() != "winlogon")
                        continue;

                    message += "    Desktop: " + desktop + "\r\n";
                    try
                    {
                        foreach (var window in GetWindows(desktop))
                        {
                            try
                            {
                                message += string.Format("        hWnd {0}: Class={1}, Text={2}\r\n", window.HWnd, window.GetClassName(), window.GetWindowText());
                            }
                            catch (Exception ex)
                            {
                                message += string.Format("        hWnd {0} Error: {1}\r\n", window.HWnd, ex.Message);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        message += "        " + ex.Message + "\r\n";
                        continue;
                    }
                }
            }

            return message;
        }



        private delegate bool EnumWindowStationsDelegate(string windowsStation, IntPtr lParam);
        private delegate bool EnumDesktopsDelegate(string desktop, IntPtr lParam);
        private delegate bool EnumDesktopWindowsDelegate(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumWindowStations(EnumWindowStationsDelegate lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr OpenWindowStation(
            [MarshalAs(UnmanagedType.LPTStr)] string WinStationName,
            [MarshalAs(UnmanagedType.Bool)] bool Inherit,
            WinStationAccess Access
        );

        [DllImport("user32.dll")]
        private static extern bool CloseWindowStation(
            IntPtr winStation
        );

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool EnumDesktops(
            IntPtr winStation,
            EnumDesktopsDelegate EnumFunc,
            IntPtr lParam
        );

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr OpenDesktop(
            [MarshalAs(UnmanagedType.LPTStr)] string DesktopName,
            uint Flags,
            bool Inherit,
            WinStationAccess Access
        );

        [DllImport("user32.dll")]
        private static extern bool CloseDesktop(
            IntPtr hDesktop
        );

        [DllImport("user32.dll")]
        private static extern bool EnumDesktopWindows(
            IntPtr hDesktop,
            EnumDesktopWindowsDelegate EnumFunc,
            IntPtr lParam
        );

        //[DllImport("user32", SetLastError = true)]
        //private static extern IntPtr GetProcessWindowStation();

        
        //[DllImport("user32.dll")]
        //private static extern bool IsWindowVisible(
        //    IntPtr hwnd
        //);

        //[DllImport("user32.dll")]
        //private static extern IntPtr GetWindowThreadProcessId(
        //    IntPtr hWnd,
        //    out IntPtr ProcessId
        //);

    }

    
}
