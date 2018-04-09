using System;
using System.Collections.Generic;
using System.Linq;
using CasterUIAutomation.Actions;
using CasterUIAutomation.Win32;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowsInput.Native;

namespace CasterUIAutomation.Tests
{
    [TestClass]
    public class KeyActionTests
    {
        // From the Dragonfly Key Action documentation, these are the formats we're parsing...
        //
        // Normal press-release key action, optionally repeated several times:
        //     [modifiers -] keyname [/ innerpause] [: repeat] [/ outerpause]
        // Press-and-hold a key, or release a held-down key:
        //     [modifiers -] keyname : direction [/ outerpause]

        VirtualKeyCode[] alt = new VirtualKeyCode[] { VirtualKeyCode.MENU };
        VirtualKeyCode[] control = new VirtualKeyCode[] { VirtualKeyCode.CONTROL };
        VirtualKeyCode[] shift = new VirtualKeyCode[] { VirtualKeyCode.SHIFT };
        VirtualKeyCode[] controlShift = new VirtualKeyCode[] { VirtualKeyCode.CONTROL, VirtualKeyCode.SHIFT };

        [TestMethod]
        public void KeyAction_Simple()
        {
            TestParsing("d", null, VirtualKeyCode.VK_D);
            TestParsing("c-s", control, VirtualKeyCode.VK_S);
            TestParsing("s-f1", shift, VirtualKeyCode.F1);
            TestParsing("a-3", alt, VirtualKeyCode.VK_3);
            TestParsing("cs-z", controlShift, VirtualKeyCode.VK_Z);
            TestParsing("left", null, VirtualKeyCode.LEFT);
            TestParsing("s-left", shift, VirtualKeyCode.LEFT);
        }

        [TestMethod]
        public void KeyAction_Complex()
        {
            string spec = "t, s-left/50:2/100";
            KeyboardEvent[] expected = new KeyboardEvent[]
            {
                new KeyboardEvent { KeyCode=VirtualKeyCode.VK_T, KeyDown=true, Timeout=0F },
                new KeyboardEvent { KeyCode=VirtualKeyCode.VK_T, KeyDown=false, Timeout=0F },
                new KeyboardEvent { KeyCode=VirtualKeyCode.SHIFT, KeyDown=true, Timeout=0.01F },
                new KeyboardEvent { KeyCode=VirtualKeyCode.LEFT, KeyDown=true, Timeout=0F },
                new KeyboardEvent { KeyCode=VirtualKeyCode.LEFT, KeyDown=false, Timeout=0.5F },
                new KeyboardEvent { KeyCode=VirtualKeyCode.LEFT, KeyDown=true, Timeout=0F },
                new KeyboardEvent { KeyCode=VirtualKeyCode.LEFT, KeyDown=false, Timeout=1F },
                new KeyboardEvent { KeyCode=VirtualKeyCode.SHIFT, KeyDown=false, Timeout=0.01F },
            };

            TestParsing(spec, expected, false);
        }

        [TestMethod]
        public void KeyAction_PressAndHold()
        {
            string spec = "shift:down/75";
            KeyboardEvent[] expected = new KeyboardEvent[]
            {
                new KeyboardEvent { KeyCode=VirtualKeyCode.SHIFT, KeyDown=true, Timeout=0.75F },
            };
            TestParsing(spec, expected, false);

            spec = "alt:up";
            expected = new KeyboardEvent[]
            {
                new KeyboardEvent { KeyCode=VirtualKeyCode.MENU, KeyDown=false, Timeout=0F },
            };
            TestParsing(spec, expected, false);

        }

        [TestMethod]
        public void KeyAction_InternationalKeyboardLayouts()
        {
            throw new NotImplementedException();
        }

        private void TestParsing(string spec, IEnumerable<VirtualKeyCode> modifiers, VirtualKeyCode key)
        {
            TestParsing(spec, modifiers, new VirtualKeyCode[] { key });
        }

        private void TestParsing(string spec, IEnumerable<VirtualKeyCode> modifiers, IEnumerable<VirtualKeyCode> key)
        {
            List<KeyboardEvent> events = new List<KeyboardEvent>();

            // key down events
            if (modifiers != null)
            {
                events.AddRange(modifiers.Select(k => new KeyboardEvent { KeyCode = k, KeyDown = true }));
            }
            events.AddRange(key.Select(k => new KeyboardEvent { KeyCode = k, KeyDown = true }));

            // key up events
            events.AddRange(key.Reverse().Select(k => new KeyboardEvent { KeyCode = k, KeyDown = false }));
            if (modifiers != null)
            {
                events.AddRange(modifiers.Reverse().Select(k => new KeyboardEvent { KeyCode = k, KeyDown = false }));
            }

            TestParsing(spec, events);
        }

        private void TestParsing(string spec, IEnumerable<KeyboardEvent> expectedEvents, bool ignoreTimings = true)
        {
            var expected = expectedEvents.ToArray();

            KeyAction action = new KeyAction();
            action.Initialize(spec);
            var actual = action.ParseSpec().ToArray();

            Assert.AreEqual(expected.Count(), actual.Count(), "Event count mismatch for spec: " + spec);

            for (int i = 0; i < expected.Length; i++)
            {
                var e = expected[i];
                var a = actual[i];

                Assert.AreEqual(e.KeyCode, a.KeyCode, "Key code mismatch at position " + i + " for spec: " + spec);
                Assert.AreEqual(e.KeyDown, a.KeyDown, "Key down mismatch at position " + i + " for spec: " + spec);
                if (!ignoreTimings)
                {
                    Assert.AreEqual(e.Timeout, a.Timeout, "Timout mismatch at position " + i + " for spec: " + spec);
                }
            }
        }

    }
}
