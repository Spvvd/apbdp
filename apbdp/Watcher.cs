namespace apbdp
{
    internal class Watcher : IDisposable
    {
        private readonly string filePath;
        private readonly FileStream fileStream;
        private readonly StreamReader streamReader;
        private readonly object lockObj = new object();
        private bool disposed;
        public event Action<string> OnLogEntry;

        public Watcher(string filePath)
        {
            this.filePath = filePath;

            // Create a new FileStream and StreamReader to read the log file
            fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            streamReader = new StreamReader(fileStream);

            // Move the StreamReader to the end of the file to read only new data
            streamReader.BaseStream.Seek(0, SeekOrigin.End);

            // Start a new thread to continuously read the log file
            var thread = new Thread(ReadLogFile) { IsBackground = true };
            thread.Start();
        }

        private void ReadLogFile()
        {
            while (!disposed)
            {
                lock (lockObj)
                {
                    // Read any new data that has been appended to the end of the file
                    var newLogEntries = streamReader.ReadToEnd();

                    if (!string.IsNullOrEmpty(newLogEntries))
                    {
                        // Raise an event for each new log entry
                        var entries = newLogEntries.Split('\n');
                        foreach (var entry in entries)
                        {
                            OnLogEntry?.Invoke(entry.Replace("\n", "").Replace("\r", ""));
                        }
                    }
                }

                // Wait for a short period before checking for new data again
                Thread.Sleep(5000);
            }
        }

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                streamReader.Dispose();
                fileStream.Dispose();
            }
        }
    }
}
