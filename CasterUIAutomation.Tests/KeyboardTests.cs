using CasterUIAutomation.Win32;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsInput.Native;

namespace CasterUIAutomation.Tests
{
    [TestClass]
    public class KeyboardTests
    {
        [TestMethod]
        public void TestKeyCodeConversion()
        {
            TestKeyCodeConversion('A', new Typeable(VirtualKeyCode.VK_A, new VirtualKeyCode[] { VirtualKeyCode.SHIFT }));
            TestKeyCodeConversion('z', new Typeable(VirtualKeyCode.VK_Z));
            TestKeyCodeConversion('3', new Typeable(VirtualKeyCode.VK_3));
            TestKeyCodeConversion('\r', new Typeable(VirtualKeyCode.RETURN));

            // NOTE: everything below this line depends on the keyboard layout and may fail on anything other than a standard US keyboard layout (not tested yet)
            // TODO: maybe explicitly set keyboard layout to make sure tests using extended key codes pass
            TestKeyCodeConversion('%', new Typeable(VirtualKeyCode.VK_5, new VirtualKeyCode[] { VirtualKeyCode.SHIFT }));
            TestKeyCodeConversion(':', new Typeable(VirtualKeyCode.OEM_1, new VirtualKeyCode[] { VirtualKeyCode.SHIFT }));
            TestKeyCodeConversion(';', new Typeable(VirtualKeyCode.OEM_1));
            TestKeyCodeConversion('/', new Typeable(VirtualKeyCode.OEM_2));
            TestKeyCodeConversion('?', new Typeable(VirtualKeyCode.OEM_2, new VirtualKeyCode[] { VirtualKeyCode.SHIFT }));
            TestKeyCodeConversion('\\', new Typeable(VirtualKeyCode.OEM_5));
            TestKeyCodeConversion('|', new Typeable(VirtualKeyCode.OEM_5, new VirtualKeyCode[] { VirtualKeyCode.SHIFT }));
            TestKeyCodeConversion('\'', new Typeable(VirtualKeyCode.OEM_7));
            TestKeyCodeConversion('"', new Typeable(VirtualKeyCode.OEM_7, new VirtualKeyCode[] { VirtualKeyCode.SHIFT }));
        }

        [TestMethod]
        public void TestInternationalKeyCodeConversion()
        {
            throw new NotImplementedException();
        }

        private void TestKeyCodeConversion(char character, Typeable key)
        {
            var typeable = Keyboard.GetActiveKeyboard().GetTypeable(character);
            Assert.AreEqual(key.Key, typeable.Key, "Key code doesn't match for char: " + character);
            Assert.IsNotNull(typeable.Modifiers, "Modifiers returned null for char: " + character);
            if (key.Modifiers != null && key.Modifiers.Length > 0)
            {
                foreach (var modifier in key.Modifiers)
                {
                    Assert.IsTrue(typeable.Modifiers.Contains(modifier), "Modifiers list  for char '" + character + "' is missing modifier: " + modifier.ToString());
                }
                Assert.AreEqual(key.Modifiers.Length, typeable.Modifiers.Length, "Modifier list count doesn't match for char: " + character);
            }
        }
    }
}
