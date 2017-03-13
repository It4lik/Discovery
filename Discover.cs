using System.Threading;
using System;
using System.Collections.Generic;

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
        private Stocker _redis; // Holds the current redis connection.
        private string _discoveryName; // Arbitrary name.
        public Discover(CheckType checkType, Subnet targetedSubnet, string discoveryName) {
            // Initalize object properties
            _checkType = checkType;
            _targetedSubnet = targetedSubnet;
            _discoveryName = discoveryName;
            _cts = new CancellationTokenSource();
            // Initialize the _discovery thread with targeted check and subnet (checkType, targetedSubnet)
            initializeDiscoveryThread();
        }

        public void startDiscovery() {
            // Used to start the thread that issue scans
            _discovery.Start();
            Console.WriteLine("DEBUG: Thread {0} started.", _discoveryName); // Debug console output
        }

        public void stopDiscovery() {
            // Used to stop the thread tha issue scans (and cleanup afterwards)
            Console.WriteLine("DEBUG: STOP CATCHED ! Aborting {0} thread.", _discoveryName); // Debug console output
            _cts.Cancel();
            Console.WriteLine("DEBUG: Thread {0} aborted.", _discoveryName); // Debug console output
        }
        private void initializeDiscoveryThread() {
            switch (_checkType) {
                // _cts.Token is always sent : it is used to stop threads
                case CheckType.tcp:
                    // Instanciate the thread with discoveryTcp method.
                    _discovery = new Thread(delegate() {
                        discoveryTcp(_targetedSubnet, _cts.Token);
                    });
                    Console.WriteLine("DEBUG: Thread {0} initialized.", _discoveryName); // Debug console output

                    break;
                
                default:
                    // Default is TCP check. See case CheckType.tcp:
                    _discovery = new Thread(delegate() {
                        discoveryTcp(_targetedSubnet, _cts.Token);
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

            //while (true) {
            for (int i = 0; i < 2; i++) {
                // Used to store aliveHosts detected by check
                List<string> aliveHosts = new List<string>();

                if(cancelToken.IsCancellationRequested) {
                    // If stopDiscovery() was called. That means that someone wants to stop the discovery, and thus, this thread. 
                    // Cleanup and exit the while loop.  
                    Console.WriteLine("DEBUG: Cancel method called on {0} thread. Cancellation requested.", _discoveryName); // Debug console output
                    return;
                }
                aliveHosts = Scan.TCPScan(_targetedSubnet.getAllIPsInSubnet(), 443);

            // BELOW is wanted logic

                // vocab : "alive host" : currently alive hosts seen by TCPScan
                // vocab : "existing host" : host that is currently in Redis

                // retrieve alive hosts with TCPScan : Scan.TCPScan(_targetedSubnet.getAllIPs())
                // test if hosts exists
                    // if yes, continue
                    // if no, store the host in redis
                // retrieve list of existing hosts in Redis
                // if a host in that list is NOT in the string list of alive hosts : delete it from redis 

            }
            Console.WriteLine("DEBUG: Out of loop in {0} thread.", _discoveryName); // Debug console output
        }
    }
}