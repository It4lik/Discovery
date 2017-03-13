using System.Threading;
using System;

namespace discovery
{
    public class Discover
    { 
        // Class that we can instantiate using "Check" class as its constructor parameter
        // Discover has a Thread property : an instance of Discover use a Thread to issue checks in a subnet
        
        public enum CheckType { tcp };
        private Thread _discovery; // Thread used to issue checks in subnet and r/w in Redis.
        private CancellationTokenSource _cts; // CancellationTokenSource used to properly stop threads (eg checks).
        private CheckType _checkType; // Check to use on subnet's hosts.
        private Subnet _targetedSubnet; // Subnet on which issue checks. 
        public Discover(CheckType checkType, Subnet targetedSubnet) {
            // Initalize object properties
            _checkType = checkType;
            _targetedSubnet = targetedSubnet;
            _cts = new CancellationTokenSource();
            // Initialize the _discovery thread with targeted check and subnet (checkType, targetedSubnet)
            initializeDiscoveryThread(_checkType, _targetedSubnet, _cts, _discovery);
        }

        public void startDiscovery(Thread discovery) {
            // Used to start the thread that issue scans
            discovery.Start();
            Console.WriteLine("Thread started."); // Debug console output
        }

        public void stopDiscovery(CancellationTokenSource cts) {
            // Used to stop the thread tha issue scans (and cleanup afterwards)
            Console.WriteLine("STOP CATCHED ! Aborting the thread."); // Debug console output
            cts.Cancel();
            cts.Dispose();
            Console.WriteLine("Thread aborted."); // Debug console output
        }
        private void initializeDiscoveryThread(CheckType checkType, Subnet targetedSubnet, CancellationTokenSource cts, Thread discovery) {
            switch (checkType) {
                // _cts.Token is always sent : it is used to stop threads
                case CheckType.tcp:
                    // Instanciate the thread with discoveryTcp method.
                    discovery = new Thread(delegate() {
                        discoveryTcp(targetedSubnet, cts.Token);
                    });
                    break;
                
                default:
                    // Default is TCP check. See case CheckType.tcp:
                    discovery = new Thread(delegate() {
                        discoveryTcp(_targetedSubnet, cts.Token);
                    });
                    break;
            }
        }
        
        private void discoveryCustom(string command) {
            // thread that issue shell commands
        }

        private void discoveryTcp(Subnet targetedSubnet, CancellationToken cancelToken) {
            // thread that open TCP connections

            //  ####  TEMP CODE  ####  //

            while (true) {

                if(cancelToken.IsCancellationRequested) {
                    // If stopDiscovery() was called. That means that someone wants to stop the discovery, and thus, this thread. 
                    // Cleanup and exit the while loop.  
                    Console.WriteLine("Cancel method called. Cancellation requested."); // Debug console output
                    return;
                }

                // vocab : "alive host" : currently alive hosts seen by TCPScan
                // vocab : "existing host" : host that is currently in Redis

                // retrieve alive hosts with TCPScan : Scan.TCPScan(_targetedSubnet.getAllIPs())
                // test if hosts exists
                    // if yes, continue
                    // if no, store the host in redis
                // retrieve list of existing hosts in Redis
                // if a host in that list is NOT in the string list of alive hosts : delete it from redis 
            }
        }
    }
}