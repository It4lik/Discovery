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
            /*             
            // Subnet on which do the tests
            Subnet LAN = new Subnet("192.168.1.0/24");
            // Redis connection to use
            Stocker redis = new Stocker("127.0.0.1", 6379);
            // Action to do if a host is up
            HostAction actionIfUp = new HostAction(HostAction.ActionType.SSHExec, "192.168.1.94", 2222, "john", "dbc", "echo \"<HOST> became UP at <TIME>\"  >> ZALU");
            // Action to do if a host is down
            HostAction actionIfDown = new HostAction(HostAction.ActionType.SSHExec, "192.168.1.94", 2222, "john", "dbc", "echo \"<HOST> became DOWN at <TIME>\"  >> ZALU");
            // Object that issue the discovery
            Discover disco = new Discover(Discover.CheckType.tcp, LAN, 6379, actionIfUp, actionIfDown, redis, "LAN");
            disco.startDiscovery();
            */

            Subnet yo = new Subnet("192.168.1.10/24");

            foreach (string curip in yo.getAllFreeIPsFromSubnet("172.1.1.0/24"))
            {
                Console.WriteLine(curip);
            }
        }
    }
}
