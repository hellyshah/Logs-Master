using System;
using System.Diagnostics;
using System.Management;

namespace Logs
{
    class CommonModule
    {
        public bool WriteToEventLog(string Entry, string AppName = "_ELogs", EventLogEntryType EventType = EventLogEntryType.Information, string LogName = "Application")
        {

            EventLog objEventLog = new EventLog();

            try
            {
                if (!System.Diagnostics.EventLog.SourceExists(AppName))
                {
                    System.Diagnostics.EventLog.CreateEventSource(AppName, LogName);
                }
                objEventLog.Source = AppName;
                objEventLog.WriteEntry(Entry, EventType);
                return true;
            }
            catch (Exception Ex)
            {
                return false;
            }
        }

        //Emuns For SessionEvents and Clock_Status
        public enum sessionEvents
        {
            Logon = 0,
            Logout,
            Lock,
            UnLock,
            Improper_ShutDown,
            RemoteConnect,
            RemoteDisconnect
        }
        public enum Clock
        {
            clockout=0,
            clockin=1,            
        }

        //Event for getting User Info using WMI
        public string GetInfo()
        {
            string username = null;
            try
            {
                // Define WMI scope to look for the Win32_ComputerSystem object
                ManagementScope ms = new ManagementScope("\\\\.\\root\\cimv2");
                ms.Connect();
                ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_ComputerSystem");
                ManagementObjectSearcher searcher =new ManagementObjectSearcher(ms, query);
                // This loop will only run at most once.
                foreach (ManagementObject mo in searcher.Get())
                {
                    // Extract the username
                    username = mo["UserName"].ToString();
                }
                // Remove the domain part from the username
                string[] usernameParts = username.Split('\\');
                // The username is contained in the last string portion.
                username = usernameParts[usernameParts.Length - 1];
            }
            catch (Exception e)
            {
                // The system currently has no users who are logged on
                // Set the username to "SYSTEM" to denote that
                WriteToEventLog("In Machine Class : " + e.Message);
                username = "Exception Here.......";
            }
            return username;
        }     
    }
}
