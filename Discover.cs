using System.Threading;
using System;
using System.Collections.Generic;
using System.Linq;

namespace discovery
{
    public class Discover
    { 
        // Class that we can instantiate using "Check" class as its constructor parameter
        // Discover has a Thread property : an instance of Discover use a Thread to issue checks in a subnet
        
        public enum CheckType { tcp };
        /// These are every threads used to scan each Subnet in _shrunkSubnets
        private CancellationTokenSource _cts; 
        /// Check to use on subnet's hosts.
        private CheckType _checkType;
        /// Subnet on which issue checks.
        private Subnet _targetedSubnet; 
        /// This is obtained by shrinking the _targetedSubnet
        public List<Subnet> _shrunkSubnets {get; set;}
        /// The mask wanted to shrink the subnet (one thread will take care of each subnet)
        private int _shrunkNetworksCIDRMask;
        /// Maximum number of threads that can for on subnet
        private int _maxThreads; 
        /// Targeted port of hosts.
        private int _targetedPort; 
        /// Holds the current redis connection
        private Stocker _redis; 
        /// Holds the action to do when finding a host
        private HostAction _actionIfUp; 
        /// Holds the action to do when a host disapppeared
        private HostAction _actionIfDown; 
        /// Arbitrary name
        private string _discoveryName; 
        public Discover(CheckType checkType, Subnet targetedSubnet, int port, HostAction actionIfUp, HostAction actionIfDown, Stocker redis, string discoveryName, int maxThreads = 3, int shrunkNetworksCIDRMask = 24) {
            // Initalize object properties
            _checkType = checkType;
            _targetedSubnet = targetedSubnet;
            _targetedPort = port;
            _redis = redis;
            _actionIfUp = actionIfUp;
            _actionIfDown = actionIfDown;
            _discoveryName = discoveryName;
            _cts = new CancellationTokenSource();

            // Default
            _maxThreads = maxThreads;

            if (shrunkNetworksCIDRMask > _targetedSubnet._maskCIDR) {
                // Shrinking initial subnets in multiple smaller ones
                _shrunkNetworksCIDRMask = shrunkNetworksCIDRMask;
                _shrunkSubnets = _targetedSubnet.Shrink(_shrunkNetworksCIDRMask);
            }
            else if (shrunkNetworksCIDRMask == _targetedSubnet._maskCIDR) {
                _shrunkSubnets.Add(_targetedSubnet);
            }
            else {
                Console.WriteLine("FATAL: You can't shrink the initial network in bigger ones. Exiting.");
                System.Environment.Exit(7);
            }

            /* foreach (Subnet net in _shrunkSubnets) {
                // Debug console output
                Console.WriteLine("One shrunk network is {0}/{1}. Type : {2}.", net._networkIP, net._maskCIDR, net._type);
            } */
        }

        /// Used to start threads that issue scans
        public void startDiscovery() {
            int threadId = 0;
            
            //while (true) {
            for (int i = 0; i < 1; i++) {
                // Parralel iteration on each shrunkSubnet, using multithreading. 
			    _shrunkSubnets.AsParallel().WithDegreeOfParallelism(_maxThreads).WithCancellation(_cts.Token).ForAll(shrunkSubnet => {
                    // te permet de récupérer l'ID du Thread en cours, je ne l'ai pas testé donc je ne sais pas ce que ça va donner dans ton cas :D
                    threadId++;
                    switch (_checkType) {
                        // _cts.Token is always sent : it is used to stop threads
                        case CheckType.tcp:
                        default:
                            discoveryTcp(_cts.Token, shrunkSubnet, threadId);
                            break;
                    }
                });
            }
        }

        public void stopDiscovery() {
            // Used to stop the thread tha issue scans (and cleanup afterwards)
            Console.WriteLine("DEBUG: STOP CATCHED ! Aborting {0} thread.", _discoveryName); // Debug console output
            _cts.Cancel();
            Console.WriteLine("DEBUG: Thread {0} aborted.", _discoveryName); // Debug console output
        }
        private void discoveryCustom(string command) {
            // thread that issue shell commands
        }

        /// Method used by TCP check threads
        private void discoveryTcp(CancellationToken cancelToken, Subnet targetedNetwork, int threadNumber) {
            // Used to store hosts that are currently alive : TCP connection succeeded
            List<string> aliveHosts = new List<string>();
            // Used to store hosts that are currently marked as "UP" in Redis
            List<string> existingHosts = new List<string>();

            // Current thread prefix
            string threadPrefix = String.Concat(_discoveryName, threadNumber);
            
                // Used to store aliveHosts detected by check
                aliveHosts = Scan.TCPScan(targetedNetwork.getAllIPsInSubnet(), _targetedPort, threadPrefix);
                existingHosts =  _redis.GetSubnetHosts(threadPrefix);
                

                if(cancelToken.IsCancellationRequested) {
                    // If stopDiscovery() was called. That means that someone wants to stop the discovery, and thus, this thread. 
                    // Cleanup and exit the while loop.  
                    Console.WriteLine("DEBUG: Cancel method called on {0} thread. Cancellation requested.", threadPrefix); // Debug console output
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
                    if ( (! aliveHosts.Contains(existingHost)) && (_redis.Read(threadPrefix, existingHost) == "UP") ) {
                        Console.WriteLine("{0}: Deleting {1} which is not alive.", threadPrefix, existingHost); // Debug console output
                        // Mark the host as down in redis
                        _redis.markHostDown(threadPrefix, existingHost);
                        // Execute action _actionIfDown
                        _actionIfDown.Execute(existingHost, threadPrefix);
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
                    if ( (! _redis.doesKeyExist(threadPrefix, aliveHost)) || (_redis.Read(threadPrefix, aliveHost) == "DOWN")) {
                        Console.WriteLine("{0}: Host {1} was not in Redis. Adding it.", threadPrefix, aliveHost); // Debug console output
                        // Mark the host as UP in redis
                        _redis.markHostUp(threadPrefix, aliveHost);
                        // Execute the _actionIfUp action
                        _actionIfUp.Execute(aliveHost, threadPrefix);
                    }
                    else {
                        Console.WriteLine("{0}: Host {1} was already in Redis ! Value : {2}", threadPrefix, aliveHost, _redis.Read(threadPrefix, aliveHost)); // Debug console output
                    }
                }
            
            Console.WriteLine("DEBUG: Out of loop in {0} thread. Self-destructin.", threadPrefix); // Debug console output
            return;
        }
    }
}