using CasterUIAutomation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using Win32 = CasterUIAutomation.Win32;
using CasterUIAutomation.SystemTray;

namespace CasterAutomationService
{

    // https://www.codeproject.com/Articles/35773/Subverting-Vista-UAC-in-Both-and-bit-Archite

    static class Program
    {
        static EventLogWrapper log;

        // This reference is only here so that Visual Studio will copy the system tray application to the output folder when we build the project
        static CasterUIAutomation.SystemTray.STAApplicationContext appContext;

        static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Maximum of one argument accepted.");
                Environment.Exit(1);
            }

            log = new EventLogWrapper("CasterUIAutomationLogSource", "CasterUIAutomationLog");

            string message = "args =";
            foreach (string arg in args)
            {
                message += " " + arg;
            }
            log.WriteEntry(message);

            if (args.Length == 1 && args[0].ToLower() == "client")
            {
                for (int i = 0; i < 10; i++)
                {
                    try
                    {
                        System.Threading.Thread.Sleep(10000);
                        timer_Elapsed();
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            log.WriteEntry(ex.ToString());
                        }
                        catch (Exception) { }
                    }
                }
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    new UIAutomationService()
                };
                ServiceBase.Run(ServicesToRun);
            }
        }

        private static void timer_Elapsed()
        {
            // foreground window info
            var forgroundWindow = Win32.Window.GetForegroundWindowEx(false);
            string message = forgroundWindow.ToString() + "> " + forgroundWindow.GetClassName() + " : " + forgroundWindow.GetWindowText();

            log.WriteEntry(message);
            
            // enumerate desktops
            log.WriteEntry(Win32.Desktop.DumpWinLogonDesktop());
        }

    }
}
