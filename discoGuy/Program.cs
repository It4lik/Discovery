﻿using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using StackExchange.Redis;
using System.Collections.Generic;
using System.IO;

namespace discovery
{
    class Program
    {
        static void Main(string[] args)
        {   
            CheckConf test = new CheckConf("hop");
            /*
            // Subnet on which do the tests
            Subnet LAN = new Subnet("192.168.1.0/22");
            // Redis connection to use
            Stocker redis = new Stocker("192.168.1.94", 6379);
            // Action to do if a host is up
            HostAction actionIfUp = new HostAction(HostAction.ActionType.SSHExec, "192.168.1.94", 2222, "john", "", "echo \"<HOST> became UP at <TIME>\"  >> ZALU");
            // Action to do if a host is down
            HostAction actionIfDown = new HostAction(HostAction.ActionType.SSHExec, "192.168.1.94", 2222, "john", "", "echo \"<HOST> became DOWN at <TIME>\"  >> ZALU");
            // Object that issue the discovery
            Discover disco = new Discover(Discover.CheckType.tcp, LAN, 443, actionIfUp, actionIfDown, redis, "LAN", 6);
            disco.startDiscovery();
            */

        }
    }
}
