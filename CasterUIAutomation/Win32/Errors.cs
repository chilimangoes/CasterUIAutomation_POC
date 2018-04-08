using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace CasterUIAutomation.Win32
{

    class Errors
    {
        public static Exception GetWin32Exception(string context)
        {
            int error = Marshal.GetLastWin32Error();
            Win32Exception innerException = new Win32Exception(error);

            string message = String.Format("{0}: (Error Code {1}) {2}", context, error, innerException.Message);
            return new Win32Exception(error, message);
        }
    }

    // TODO: refactor this out
    internal class Win32ErrorCodeException : Win32Exception
    {
        string message;

        internal Win32ErrorCodeException(string context)
        {
            int error = Marshal.GetLastWin32Error();
            Win32Exception innerException = new Win32Exception(error);

            message = String.Format("{0}: (Error Code {1}) {2}", context, error, innerException.Message);
        }

        public override string Message
        {
            get { return message; }
        }
    }
}
