using CasterUIAutomation.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsInput.Native;

namespace CasterUIAutomation.Actions
{
    public class KeyAction : IAction
    {
        bool initialized = false;
        Keyboard keyboard;

        const char KEY_SEPARATOR = ',';
        char[] DELIMITER_CHARACTERS = new char[] { ':', '/' };
        const char MODIFIER_PREFIX_DELIMITER = '-';
        Dictionary<char, string> MODIFIER_PREFIX_CHARACTERS = new Dictionary<char, string>()
        {
            { 'a', "alt" },
            { 'c', "control" },
            { 's', "shift" },
            { 'w', "win" },
        };
        const float INTERVAL_FACTOR = 0.01F; // pauses for key action are defined in hundredths of seconds
        const float INTERVAL_DEFAULT = 0.0F;

        //public KeyAction(Dictionary<string, string> parameters)
        //{
        //    if (parameters == null)
        //        throw new ArgumentNullException("parameters");
        //    if (!parameters.ContainsKey("spec"))
        //        throw new ArgumentOutOfRangeException("parameters", "Expected parameter 'spec' not found in parameter list.");

        //    Spec = parameters["spec"];
        //}
        //public KeyAction(string spec)
        //{
        //    Spec = spec;
        //}

        public string Spec { get; private set; }

        public void Initialize(Dictionary<string, string> parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters");
            if (!parameters.ContainsKey("spec"))
                throw new ArgumentOutOfRangeException("parameters", "Expected parameter 'spec' not found in parameter list.");

            Initialize(parameters["spec"]);
        }
        public void Initialize(string spec)
        {
            Spec = spec;
            initialized = true;
        }

        public void Execute()
        {
            AssertInitialized();

            var events = ParseSpec();
            GetKeyboard().SendKeyboardEvents(events);
        }

        private Keyboard GetKeyboard()
        {
            if (keyboard == null)
            {
                keyboard = Keyboard.GetActiveKeyboard();
            }
            return keyboard;
        }

        private void AssertInitialized()
        {
            if (!initialized)
                throw new InvalidOperationException("This object must be initialized before it can be used.");
        }

        public IEnumerable<KeyboardEvent> ParseSpec()
        {
            AssertInitialized();

            List<KeyboardEvent> events = new List<KeyboardEvent>();
            foreach (string singleSpec in Spec.Split(KEY_SEPARATOR))
            {
                try
                {
                    events.AddRange(ParseSingle(singleSpec));
                }
                catch (Exception ex)
                {
                    throw new FormatException("Error parsing term '" + singleSpec + "' in spec: " + Spec, ex);
                }
            }
            return events;
        }

