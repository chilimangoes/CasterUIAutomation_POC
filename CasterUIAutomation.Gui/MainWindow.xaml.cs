using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Automation;
using System.Diagnostics;
using WindowsInput;
using WindowsInput.Native;
using Win32 = CasterUIAutomation.Win32;
using CasterUIAutomation.Communication;

namespace CasterUIAutomation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        AutomationFocusChangedEventHandler focusHandler = null;
        System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
        HttpAsyncHost server;
        public MainWindow()
        {
            InitializeComponent();

            server = new HttpAsyncHost(new CasterHttpHandler());
            server.StartAsync("localhost", 2083);

            timer.Interval = new TimeSpan(0, 0, 5);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void appendLog(string message)
        {
            txtOutput.Text = message + "\r\n" + txtOutput.Text;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            var forgroundWindow = Win32.Window.GetForegroundWindowEx(false);
            string message = forgroundWindow.ToString() + "> " + forgroundWindow.GetClassName() + " : " + forgroundWindow.GetWindowText();

            var el = AutomationElement.FocusedElement;
            if (el == null)
            {
                message += " : ";
            }
            else
            {
                message += " : " + el.Current.AutomationId + " : " + el.Current.ClassName;
            }

            appendLog(message);

            //try
            //{
            //    appendLog(Win32.Desktop.DumpWinLogonDesktop());
            //}
            //catch (Exception ex)
            //{
            //    appendLog(ex.ToString());
            //}
            
        }

        private void btnCapture_Click(object sender, RoutedEventArgs e)
        {
            focusHandler = new AutomationFocusChangedEventHandler(OnFocusChange);
            Automation.AddAutomationFocusChangedEventHandler(focusHandler);
        }

        private void OnFocusChange(object src, AutomationFocusChangedEventArgs e)
        {
            // TODO Add event handling code.
            // The arguments tell you which elements have lost and received focus.
            txtOutput.Text += "\r\nUI Automation Focus Changed: " + e.ObjectId;
            txtOutput.Text += "\r\nObjectId=" + e.ObjectId;
            txtOutput.Text += "\r\ne=" + e.ToString();
            txtOutput.Text += "\r\nsrc=" + src.ToString();
        }

        public void UnsubscribeFocusChange()
        {
            if (focusHandler != null)
            {
                Automation.RemoveAutomationFocusChangedEventHandler(focusHandler);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            UnsubscribeFocusChange();
        }

        private void btnTestNotepad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process process = null;
                var processes = Process.GetProcessesByName("notepad");
                if (processes.Length == 0)
                {
                    process = Process.Start(@"notepad.exe");
                    process.WaitForInputIdle();
                }
                else
                {
                    process = processes[0];
                }

                process.SetForegroundWindow();

                var sim = new InputSimulator();
                sim.Keyboard
                    .KeyPress(VirtualKeyCode.HOME)
                    .ModifiedKeyStroke(VirtualKeyCode.LCONTROL, VirtualKeyCode.VK_A)
                    .KeyPress(VirtualKeyCode.DELETE)
                    .TextEntry("These are your orders if you choose to accept them...")
                    .KeyPress(VirtualKeyCode.RETURN)
                    .TextEntry("This message will self destruct in 5 seconds.")
                    .ModifiedKeyStroke(new VirtualKeyCode[] {
                        VirtualKeyCode.LCONTROL, VirtualKeyCode.LSHIFT }, 
                        VirtualKeyCode.LEFT)
                    //.Sleep(100)
                    .KeyPress(VirtualKeyCode.LEFT)
                    .ModifiedKeyStroke(new VirtualKeyCode[] {
                        VirtualKeyCode.LCONTROL, VirtualKeyCode.LSHIFT },
                        VirtualKeyCode.LEFT)
                    //.Sleep(100)
                    .KeyPress(VirtualKeyCode.LEFT)
                    .ModifiedKeyStroke(new VirtualKeyCode[] {
                        VirtualKeyCode.LCONTROL, VirtualKeyCode.LSHIFT },
                        VirtualKeyCode.LEFT)
                    .Sleep(20);
                    //.Sleep(5000)
                    //.ModifiedKeyStroke(VirtualKeyCode.MENU, VirtualKeyCode.SPACE)
                    //.KeyPress(VirtualKeyCode.DOWN)
                    //.KeyPress(VirtualKeyCode.RETURN);
                    
                var notepad = AutomationElement.RootElement.FindChildByProcessId(process.Id);
                var document = notepad.FindChildByClass("Edit");
                string text = document.GetPattern<TextPattern>(TextPattern.Pattern).GetVisibleRanges()[0].GetText(10000);

                txtOutput.Text = text;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnStopTimer_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
        }

        // https://stackoverflow.com/questions/22390117/how-to-create-a-process-with-elevated-privileges-that-displays-users-name-inste
        private void btnFindWinLogon_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var processes = Process.GetProcessesByName("winlogon");

                if (processes.Count() == 0)
                    throw new ApplicationException("Couldn't find winlogon process! Aborting.");
                if (processes.Count() > 1)
                    appendLog("More than one winlogon process found! The first one will be used.");

                var process = processes.First();

                //TokenAccessLevels tokenAccess = TokenAccessLevels.Query | TokenAccessLevels.Duplicate | TokenAccessLevels.AssignPrimary;
                //using (var tokenHandle = Win32.Process.OpenProcessToken(process.Handle, tokenAccess))
                //{
                //    appendLog("----------------------------------------");
                //    appendLog("Token Handle: " + tokenHandle);//.DangerousGetHandle();
                //    appendLog("Process Handle: " + process.Handle);
                //    appendLog("Session ID: " + process.SessionId);
                //    appendLog("PID: " + process.Id);
                //    appendLog("Process Name: " + process.ProcessName);
                //    appendLog("----------------------------------------");
                //}

            }
            catch (Exception ex)
            {
                appendLog(ex.ToString());
                throw;
            }


        }

        private void btnEnumDesktop_Click(object sender, RoutedEventArgs e)
        {
            appendLog(Win32.Desktop.DumpWinLogonDesktop());
        }

        private async void btnAttach_Click(object sender, RoutedEventArgs e)
        {
            var sim = new InputSimulator();
            sim.Keyboard
                .ModifiedKeyStroke(VirtualKeyCode.MENU, VirtualKeyCode.TAB)
                .Sleep(500);

            using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient())
            {
                // Normal press-release key action, optionally repeated several times:
                //     [modifiers -] keyname [/ innerpause] [: repeat] [/ outerpause]
                // Press-and-hold a key, or release a held-down key:
                //     [modifiers -] keyname : direction [/ outerpause]

                var values = new Dictionary<string, string>()
                {
                    { "spec", "t,e,s, t, s-left/0.5:4/1, c-c/2, c-v/1, c-v" },
                    { "prop1", "hello" },
                    { "prop2", "world" }
                };
                var content = new System.Net.Http.FormUrlEncodedContent(values);
                var response = await client.PostAsync("http://localhost:2083/" + txtHandle.Text, content);
                string responseString = await response.Content.ReadAsStringAsync();
                appendLog(responseString);
            }
        }
    }
}
