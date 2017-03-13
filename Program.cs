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
            /*Console.WriteLine("Starting...");
            Stocker test = new Stocker("172.17.0.2", 6379);
            Console.WriteLine(test.GetHost());

            test.Write("yap", "666");
            Console.WriteLine(test.Read("yap"));*/

            // Console.WriteLine(Check.TCPTestHost("192.168.1.94", 8888));
            //Check.CustomTest("192.168.1.94", 8788, "C:\\Windows\\System32\\bash.exe", "-c \"qls\"");

            /*Subnet yop = new Subnet("192.168.1.0/24");
            Scan.TCPScan(yop.getAllIPsInSubnet(), 9999);*/
            
            
            /*Subnet LAN = new Subnet("192.168.1.0/24");
            Stocker redis = new Stocker("127.0.0.1", 6379);
            Discover disco = new Discover(Discover.CheckType.tcp, LAN, 6379, redis, "LAN");
            disco.startDiscovery();*/

        }
    }
}
