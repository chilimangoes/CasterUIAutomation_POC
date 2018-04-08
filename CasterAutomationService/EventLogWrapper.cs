using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CasterAutomationService
{
    class EventLogWrapper
    {
        EventLog log;
        string partialMessage = "";

        public EventLogWrapper(string source, string logName)
        {
            log = new EventLog();
            //EventLog.Delete("CasterUIAutomation");
            //EventLog.DeleteEventSource("CasterUIAutomation");
            //EventLog.CreateEventSource("CasterUIAutomation", "CasterUIAutomation");
            if (!EventLog.SourceExists(source))
            {
                EventLog.CreateEventSource(source, logName);
            }
            log.Source = source;
            log.Log = logName;
        }

        public void WritePartialEntry(string partialMessage, bool newLine = true)
        {
            this.partialMessage += partialMessage;
            if (newLine)
            {
                this.partialMessage += "\r\n";
            }
        }
        public void WriteEntry(string message)
        {
            WriteEntry(message, EventLogEntryType.Information);
        }
        public void WriteEntry(string message, EventLogEntryType type)
        {
            partialMessage += message;
            log.WriteEntry(partialMessage, type);
            partialMessage = "";
        }
    }
}
