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
            

            Subnet yop = new Subnet("192.168.1.0/24");
            Scan.TCPscan(yop.getAllIPsInSubnet(), 8888);

            /*TcpClient yo = new TcpClient();
            Console.WriteLine( yo.ConnectAsync("192.168.1.94", 8888).Wait(10000));*/

                
            // Console.WriteLine(Check.TCPTestHost("192.168.1.94", 8888));
            //Check.CustomTest("192.168.1.94", 8788, "C:\\Windows\\System32\\bash.exe", "-c \"qls\"");
        }
    }
}
