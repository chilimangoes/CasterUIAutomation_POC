using CasterUIAutomation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using Win32 = CasterUIAutomation.Win32;

// https://docs.microsoft.com/en-us/dotnet/framework/windows-services/walkthrough-creating-a-windows-service-application-in-the-component-designer

namespace CasterAutomationService
{
    public partial class UIAutomationService : ServiceBase
    {
        EventLog log;
        System.Timers.Timer timer = new System.Timers.Timer();
        Win32.NativeTypes.PROCESS_INFORMATION processInfo = new Win32.NativeTypes.PROCESS_INFORMATION();

        public UIAutomationService()
        {
            InitializeComponent();

            log = new EventLog();
            //EventLog.Delete("CasterUIAutomation");
            //EventLog.DeleteEventSource("CasterUIAutomation");
            //EventLog.CreateEventSource("CasterUIAutomation", "CasterUIAutomation");
            if (!EventLog.SourceExists("CasterUIAutomationLogSource"))
            {
                EventLog.CreateEventSource("CasterUIAutomationLogSource", "CasterUIAutomationLog");
            }
            log.Source = "CasterUIAutomationLogSource";
            log.Log = "CasterUIAutomationLog";

            timer.Interval = 10000; // 10 seconds  
            timer.Elapsed += timer_Elapsed; ;
            timer.Start();
        }

        protected override void OnStart(string[] args)
        {
            log.WriteEntry("Starting service");
            timer.Stop();

            relaunchAsUserProcess();
        }

        private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var forgroundWindow = Win32.Window.GetForegroundWindowEx();
            string message = forgroundWindow.ToString() + "> " + forgroundWindow.GetClassName() + " : " + forgroundWindow.GetWindowText();

            log.WriteEntry(message);
        }

        protected override void OnStop()
        {
            log.WriteEntry("Stopping service");

            try
            {
                if (processInfo.dwProcessId > 0)
                {
                    var p = Process.GetProcessById(Convert.ToInt32(processInfo.dwProcessId));
                    p.Kill();
                }
            }
            catch (Exception ex)
            {
                log.WriteEntry(ex.ToString());
            }
        }
        
        private void relaunchAsUserProcess()
        {
            try
            {
                var processes = Process.GetProcessesByName("winlogon");

                if (processes.Count() == 0)
                    throw new ApplicationException("Couldn't find winlogon process! Aborting.");
                if (processes.Count() > 1)
                    log.WriteEntry("More than one winlogon process found! The first one will be used.");

                var process = processes.First();

                //appendLog("----------------------------------------");
                //appendLog("Process Handle: " + process.Handle);
                //appendLog("Session ID: " + process.SessionId);
                //appendLog("PID: " + process.Id);
                //appendLog("Process Name: " + process.ProcessName);
                //appendLog("----------------------------------------");
                string path = "\"" + Process.GetCurrentProcess().MainModule.FileName + "\" client";
                //path = @"C:\Program Files (x86)\Winspector\WinspectorU.exe";

                // The exceptions to this are services that need to interact with the console user, so these load into Winsta0 instead.
                // https://blogs.technet.microsoft.com/askperf/2007/07/24/sessions-desktops-and-windows-stations/

                processInfo = Win32.Process.Launch(path, "client", process.Handle, @"WinSta0\Winlogon");
            }
            catch (Exception ex)
            {
                log.WriteEntry(ex.ToString());
                throw;
            }
        }
    }
}
