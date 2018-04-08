using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WindowsInput;
using WindowsInput.Native;

// Loosely based off of code in Dragonfly, to make porting the Dragonfly Key action class easier

// TODO: Test with RDP and VNC connections

// TODO: Needs to be tested with international keyboards and different language/locale settings
//
// @comodoro has specifically mentioned issues with this in the Gitter channel, around March 2018
// 
// See the following for info about mapping scan codes to virtual key codes and vice-versa
// http://archives.miloush.net/michkap/archive/2006/09/10/748742.html
// http://archives.miloush.net/michkap/archive/2006/04/06/569632.html
// https://stackoverflow.com/questions/4822261/is-it-possible-to-create-a-keyboard-layout-that-is-identical-to-the-keyboard-use
// https://stackoverflow.com/questions/1021175/sendinput-keyboard-letters-c-c
// https://stackoverflow.com/questions/1970917/sendinput-and-non-english-characters-and-keyboard-layouts
// 
// Some of the code here might be useful for getting key codes based on the keyboard layout:
// https://www.pinvoke.net/default.aspx/user32.vkkeyscanex
// https://www.pinvoke.net/default.aspx/user32.GetKeyboardLayout
//
// This may be part of cause of the problem that @comodoro was reporting:
// https://stackoverflow.com/questions/16442116/getkeyboardlayout-returns-strange-layout
// "GetKeyboardLayout() is what you use. With the further constraint that it is a process-specific 
// value and you cannot obtain the active keyboard layout for another process. Which makes using 
// ToUnicodeEx in a low-level keyboard hook a lost cause."
// 
// BUT, this comment seems to offer the solution:
// "If anyone comes across this, it is actually possible to get the KeyLayout of another process. I'm 
// using toUnicodeEx and it works perfectly fine :)
//   GetKeyboardLayout(GetWindowThreadProcessId(GetForegroundWindow(), NULL))"
//
// The notes in the documentation mention listening for the WM_INPUTLANGCHANGE message 
// https://msdn.microsoft.com/en-us/library/windows/desktop/ms646296(v=vs.85).aspx

namespace CasterUIAutomation.Win32
{

    public class Keyboard
    {
        IntPtr keyboardLayout;

        private Keyboard(IntPtr keyboardLayout)
        {
            this.keyboardLayout = keyboardLayout;
        }

        public void SendKeyboardEvents(IEnumerable<KeyboardEvent> events)
        {
            var keyboard = new InputSimulator().Keyboard;
            foreach (var e in events)
            {
                if (e.KeyDown)
                    keyboard.KeyDown(e.KeyCode);
                else
                    keyboard.KeyUp(e.KeyCode);

                if (e.Timeout > 0)
                    keyboard.Sleep((int)(e.Timeout * 1000));
            }
        }

        public Typeable GetTypeable(char character)
        {
            
            var code = VkKeyScanEx(character, keyboardLayout);

            VirtualKeyCode key = (VirtualKeyCode)(code & 0x00ff);
            List<VirtualKeyCode> modifiers = new List<VirtualKeyCode>();
            if ((code & 0x0100) != 0) modifiers.Add(VirtualKeyCode.SHIFT);
            if ((code & 0x0200) != 0) modifiers.Add(VirtualKeyCode.CONTROL);
            if ((code & 0x0400) != 0) modifiers.Add(VirtualKeyCode.MENU);

            return new Typeable(key, modifiers.ToArray());
        }

        public static Keyboard GetActiveKeyboard()
        {
            var keyboardLayout = GetKeyboardLayout(Window.GetForegroundWindowEx().GetProcessId());
            return new Keyboard(keyboardLayout);
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern short VkKeyScanEx(char ch, IntPtr dwhkl);

        [DllImport("user32.dll")]
        static extern IntPtr GetKeyboardLayout(IntPtr idThread);

    }

    public class Typeable
    {
        public Typeable(VirtualKeyCode key)
            : this(key, new VirtualKeyCode[0]) { }
        public Typeable(VirtualKeyCode key, VirtualKeyCode[] modifiers)
        {
            Key = key;
            Modifiers = modifiers ?? new VirtualKeyCode[0];
        }

        public VirtualKeyCode Key { get; private set; }
        public VirtualKeyCode[] Modifiers { get; private set; }

        public IEnumerable<KeyboardEvent> GetOnEvents(float timeout=0.01F)
        {
            var events = Modifiers.Select(k => new KeyboardEvent { KeyCode = k, KeyDown = true, Timeout = 0 })
                .ToList();
            events.Add(new KeyboardEvent { KeyCode = this.Key, KeyDown = true, Timeout = timeout });
            return events;
        }
        public IEnumerable<KeyboardEvent> GetOffEvents(float timeout = 0.01F)
        {
            var events = Modifiers.Select(k => new KeyboardEvent { KeyCode = k, KeyDown = false, Timeout = 0 })
                .ToList();
            events.Add(new KeyboardEvent { KeyCode = this.Key, KeyDown = false, Timeout = timeout });
            events.Reverse();
            return events;
        }
        public IEnumerable<KeyboardEvent> GetEvents(float timeout = 0.01F)
        {
            var events = GetOnEvents().ToList();
            events.AddRange(GetOffEvents());
            return events;
        }
    }

    public class KeyboardEvent
    {
        public VirtualKeyCode KeyCode { get; set; }
        public bool KeyDown { get; set; }
        /// <summary>
        /// Amount of time, in seconds, to wait after processing this event 
        /// before contiuing to process further keyboard events.
        /// </summary>
        public float Timeout { get; set; }
    }
}
