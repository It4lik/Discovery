using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace discovery
{
    class Program
    {
        static void Main(string[] args)
        {                

            Regex CIDRRegex = new Regex(@"^(([1-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.)(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){2}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\/(([1-9])|([12][0-9])|(3[0-2]))$");
            Console.WriteLine(CIDRRegex.IsMatch("172.17.0.0/24"));
            // Subnet on which do the tests
            Subnet LAN = new Subnet("172.17.0.0/24");

            // Redis connection to use
            Stocker redis = new Stocker("127.0.0.1", 6379);
            // Action to do if a host is up
            HostAction actionIfUp = new HostAction(HostAction.ActionType.SSHExec, "127.0.0.1", 4444, "john", "", "echo \"<HOST> became UP at <TIME>\"  >> ZALU");
            // Action to do if a host is down
            HostAction actionIfDown = new HostAction(HostAction.ActionType.SSHExec, "127.0.0.1", 4444, "john", "", "echo \"<HOST> became DOWN at <TIME>\"  >> ZALU");
            // Object that issue the discovery
            Discover disco = new Discover("LAN", Discover.CheckType.tcp, LAN, 6379, actionIfUp, actionIfDown, redis);
            disco.StartDiscovery();

        }
    }
}
