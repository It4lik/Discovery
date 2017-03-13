﻿using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace discovery
{
    class Program
    {
        static void Main(string[] args)
        {            
            /*Console.WriteLine("Starting...");
            Stocker test = new Stocker("172.17.0.2", 6379);
            Console.WriteLine(test.GetHost());

            test.Write("yap", "666");
            Console.WriteLine(test.Read("yap"));*/

            // Console.WriteLine(Check.TCPTestHost("192.168.1.94", 8888));
            //Check.CustomTest("192.168.1.94", 8788, "C:\\Windows\\System32\\bash.exe", "-c \"qls\"");

            /*Subnet yop = new Subnet("192.168.1.0/24");
            Scan.TCPScan(yop.getAllIPsInSubnet(), 9999);*/

            Subnet yop = new Subnet("192.168.1.0/24");
            Discover test = new Discover(Discover.CheckType.tcp, yop, "ZOB");
            test.startDiscovery();
            System.Threading.Thread.Sleep(50);
            test.stopDiscovery();
            System.Threading.Thread.Sleep(5000);
        }
    }
}
