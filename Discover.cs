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
        private Thread _discovery; // Thread used to issue checks in subnet and r/w in Redis. // THIS IS GOING TO DISAPPEAR
        private List<Thread> _discoveries; // These are every threads used to scan each Subnet in _shrunkSubnets
        private CancellationTokenSource _cts; // CancellationTokenSource used to properly stop threads (eg checks).
        private CheckType _checkType; // Check to use on subnet's hosts.
        private Subnet _targetedSubnet; // Subnet on which issue checks.
        private List<Subnet> _shrunkSubnets;  // This is obtained by shrinking the _targetedSubnet
        private int _targetedPort; // Targeted port of hosts.
        private Stocker _redis; // Holds the current redis connection.
        private HostAction _actionIfUp; // Holds the action to do when finding a host
        private HostAction _actionIfDown; // Holds the action to do when a host disapppeared
        private string _discoveryName; // Arbitrary name.
        public Discover(CheckType checkType, Subnet targetedSubnet, int port, HostAction actionIfUp, HostAction actionIfDown, Stocker redis, string discoveryName) {
            // Initalize object properties
            _checkType = checkType;
            _targetedSubnet = targetedSubnet;
            _targetedPort = port;
            _redis = redis;
            _actionIfUp = actionIfUp;
            _actionIfDown = actionIfDown;
            _discoveryName = discoveryName;
            _cts = new CancellationTokenSource();

            // HERE shrink network

            // Initialize every _discoveries Thread with targeted CheckType and subnets in _shrunkSubnets
            initializeDiscoveriesThreads();
        }

        public void startDiscovery() {
            // Used to start the thread that issue scans

            // Implementing subnet shrinking :
            //   - this must be a for each in _discoveries list
            //   - each thread in _discoveries list must be started

            _discovery.Start();
            Console.WriteLine("DEBUG: Thread {0} started.", _discoveryName); // Debug console output
        }

        public void stopDiscovery() {
            // Used to stop the thread tha issue scans (and cleanup afterwards)
            Console.WriteLine("DEBUG: STOP CATCHED ! Aborting {0} thread.", _discoveryName); // Debug console output
            _cts.Cancel();
            Console.WriteLine("DEBUG: Thread {0} aborted.", _discoveryName); // Debug console output
        }
        private void initializeDiscoveriesThreads() {

            // Implementing subnet shrinking : 
            //   - this must be a foreach on _shrunkSubnets
            //   - each iteration initialize a thread in _discoveries
            switch (_checkType) {
                // _cts.Token is always sent : it is used to stop threads
                case CheckType.tcp:
                    // Instanciate the thread with discoveryTcp method.
                    _discovery = new Thread(delegate() {
                        discoveryTcp(_cts.Token);
                    });
                    Console.WriteLine("DEBUG: Thread {0} initialized.", _discoveryName); // Debug console output

                    break;
                
                default:
                    // Default is TCP check. See case CheckType.tcp:
                    _discovery = new Thread(delegate() {
                        discoveryTcp(_cts.Token);
                    });
                    
                    break;
            }
        }
        
        private void discoveryCustom(string command) {
            // thread that issue shell commands
        }

        private void discoveryTcp(CancellationToken cancelToken) {
            // thread that open TCP connections

            List<string> aliveHosts = new List<string>();
            List<string> existingHosts = new List<string>();

            
            //while (true) {
            for (int i = 0; i < 1; i++) {
                // Used to store aliveHosts detected by check
                aliveHosts = Scan.TCPScan(_targetedSubnet.getAllIPsInSubnet(), _targetedPort);
                existingHosts =  _redis.GetSubnetHosts(_discoveryName);
                
                if(cancelToken.IsCancellationRequested) {
                    // If stopDiscovery() was called. That means that someone wants to stop the discovery, and thus, this thread. 
                    // Cleanup and exit the while loop.  
                    Console.WriteLine("DEBUG: Cancel method called on {0} thread. Cancellation requested.", _discoveryName); // Debug console output
                    return;
                }

                /*
                // Delete host in redis if it's not alive
                // For each hosts in Redis (existingHost list)
                //  - print not alive host in console
                //  - mark the host as "DOWN" in redis
                //  - execute _actionIfDown
                */
                foreach (string existingHost in existingHosts) {
                    // If the host is in redis as "UP", but not in current alive hosts list
                    if ( (! aliveHosts.Contains(existingHost)) && (_redis.Read(_discoveryName, existingHost) == "UP") ) {
                        Console.WriteLine("{0}: Deleting {1} which is not alive.", _discoveryName, existingHost); // Debug console output
                        // Mark the host as down in redis
                        _redis.markHostDown(_discoveryName, existingHost);
                        // Execute action _actionIfDown
                        _actionIfDown.Execute(existingHost);
                    }
                }

                /* 
                // Work on current alive hosts :
                //  - print found host in console
                //  - add found host in Redis if not exists, as "UP"
                //  - execute _actionIfUp 
                */
                foreach (string aliveHost in aliveHosts) {
                    // If the host is alive and doesn't exist in redis or marked as DOWN
                    if ( (! _redis.doesKeyExist(_discoveryName, aliveHost)) || (_redis.Read(_discoveryName, aliveHost) == "DOWN")) {
                        Console.WriteLine("{0}: Host {1} was not in Redis. Adding it.", _discoveryName, aliveHost); // Debug console output
                        // Mark the host as UP in redis
                        _redis.markHostUp(_discoveryName, aliveHost);
                        // Execute the _actionIfUp action
                        _actionIfUp.Execute(aliveHost);
                    }
                    else {
                        Console.WriteLine("{0}: Host {1} was already in Redis ! Value : {2}", _discoveryName, aliveHost, _redis.Read(_discoveryName, aliveHost)); // Debug console output
                    }
                }
            }
            Console.WriteLine("DEBUG: Out of loop in {0} thread. Self-destructin.", _discoveryName); // Debug console output
            return;
        }
    }
}