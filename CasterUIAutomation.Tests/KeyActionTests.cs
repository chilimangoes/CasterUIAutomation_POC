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
        public void TestParsingSimple()
        {
            TestParsing("d", null, VirtualKeyCode.VK_D);
            TestParsing("c-s", control, VirtualKeyCode.VK_S);
            TestParsing("s-f1", shift, VirtualKeyCode.F1);
            TestParsing("a-3", alt, VirtualKeyCode.VK_3);
            TestParsing("cs-z", controlShift, VirtualKeyCode.VK_Z);
        }

        [TestMethod]
        public void TestParsingSymbols()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestParsingPressAndHold()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestParsingTimings()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestParsingInternationalKeyboardLayouts()
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

            KeyAction action = new KeyAction(spec);
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
