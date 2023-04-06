using System.Diagnostics;

namespace apbdp
{
    class apbdp
    {
        Utils Utils = new Utils();

        private static string gameProcess = "apb.exe"; 
        private static string[] gameProcesses = { gameProcess, "apb_be.exe", "apb_catcher.exe" };

#if RELEASE
        private static string binariesPath = $"Binaries";
        private static string logPath = "APBGame\\Logs\\Current.log";
#elif DEBUG
        private static string binariesPath = $"D:\\Games\\APB13\\Binaries";
        private static string logPath = "D:\\Games\\APB13\\APBGame\\Logs\\Current.log";
#endif

        static void Main(string[] args)
        {
            apbdp instance = new apbdp();
            instance.Run(args);
        }

        private void Run(string[] args)
        {
            // Start apb process with args
            Process.Start($"{binariesPath}\\{gameProcess}", Utils.StringListToString(args));

            // Check if all game / ac processes are running
            while (!Utils.CheckGameRunning(gameProcesses)) Thread.Sleep(1000);

            // Init Discord Presence
            var discord = new Discord();

            // Init Watcher and listen to log file
            var parser = new Watcher(logPath);
            var handler = new LogHandler(discord);

            // Listen to the OnLogEntry event and parse when raised
            parser.OnLogEntry += (line) =>
            {
#if DEBUG
                Console.WriteLine(line);
#endif          
                handler.Parse(line);
            };

            // Get game process again since the anti-cheat restarts the main process
            // and wait until it is exited
            Utils.GetGameProcess(gameProcess).WaitForExit();
        }
    } 
}