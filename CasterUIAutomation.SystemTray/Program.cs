using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace CasterUIAutomation.SystemTray
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Use the assembly GUID as the name of the mutex which we use to detect if an application instance is already running
            bool createdNew = false;
            string mutexName = Assembly.GetExecutingAssembly().GetType().GUID.ToString();
            using (Mutex mutex = new Mutex(false, mutexName, out createdNew))
            {
                if (!createdNew)
                {
                    // Only allow one instance
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                try
                {
                    STAApplicationContext context = new STAApplicationContext();
                    Application.Run(context);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "CasterUIAutomation: Error");
                }
            }
        }
    }
}
