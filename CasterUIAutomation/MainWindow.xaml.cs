using System;
using System.Collections.Generic;
using System.Linq;
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

namespace CasterUIAutomation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        AutomationFocusChangedEventHandler focusHandler = null;

        public MainWindow()
        {
            InitializeComponent();
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
    }
}