        private IEnumerable<KeyboardEvent> ParseSingle(string spec)
        {
            // This method translated as literally as possible from the Dragonfly Key action source
            // to maximize compatibility.

            // From the Dragonfly Key Action documentation, these are the formats we're parsing...
            //
            // Normal press-release key action, optionally repeated several times:
            //     [modifiers -] keyname [/ innerpause] [: repeat] [/ outerpause]
            // Press-and-hold a key, or release a held-down key:
            //     [modifiers -] keyname : direction [/ outerpause]

            spec = spec.Trim();
            if (string.IsNullOrWhiteSpace(spec))
                return new List<KeyboardEvent>();

            // parse modifier prefix, if any
            List<Typeable> modifiers = new List<Typeable>();
            var index = spec.IndexOf(MODIFIER_PREFIX_DELIMITER);
            if (index >= 0)
            {
                var s = spec.Substring(0, index);
                index += 1;
                foreach (char c in s)
                {
                    if (!MODIFIER_PREFIX_CHARACTERS.ContainsKey(c))
                    {
                        throw new FormatException("Spec contains invalid modifier prefix character: " + c);
                    }
                    var m = staticKeyboardMappings[MODIFIER_PREFIX_CHARACTERS[c]];
                    if (modifiers.Contains(m))
                    {
                        throw new FormatException("Spec contains duplicate modifier prefix character: " + c);
                    }
                    modifiers.Add(m);
                }
            }
            else
            {
                index = 0;
                // modifiers = new List<Typeable>();
            }

            string special = null;
            string keyname = null;
            string inner_pause_s = null;
            string outer_pause_s = null;
            float inner_pause = -1;
            float outer_pause = -1;

            // find out what delimiter sequence is used... e.g. "/:", "/:/", ":/", etc... 
            var delimiters = spec.Substring(index)
                .Select((c, i) => new { c, i = i + index})
                .Where(el => DELIMITER_CHARACTERS.Contains(el.c));
            string delimiter_sequence = string.Join("", delimiters.Select(d => d.c));
            var delimiter_index = delimiters.Select(d => d.i).ToArray();

            // and adjust parsing based on the sequence found
            switch (delimiter_sequence)
            {
                case "":
                    keyname = spec.Substring(index);
                    break;
                case "/":
                    keyname = spec.PySubstring(index, delimiter_index[0]);
                    outer_pause_s = spec.Substring(delimiter_index[0] + 1);
                    break;
                case "/:":
                    keyname = spec.PySubstring(index, delimiter_index[0]);
                    inner_pause_s = spec.PySubstring(delimiter_index[0] + 1, delimiter_index[1]);
                    special = spec.Substring(delimiter_index[1] + 1);
                    break;
                case "/:/":
                    keyname = spec.PySubstring(index, delimiter_index[0]);
                    inner_pause_s = spec.PySubstring(delimiter_index[0] + 1, delimiter_index[1]);
                    special = spec.PySubstring(delimiter_index[1] + 1, delimiter_index[2]);
                    outer_pause_s = spec.Substring(delimiter_index[2] + 1);
                    break;
                case ":":
                    keyname = spec.PySubstring(index, delimiter_index[0]);
                    special = spec.Substring(delimiter_index[0] + 1);
                    break;
                case ":/":
                    keyname = spec.PySubstring(index, delimiter_index[0]);
                    special = spec.PySubstring(delimiter_index[0] + 1, delimiter_index[1]);
                    outer_pause_s = spec.Substring(delimiter_index[1] + 1);
                    break;
                default:
                    throw new FormatException("Unrecognized delimiter sequence in spec: " + delimiter_sequence);
            }

            var code = GetTypable(keyname);

            if (inner_pause_s != null)
            {
                if (float.TryParse(inner_pause_s, out inner_pause))
                {
                    inner_pause *= INTERVAL_FACTOR;
                    if (inner_pause < 0)
                        throw new ArgumentOutOfRangeException("Inner pause calculated from spec was less than zero: " + inner_pause_s);
                }
                else
                    throw new FormatException("Invalid inner pause value: '" + inner_pause_s + "' should be a positive number.");
            }

            if (outer_pause_s != null)
            {
                if (float.TryParse(outer_pause_s, out outer_pause))
                {
                    outer_pause *= INTERVAL_FACTOR;
                    if (outer_pause < 0)
                        throw new ArgumentOutOfRangeException("Outer pause calculated from spec was less than zero: " + outer_pause_s);
                }
                else
                    throw new FormatException("Invalid outer pause value: '" + outer_pause_s + "' should be a positive number.");
            }
            else
            {
                outer_pause = INTERVAL_DEFAULT * INTERVAL_FACTOR;
            }

            bool? direction = null;
            int repeat = 1;
            if (special != null)
            {
                if (special == "down") direction = true;
                else if (special == "up") direction = false;
                else
                {
                    if (!int.TryParse(special, out repeat) || repeat < 0)
                    {
                        throw new FormatException("Invalid repeat value: '" + special + "' should be a positive integer.");
                    }
                }
            }

            List<KeyboardEvent> events = new List<KeyboardEvent>();
            if (direction == null)
            {
                if (inner_pause < 0)
                    inner_pause = INTERVAL_DEFAULT * INTERVAL_FACTOR;

                if (repeat > 0)
                {
                    foreach (var m in modifiers)
                    {
                        events.AddRange(m.GetOnEvents());
                    }
                    for (int i = 0; i < repeat - 1; i++)
                    {
                        events.AddRange(code.GetEvents(inner_pause));
                    }
                    events.AddRange(code.GetEvents(outer_pause));
                    foreach (var m in modifiers.ToArray().Reverse())
                    {
                        events.AddRange(m.GetOffEvents());
                    }
                }
            }
            else
            {
                if (modifiers.Count() > 0)
                    throw new FormatException("Cannot use direction with modifiers.");
                if (inner_pause >= 0)
                    throw new FormatException("Cannot use direction with inner pause.");
                if (direction != null && (bool)direction)
                    events = code.GetOnEvents(outer_pause).ToList();
                else
                    events = code.GetOffEvents(outer_pause).ToList();
            }

            return events;
        }

