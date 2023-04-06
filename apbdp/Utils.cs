using System.Diagnostics;

namespace apbdp
{
    class Utils
    {
        public bool CheckGameRunning(string[] gameProcesses)
        {
            bool allProcessesRunning = true;
            Process[] allProcesses = Process.GetProcesses();

            foreach (string processName in gameProcesses)
            {
                bool processRunning = false;
                foreach (Process process in allProcesses)
                {
                    if ($"{process.ProcessName.ToLower()}.exe".EndsWith(processName.ToLower()))
                    {
                        processRunning = true;
                        break;
                    }
                }

                if (!processRunning)
                {
                    allProcessesRunning = false;
                    break;
                }
            }

            return allProcessesRunning;
        }

        public static Process GetGameProcess(string processName)
        {
            foreach (Process process in Process.GetProcesses())
            {
                if ($"{process.ProcessName.ToLower()}.exe".EndsWith(processName.ToLower()))
                {
                    return process;
                }
            }

            return null;
        }

        public string StringListToString(string[] args)
        {
            string arguments = "";
            foreach (string arg in args)
            {
                arguments += " " + arg;
            }
            return arguments;
        }
    }
}
