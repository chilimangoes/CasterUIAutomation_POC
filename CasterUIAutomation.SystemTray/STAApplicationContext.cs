using CasterUIAutomation.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CasterUIAutomation.SystemTray
{
    public class STAApplicationContext : ApplicationContext
    {
        private ViewManager _viewManager;
        private HttpAsyncHost _server;

        public STAApplicationContext()
        {
            _server = new HttpAsyncHost(new CasterHttpHandler());
            _viewManager = new ViewManager(_server);
        }

        // Called from the Dispose method of the base class
        protected override void Dispose(bool disposing)
        {
            _server?.Stop();
            _viewManager?.Dispose();

            _server = null;
            _viewManager = null;
        }
    }
}
