using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using System.ComponentModel;
using Icons = CasterUIAutomation.Gui.Resources.Icons;
using CasterUIAutomation.Communication;
using System.Diagnostics;
using System.Drawing;

namespace CasterUIAutomation.SystemTray
{
    public class ViewManager : IDisposable
    {
        private const string APP_NAME = "CasterUIAutomation";
        private HttpAsyncHost _server;

        public ViewManager(HttpAsyncHost server)
        {
            _server = server;

            _server.StatusChanged += this.OnStatusChange;

            _components = new Container();
            _notifyIcon = new NotifyIcon(_components)
            {
                ContextMenuStrip = new ContextMenuStrip(),
                Icon = Icons.AppPausedIcon,
                Text = "CasterUIAutomation: Starting Up...",
                Visible = true,
            };

            _notifyIcon.ContextMenuStrip.Opening += ContextMenuStrip_Opening;
            _notifyIcon.DoubleClick += notifyIcon_DoubleClick;
            _notifyIcon.MouseUp += notifyIcon_MouseUp;

            _hiddenWindow = new System.Windows.Window();
            _hiddenWindow.Hide();

            StartServer();
        }

        Icon AppIcon
        {
            get
            {
                return (_server != null && _server.Status == HttpAsyncHostStatus.Running)
                    ? Icons.AppIcon
                    : Icons.AppPausedIcon;
            }
        }

        // This allows code to be run on a GUI thread
        private System.Windows.Window _hiddenWindow;

        private System.ComponentModel.IContainer _components;
        private NotifyIcon _notifyIcon;
        
        private ToolStripMenuItem _startServiceMenuItem;
        private ToolStripMenuItem _stopServiceMenuItem;
        private ToolStripMenuItem _exitMenuItem;

        private void DisplayStatusMessage(string text, bool showBalloon = true, ToolTipIcon balloonIcon = ToolTipIcon.None)
        {
            _hiddenWindow.Dispatcher.Invoke(delegate
            {
                string message = APP_NAME + ": " + text;
                _notifyIcon.Text = message;
                if (showBalloon)
                {
                    //_notifyIcon.BalloonTipText = message;
                    // NOTE: the timeout value seems to be ignored on recent versions of Windows
                    _notifyIcon.ShowBalloonTip(3000, APP_NAME, text, balloonIcon);
                }
            });
        }

        private void SetMenuItems()
        {
            if (_server == null || _startServiceMenuItem == null || _stopServiceMenuItem == null || _exitMenuItem == null)
            {
                return;
            }

            switch (_server.Status)
            {
                case HttpAsyncHostStatus.Uninitialised:
                    _startServiceMenuItem.Enabled = false;
                    _stopServiceMenuItem.Enabled = false;
                    _exitMenuItem.Enabled = true;
                    break;
                case HttpAsyncHostStatus.Initialised:
                    _startServiceMenuItem.Enabled = true;
                    _stopServiceMenuItem.Enabled = false;
                    _exitMenuItem.Enabled = true;
                    break;
                case HttpAsyncHostStatus.Starting:
                    _startServiceMenuItem.Enabled = false;
                    _stopServiceMenuItem.Enabled = false;
                    _exitMenuItem.Enabled = false;
                    break;
                case HttpAsyncHostStatus.Running:
                    _startServiceMenuItem.Enabled = false;
                    _stopServiceMenuItem.Enabled = true;
                    _exitMenuItem.Enabled = true;
                    break;
                //case HttpAsyncHostStatus.Error:
                //    _startServiceMenuItem.Enabled = false;
                //    _stopServiceMenuItem.Enabled = false;
                //    _exitMenuItem.Enabled = true;
                //    break;
                default:
                    Debug.Assert(false, "SetButtonStatus() => Unknown state");
                    break;
            }
            
        }

        private void OnStatusChange(HttpAsyncHostStatus oldStatus, HttpAsyncHostStatus newStatus)
        {
            MethodInvoker method = delegate
            {
                if (_server == null || _notifyIcon == null)
                {
                    return;
                }

                SetMenuItems();

                switch (_server.Status)
                {
                    case HttpAsyncHostStatus.Initialised:
                        DisplayStatusMessage("Stopped");
                        break;
                    case HttpAsyncHostStatus.Running:
                        DisplayStatusMessage("Running");
                        break;
                    case HttpAsyncHostStatus.Starting:
                        DisplayStatusMessage("Starting", false);
                        break;
                    case HttpAsyncHostStatus.Uninitialised:
                        DisplayStatusMessage("Not Ready", false);
                        break;
                    //case HttpAsyncHostStatus.Error:
                    //    DisplayStatusMessage("Error occurred, check logs", true, ToolTipIcon.Error);
                    //    break;
                    default:
                        DisplayStatusMessage("Unrecognized status code", true, ToolTipIcon.Error);
                        break;
                }
                _notifyIcon.Icon = AppIcon;
            };

            if (_notifyIcon != null && _notifyIcon.ContextMenuStrip.InvokeRequired)
            {
                _notifyIcon.ContextMenuStrip.Invoke(method);
            }
            else
            {
                method.Invoke();
            }
        }

        private void StartServer()
        {
            if (_server?.Status == HttpAsyncHostStatus.Initialised)
            {
                // TODO: Refactor this to use a configurable port
                _server?.StartAsync("localhost", 2083);
            }
        }

        private void startStopServiceItem_Click(object sender, EventArgs e)
        {
            if (_server?.Status == HttpAsyncHostStatus.Running)
            {
                _server.Stop();
            }
            else
            {
                StartServer();
            }
        }

        private void exitItem_Click(object sender, EventArgs e)
        {
            this.Dispose();
            Application.Exit();
        }

        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            
        }

        private void notifyIcon_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // HACK: ShowContextMenu is private so we call it via reflection
                MethodInfo mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
                mi.Invoke(_notifyIcon, null);
            }
        }

        private ToolStripMenuItem ToolStripMenuItemWithHandler(string displayText, string tooltipText, EventHandler eventHandler)
        {
            var item = new ToolStripMenuItem(displayText);
            if (eventHandler != null)
            {
                item.Click += eventHandler;
            }

            item.ToolTipText = tooltipText;
            return item;
        }

        private void ContextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = false;

            if (_notifyIcon.ContextMenuStrip.Items.Count == 0)
            {
                _startServiceMenuItem = ToolStripMenuItemWithHandler(
                    "Start Service",
                    "Starts listening for commands through the HTTP service",
                    startStopServiceItem_Click);
                _stopServiceMenuItem = ToolStripMenuItemWithHandler(
                    "Stop Service",
                    "Stops listening for commands through the HTTP service",
                    startStopServiceItem_Click);
                _exitMenuItem = ToolStripMenuItemWithHandler("&Exit", "Shut down CasterUIAutomation app", exitItem_Click);

                _notifyIcon.ContextMenuStrip.Items.Add(_startServiceMenuItem);
                _notifyIcon.ContextMenuStrip.Items.Add(_stopServiceMenuItem);
                _notifyIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
                _notifyIcon.ContextMenuStrip.Items.Add(_exitMenuItem);
            }

            SetMenuItems();
        }

        public void Dispose()
        {
            if (_server != null)
            {
                _server?.Stop();
                _server.StatusChanged -= this.OnStatusChange;
            }
            _server = null;
        }
    }
}
