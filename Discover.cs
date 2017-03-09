using System.Threading;
using System;

namespace discovery
{
    public class Discover
    { 
        // Class that we can instantiate using "Check" class as its constructor parameter
        // Discover has a Thread property : an instance of Discover use a Thread to issue checks in a subnet
        private Thread _discovery;

        public Discover() {

        }
        private void discoveryCustom(string command) {
            // thread that issue shell commands
        }

        private void discoveryTcp() {
            // thread that open TCP connections
        }
    }
}