using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using StackExchange.Redis;
using System.Collections.Generic;

namespace discovery
{
    class Program
    {
        static void Main(string[] args)
        {   
                
            // Subnet on which do the tests
            Subnet LAN = new Subnet("177.177.1.0/20");
            // Redis connection to use
            Stocker redis = new Stocker("177.177.1.10", 6379);
            // Action to do if a host is up
            HostAction actionIfUp = new HostAction(HostAction.ActionType.SSHExec, "177.177.1.21", 22, "john", "", "echo \"<HOST> became UP at <TIME>\"  >> ZALU");
            // Action to do if a host is down
            HostAction actionIfDown = new HostAction(HostAction.ActionType.SSHExec, "177.177.1.21", 22, "john", "", "echo \"<HOST> became DOWN at <TIME>\"  >> ZALU");
            // Object that issue the discovery
            Discover disco = new Discover(Discover.CheckType.tcp, LAN, 6379, actionIfUp, actionIfDown, redis, "CTNET", 6);
            disco.startDiscovery();

        }
    }
}
