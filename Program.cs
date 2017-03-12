using System;
using System.Net;
using System.Net.Sockets;
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
            
            /*Subnet yop = new Subnet("192.168.1.0/22");
            yop.iterateOnSubnet();*/
           
            string y = "192.168.1.94";

            TcpClient test = new TcpClient();
            test.ConnectAsync(y, 8888);
            test.Available()
            Console.WriteLine(test.Connected);
        }
    }
}
