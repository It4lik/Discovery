using System;
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
            Subnet yop = new Subnet("192.168.1.0/22");
            yop.iterateOnSubnet();
        }
    }
}