        private Typeable GetTypable(string keyname)
        {
            if (dynamicKeyboardMappings.ContainsKey(keyname))
            {
                return GetKeyboard().GetTypeable(dynamicKeyboardMappings[keyname]);
            }

            if (staticKeyboardMappings.ContainsKey(keyname))
            {
                return staticKeyboardMappings[keyname];
            }

            // TODO: Should we allow for non-english characters here by passing the keyname to keyboard.GetTypeable()?
            //       (This would be an extension to the Dragonfly spec.)

            throw new FormatException("Unrecognized keyname: " + keyname);
        }

        #region Static keyboard mappings
        static Dictionary<string, Typeable> staticKeyboardMappings = new Dictionary<string, Typeable>()
        {
            // Lowercase letter keys
            { "a",                new Typeable(VirtualKeyCode.VK_A) },
            { "alpha",            new Typeable(VirtualKeyCode.VK_A) },
            { "b",                new Typeable(VirtualKeyCode.VK_B) },
            { "bravo",            new Typeable(VirtualKeyCode.VK_B) },
            { "c",                new Typeable(VirtualKeyCode.VK_C) },
            { "charlie",          new Typeable(VirtualKeyCode.VK_C) },
            { "d",                new Typeable(VirtualKeyCode.VK_D) },
            { "delta",            new Typeable(VirtualKeyCode.VK_D) },
            { "e",                new Typeable(VirtualKeyCode.VK_E) },
            { "echo",             new Typeable(VirtualKeyCode.VK_E) },
            { "f",                new Typeable(VirtualKeyCode.VK_F) },
            { "foxtrot",          new Typeable(VirtualKeyCode.VK_F) },
            { "g",                new Typeable(VirtualKeyCode.VK_G) },
            { "golf",             new Typeable(VirtualKeyCode.VK_G) },
            { "h",                new Typeable(VirtualKeyCode.VK_H) },
            { "hotel",            new Typeable(VirtualKeyCode.VK_H) },
            { "i",                new Typeable(VirtualKeyCode.VK_I) },
            { "india",            new Typeable(VirtualKeyCode.VK_I) },
            { "j",                new Typeable(VirtualKeyCode.VK_J) },
            { "juliet",           new Typeable(VirtualKeyCode.VK_J) },
            { "k",                new Typeable(VirtualKeyCode.VK_K) },
            { "kilo",             new Typeable(VirtualKeyCode.VK_K) },
            { "l",                new Typeable(VirtualKeyCode.VK_L) },
            { "lima",             new Typeable(VirtualKeyCode.VK_L) },
            { "m",                new Typeable(VirtualKeyCode.VK_M) },
            { "mike",             new Typeable(VirtualKeyCode.VK_M) },
            { "n",                new Typeable(VirtualKeyCode.VK_N) },
            { "november",         new Typeable(VirtualKeyCode.VK_N) },
            { "o",                new Typeable(VirtualKeyCode.VK_O) },
            { "oscar",            new Typeable(VirtualKeyCode.VK_O) },
            { "p",                new Typeable(VirtualKeyCode.VK_P) },
            { "papa",             new Typeable(VirtualKeyCode.VK_P) },
            { "q",                new Typeable(VirtualKeyCode.VK_Q) },
            { "quebec",           new Typeable(VirtualKeyCode.VK_Q) },
            { "r",                new Typeable(VirtualKeyCode.VK_R) },
            { "romeo",            new Typeable(VirtualKeyCode.VK_R) },
            { "s",                new Typeable(VirtualKeyCode.VK_S) },
            { "sierra",           new Typeable(VirtualKeyCode.VK_S) },
            { "t",                new Typeable(VirtualKeyCode.VK_T) },
            { "tango",            new Typeable(VirtualKeyCode.VK_T) },
            { "u",                new Typeable(VirtualKeyCode.VK_U) },
            { "uniform",          new Typeable(VirtualKeyCode.VK_U) },
            { "v",                new Typeable(VirtualKeyCode.VK_V) },
            { "victor",           new Typeable(VirtualKeyCode.VK_V) },
            { "w",                new Typeable(VirtualKeyCode.VK_W) },
            { "whisky",           new Typeable(VirtualKeyCode.VK_W) },
            { "x",                new Typeable(VirtualKeyCode.VK_X) },
            { "xray",             new Typeable(VirtualKeyCode.VK_X) },
            { "y",                new Typeable(VirtualKeyCode.VK_Y) },
            { "yankee",           new Typeable(VirtualKeyCode.VK_Y) },
            { "z",                new Typeable(VirtualKeyCode.VK_Z) },
            { "zulu",             new Typeable(VirtualKeyCode.VK_Z) },

            // Uppercase letter keys
            { "A",                new Typeable(VirtualKeyCode.VK_A, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },
            { "Alpha",            new Typeable(VirtualKeyCode.VK_A, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },
            { "B",                new Typeable(VirtualKeyCode.VK_B, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },
            { "Bravo",            new Typeable(VirtualKeyCode.VK_B, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },
            { "C",                new Typeable(VirtualKeyCode.VK_C, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },
            { "Charlie",          new Typeable(VirtualKeyCode.VK_C, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },
            { "D",                new Typeable(VirtualKeyCode.VK_D, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },
            { "Delta",            new Typeable(VirtualKeyCode.VK_D, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },
            { "E",                new Typeable(VirtualKeyCode.VK_E, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },
            { "Echo",             new Typeable(VirtualKeyCode.VK_E, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },
            { "F",                new Typeable(VirtualKeyCode.VK_F, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },
            { "Foxtrot",          new Typeable(VirtualKeyCode.VK_F, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },
            { "G",                new Typeable(VirtualKeyCode.VK_G, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },
            { "Golf",             new Typeable(VirtualKeyCode.VK_G, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },
            { "H",                new Typeable(VirtualKeyCode.VK_H, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },
            { "Hotel",            new Typeable(VirtualKeyCode.VK_H, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },
            { "I",                new Typeable(VirtualKeyCode.VK_I, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },
            { "India",            new Typeable(VirtualKeyCode.VK_I, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },
            { "J",                new Typeable(VirtualKeyCode.VK_J, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },
            { "Juliet",           new Typeable(VirtualKeyCode.VK_J, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },
            { "K",                new Typeable(VirtualKeyCode.VK_K, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },
            { "Kilo",             new Typeable(VirtualKeyCode.VK_K, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },
            { "L",                new Typeable(VirtualKeyCode.VK_L, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },
            { "Lima",             new Typeable(VirtualKeyCode.VK_L, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },
            { "M",                new Typeable(VirtualKeyCode.VK_M, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },
            { "Mike",             new Typeable(VirtualKeyCode.VK_M, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },
            { "N",                new Typeable(VirtualKeyCode.VK_N, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },
            { "November",         new Typeable(VirtualKeyCode.VK_N, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },
            { "O",                new Typeable(VirtualKeyCode.VK_O, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },
            { "Oscar",            new Typeable(VirtualKeyCode.VK_O, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },
            { "P",                new Typeable(VirtualKeyCode.VK_P, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },
            { "Papa",             new Typeable(VirtualKeyCode.VK_P, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },
            { "Q",                new Typeable(VirtualKeyCode.VK_Q, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },
            { "Quebec",           new Typeable(VirtualKeyCode.VK_Q, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },
            { "R",                new Typeable(VirtualKeyCode.VK_R, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },
            { "Romeo",            new Typeable(VirtualKeyCode.VK_R, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },
            { "S",                new Typeable(VirtualKeyCode.VK_S, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },
            { "Sierra",           new Typeable(VirtualKeyCode.VK_S, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },
            { "T",                new Typeable(VirtualKeyCode.VK_T, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },
            { "Tango",            new Typeable(VirtualKeyCode.VK_T, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },
            { "U",                new Typeable(VirtualKeyCode.VK_U, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },
            { "Uniform",          new Typeable(VirtualKeyCode.VK_U, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },
            { "V",                new Typeable(VirtualKeyCode.VK_V, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },
            { "Victor",           new Typeable(VirtualKeyCode.VK_V, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },
            { "W",                new Typeable(VirtualKeyCode.VK_W, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },
            { "Whisky",           new Typeable(VirtualKeyCode.VK_W, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },
            { "X",                new Typeable(VirtualKeyCode.VK_X, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },
            { "Xray",             new Typeable(VirtualKeyCode.VK_X, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },
            { "Y",                new Typeable(VirtualKeyCode.VK_Y, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },
            { "Yankee",           new Typeable(VirtualKeyCode.VK_Y, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },
            { "Z",                new Typeable(VirtualKeyCode.VK_Z, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },
            { "Zulu",             new Typeable(VirtualKeyCode.VK_Z, new VirtualKeyCode[]{ VirtualKeyCode.SHIFT }) },

            // Number keys
            { "0",                new Typeable(VirtualKeyCode.VK_0) },
            { "zero",             new Typeable(VirtualKeyCode.VK_0) },
            { "1",                new Typeable(VirtualKeyCode.VK_1) },
            { "one",              new Typeable(VirtualKeyCode.VK_1) },
            { "2",                new Typeable(VirtualKeyCode.VK_2) },
            { "two",              new Typeable(VirtualKeyCode.VK_2) },
            { "3",                new Typeable(VirtualKeyCode.VK_3) },
            { "three",            new Typeable(VirtualKeyCode.VK_3) },
            { "4",                new Typeable(VirtualKeyCode.VK_4) },
            { "four",             new Typeable(VirtualKeyCode.VK_4) },
            { "5",                new Typeable(VirtualKeyCode.VK_5) },
            { "five",             new Typeable(VirtualKeyCode.VK_5) },
            { "6",                new Typeable(VirtualKeyCode.VK_6) },
            { "six",              new Typeable(VirtualKeyCode.VK_6) },
            { "7",                new Typeable(VirtualKeyCode.VK_7) },
            { "seven",            new Typeable(VirtualKeyCode.VK_7) },
            { "8",                new Typeable(VirtualKeyCode.VK_8) },
            { "eight",            new Typeable(VirtualKeyCode.VK_8) },
            { "9",                new Typeable(VirtualKeyCode.VK_9) },
            { "nine",             new Typeable(VirtualKeyCode.VK_9) },

            // Whitespace and editing keys
            { "enter",            new Typeable(VirtualKeyCode.RETURN) },
            { "tab",              new Typeable(VirtualKeyCode.TAB) },
            { "space",            new Typeable(VirtualKeyCode.SPACE) },
            { "backspace",        new Typeable(VirtualKeyCode.BACK) },
            { "delete",           new Typeable(VirtualKeyCode.DELETE) },
            { "del",              new Typeable(VirtualKeyCode.DELETE) },

            // Modifier keys
            { "shift",            new Typeable(VirtualKeyCode.SHIFT) },
            { "control",          new Typeable(VirtualKeyCode.CONTROL) },
            { "ctrl",             new Typeable(VirtualKeyCode.CONTROL) },
            { "alt",              new Typeable(VirtualKeyCode.MENU) },

            // Special keys
            { "escape",           new Typeable(VirtualKeyCode.ESCAPE) },
            { "insert",           new Typeable(VirtualKeyCode.INSERT) },
            { "pause",            new Typeable(VirtualKeyCode.PAUSE) },
            { "win",              new Typeable(VirtualKeyCode.LWIN) },
            { "apps",             new Typeable(VirtualKeyCode.APPS) },
            { "popup",            new Typeable(VirtualKeyCode.APPS) },

            // Navigation keys
            { "up",               new Typeable(VirtualKeyCode.UP) },
            { "down",             new Typeable(VirtualKeyCode.DOWN) },
            { "left",             new Typeable(VirtualKeyCode.LEFT) },
            { "right",            new Typeable(VirtualKeyCode.RIGHT) },
            { "pageup",           new Typeable(VirtualKeyCode.PRIOR) },
            { "pgup",             new Typeable(VirtualKeyCode.PRIOR) },
            { "pagedown",         new Typeable(VirtualKeyCode.NEXT) },
            { "pgdown",           new Typeable(VirtualKeyCode.NEXT) },
            { "home",             new Typeable(VirtualKeyCode.HOME) },
            { "end",              new Typeable(VirtualKeyCode.END) },

            // Number pad keys
            { "npmul",            new Typeable(VirtualKeyCode.MULTIPLY) },
            { "npadd",            new Typeable(VirtualKeyCode.ADD) },
            { "npsep",            new Typeable(VirtualKeyCode.SEPARATOR) },
            { "npsub",            new Typeable(VirtualKeyCode.SUBTRACT) },
            { "npdec",            new Typeable(VirtualKeyCode.DECIMAL) },
            { "npdiv",            new Typeable(VirtualKeyCode.DIVIDE) },
            { "numpad0",          new Typeable(VirtualKeyCode.NUMPAD0) },
            { "np0",              new Typeable(VirtualKeyCode.NUMPAD0) },
            { "numpad1",          new Typeable(VirtualKeyCode.NUMPAD1) },
            { "np1",              new Typeable(VirtualKeyCode.NUMPAD1) },
            { "numpad2",          new Typeable(VirtualKeyCode.NUMPAD2) },
            { "np2",              new Typeable(VirtualKeyCode.NUMPAD2) },
            { "numpad3",          new Typeable(VirtualKeyCode.NUMPAD3) },
            { "np3",              new Typeable(VirtualKeyCode.NUMPAD3) },
            { "numpad4",          new Typeable(VirtualKeyCode.NUMPAD4) },
            { "np4",              new Typeable(VirtualKeyCode.NUMPAD4) },
            { "numpad5",          new Typeable(VirtualKeyCode.NUMPAD5) },
            { "np5",              new Typeable(VirtualKeyCode.NUMPAD5) },
            { "numpad6",          new Typeable(VirtualKeyCode.NUMPAD6) },
            { "np6",              new Typeable(VirtualKeyCode.NUMPAD6) },
            { "numpad7",          new Typeable(VirtualKeyCode.NUMPAD7) },
            { "np7",              new Typeable(VirtualKeyCode.NUMPAD7) },
            { "numpad8",          new Typeable(VirtualKeyCode.NUMPAD8) },
            { "np8",              new Typeable(VirtualKeyCode.NUMPAD8) },
            { "numpad9",          new Typeable(VirtualKeyCode.NUMPAD9) },
            { "np9",              new Typeable(VirtualKeyCode.NUMPAD9) },

            // Function keys
            { "f1",               new Typeable(VirtualKeyCode.F1) },
            { "f2",               new Typeable(VirtualKeyCode.F2) },
            { "f3",               new Typeable(VirtualKeyCode.F3) },
            { "f4",               new Typeable(VirtualKeyCode.F4) },
            { "f5",               new Typeable(VirtualKeyCode.F5) },
            { "f6",               new Typeable(VirtualKeyCode.F6) },
            { "f7",               new Typeable(VirtualKeyCode.F7) },
            { "f8",               new Typeable(VirtualKeyCode.F8) },
            { "f9",               new Typeable(VirtualKeyCode.F9) },
            { "f10",              new Typeable(VirtualKeyCode.F10) },
            { "f11",              new Typeable(VirtualKeyCode.F11) },
            { "f12",              new Typeable(VirtualKeyCode.F12) },
            { "f13",              new Typeable(VirtualKeyCode.F13) },
            { "f14",              new Typeable(VirtualKeyCode.F14) },
            { "f15",              new Typeable(VirtualKeyCode.F15) },
            { "f16",              new Typeable(VirtualKeyCode.F16) },
            { "f17",              new Typeable(VirtualKeyCode.F17) },
            { "f18",              new Typeable(VirtualKeyCode.F18) },
            { "f19",              new Typeable(VirtualKeyCode.F19) },
            { "f20",              new Typeable(VirtualKeyCode.F20) },
            { "f21",              new Typeable(VirtualKeyCode.F21) },
            { "f22",              new Typeable(VirtualKeyCode.F22) },
            { "f23",              new Typeable(VirtualKeyCode.F23) },
            { "f24",              new Typeable(VirtualKeyCode.F24) },

            // Multimedia keys
            { "volumeup",         new Typeable(VirtualKeyCode.VOLUME_UP) },
            { "volup",            new Typeable(VirtualKeyCode.VOLUME_UP) },
            { "volumedown",       new Typeable(VirtualKeyCode.VOLUME_DOWN) },
            { "voldown",          new Typeable(VirtualKeyCode.VOLUME_DOWN) },
            { "volumemute",       new Typeable(VirtualKeyCode.VOLUME_MUTE) },
            { "volmute",          new Typeable(VirtualKeyCode.VOLUME_MUTE) },
            { "tracknext",        new Typeable(VirtualKeyCode.MEDIA_NEXT_TRACK) },
            { "trackprev",        new Typeable(VirtualKeyCode.MEDIA_PREV_TRACK) },
            { "playpause",        new Typeable(VirtualKeyCode.MEDIA_PLAY_PAUSE) },
            { "browserback",      new Typeable(VirtualKeyCode.BROWSER_BACK) },
            { "browserforward",   new Typeable(VirtualKeyCode.BROWSER_FORWARD) }

        };
        #endregion Static keyboard mappings

        #region Dynamic keyboard mappings
        static Dictionary<string, char> dynamicKeyboardMappings = new Dictionary<string, char>()
        {
            { "bang",             '!' },
            { "exclamation",      '!' },
            { "at",               '@' },
            { "hash",             '#' },
            { "dollar",           '$' },
            { "percent",          '%' },
            { "caret",            '^' },
            { "and",              '&' },
            { "ampersand",        '&' },
            { "star",             '*' },
            { "asterisk",         '*' },
            { "leftparen",        '(' },
            { "lparen",           '(' },
            { "rightparen",       ')' },
            { "rparen",           ')' },
            { "minus",            '-' },
            { "hyphen",           '-' },
            { "underscore",       '_' },
            { "plus",             '+' },
            { "backtick",         '`' },
            { "tilde",            '~' },
            { "leftbracket",      '[' },
            { "lbracket",         '[' },
            { "rightbracket",     ']' },
            { "rbracket",         ']' },
            { "leftbrace",        '{' },
            { "lbrace",           '{' },
            { "rightbrace",       '}' },
            { "rbrace",           '}' },
            { "backslash",        '\\' },
            { "bar",              '|' },
            { "colon",            ':' },
            { "semicolon",        ';' },
            { "apostrophe",       '\'' },
            { "singlequote",      '\'' },
            { "squote",           '\'' },
            { "quote",            '"' },
            { "doublequote",      '"' },
            { "dquote",           '"' },
            { "comma",            ',' },
            { "dot",              '.' },
            { "slash",            '/' },
            { "lessthan",         '<' },
            { "leftangle",        '<' },
            { "langle",           '<' },
            { "greaterthan",      '>' },
            { "rightangle",       '>' },
            { "rangle",           '>' },
            { "question",         '?' },
            { "equal",            '=' },
            { "equals",           '=' },
        };
        #endregion Dynamic keyboard mappings

    }
}
