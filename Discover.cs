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
            while (true) {
                // vocab : "alive host" : currently alive hosts seen by TCPScan
                // vocab : "existing host" : host that is currently in Redis

                // retrieve alive hosts with TCPScan
                // test if hosts exists
                    // if yes, continue
                    // if no, store the host in redis
                // retrieve list of existing hosts in Redis
                // if a host in that list is NOT in the string list of alive hosts : delete it from redis 

            }
        }
    }
}