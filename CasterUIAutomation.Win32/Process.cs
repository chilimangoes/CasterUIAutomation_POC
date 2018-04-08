using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using CasterUIAutomation.Win32.NativeTypes;

namespace CasterUIAutomation.Win32
{
    public class Process
    {

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool OpenProcessToken(IntPtr ProcessHandle, TokenAccessLevels DesiredAccess, out SafeTokenHandle TokenHandle);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool CreateProcessAsUser(
            SafeTokenHandle hToken,
            string lpApplicationName,
            string lpCommandLine,
            ref SECURITY_ATTRIBUTES lpProcessAttributes,
            ref SECURITY_ATTRIBUTES lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool OpenProcessToken(IntPtr ProcessHandle, UInt32 DesiredAccess, ref IntPtr TokenHandle);


        private static SafeTokenHandle OpenProcessToken(IntPtr processHandle, TokenAccessLevels desiredAccess)
        {
            SafeTokenHandle tokenHandle;
            if (!OpenProcessToken(processHandle, desiredAccess, out tokenHandle))
                throw new ApplicationException("Unable to open process token for handle " + processHandle);

            return tokenHandle;
        }


        private const short SW_SHOW = 5;
        private const uint TOKEN_QUERY = 0x0008;
        private const uint TOKEN_DUPLICATE = 0x0002;
        private const uint TOKEN_ASSIGN_PRIMARY = 0x0001;
        private const int GENERIC_ALL_ACCESS = 0x10000000;
        private const int STARTF_USESHOWWINDOW = 0x00000001;
        private const int STARTF_FORCEONFEEDBACK = 0x00000040;
        private const uint CREATE_UNICODE_ENVIRONMENT = 0x00000400;
        private const int STARTF_RUNFULLSCREEN = 0x00000020;

        private static PROCESS_INFORMATION LaunchProcessAsUser(string exePath, string processArgs, SafeTokenHandle token, IntPtr envBlock, string desktop = @"WinSta0\Default")//, uint WindowMode = 1)
        {
            bool result = false;

            PROCESS_INFORMATION pi = new PROCESS_INFORMATION();
            SECURITY_ATTRIBUTES saProcess = new SECURITY_ATTRIBUTES();
            SECURITY_ATTRIBUTES saThread = new SECURITY_ATTRIBUTES();
            saProcess.nLength = (uint)Marshal.SizeOf(saProcess);
            saThread.nLength = (uint)Marshal.SizeOf(saThread);

            STARTUPINFO si = new STARTUPINFO();
            si.cb = (uint)Marshal.SizeOf(si);

            //if this member is NULL, the new process inherits the desktop
            //and window station of its parent process. If this member is
            //an empty string, the process does not inherit the desktop and
            //window station of its parent process; instead, the system
            //determines if a new desktop and window station need to be created.
            //If the impersonated user already has a desktop, the system uses the
            //existing desktop.

            si.lpDesktop = desktop;
            si.dwFlags = STARTF_USESHOWWINDOW | STARTF_FORCEONFEEDBACK;

            //Check the Startup Mode of the Process 
            //if (WindowMode == 1)
            //    si.wShowWindow = SW_SHOW;
            //else if (WindowMode == 2)
            //{ //Do Nothing
            //}
            //else if (WindowMode == 3)
            //    si.wShowWindow = 0; //Hide Window 
            //else if (WindowMode == 4)
            //    si.wShowWindow = 3; //Maximize Window
            //else if (WindowMode == 5)
            //    si.wShowWindow = 6; //Minimize Window
            //else
            si.wShowWindow = SW_SHOW;


            //Set other si properties as required.
            result = CreateProcessAsUser(
                token,
                null,
                exePath,
                ref saProcess,
                ref saThread,
                false,
                CREATE_UNICODE_ENVIRONMENT,
                envBlock,
                null,
                ref si,
                out pi);

            if (result == false)
            {
                throw new Win32ErrorCodeException("CreateProcessAsUser");
            }

            return pi;
        }

        public static PROCESS_INFORMATION Launch(string exePath, string processArgs, IntPtr processHandle, string desktop = @"WinSta0\Default")
        {
            TokenAccessLevels tokenAccess = TokenAccessLevels.Query | TokenAccessLevels.Duplicate | TokenAccessLevels.AssignPrimary;
            using (var token = Process.OpenProcessToken(processHandle, tokenAccess))
            {
                if (token.IsClosed || token.IsInvalid)
                {
                    throw new ApplicationException("Invalid process token retrieved while attempting to launch process as user.");
                }

                return LaunchProcessAsUser(exePath, processArgs, token, IntPtr.Zero);
            }
        }

    }
}
