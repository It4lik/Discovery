using System;
using System.IO;

namespace discovery
{
    public class ConfWatcher
    { 
        /// FileSystemWatcher used to watch config file for changes
        private FileSystemWatcher _confWatcher;

        /// Use a string (path configuration file) to instanciate a FileSystemWatcher object
        public ConfWatcher(string configurationFile) {
            // Instantiate the FileSystemWatcher
            _confWatcher = new FileSystemWatcher();
            // Filter only some of the operations (needed to detect changes)
            _confWatcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size;

            // Target the config file
            _confWatcher.Path = Path.GetDirectoryName(configurationFile); 
            _confWatcher.Filter = Path.GetFileName(configurationFile);

            // Set the handler when file changed
            _confWatcher.Changed += new FileSystemEventHandler(ConfChanged);
            _confWatcher.EnableRaisingEvents = true;

        }

        private static void ConfChanged(object source, FileSystemEventArgs e) {
            Console.WriteLine("hop");
            return;
        }
    }
}