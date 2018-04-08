using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CasterUIAutomation
{
    static class StringExtensions
    {
        /// <summary>
        /// Simple extension method to emulate the use of list comprehensions on
        /// strings in Python to make code translation a little less error prone.
        /// For example myString[1:5] can be translated as myString.PySubstring(1, 5);
        /// </summary>
        public static string PySubstring(this string s, int startIndex, int endIndex)
        {
            return s.Substring(startIndex, endIndex - startIndex);
        }
    }
}
