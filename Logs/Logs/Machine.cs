using System;
using System.Management;

namespace Logs
{
    class Machine
    {
        CommonModule obj = new CommonModule();
        private static Object _classLocker = new Object();
        private static Machine _machine;

        private Machine()
        {
        } // end private Machine()

        public static Machine getInstance()
        {
            if (_machine == null)
            {
                lock (_classLocker)
                {
                    if (_machine == null)
                    {
                        _machine = new Machine();
                    }
                }
            }
            return _machine;
        } // end public static Machine getInstance()

        public String getUsername()
        {
            string username = null;
            try
            {
                // Define WMI scope to look for the Win32_ComputerSystem object
                ManagementScope ms = new ManagementScope("\\\\.\\root\\cimv2");
                ms.Connect();

                ObjectQuery query = new ObjectQuery
                        ("SELECT * FROM Win32_ComputerSystem");
                ManagementObjectSearcher searcher =
                        new ManagementObjectSearcher(ms, query);

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
                obj.WriteToEventLog("In Machine Class : "+e.Message);

                username = "LOGOUT";
            }
            return username;
        }
    }
}
