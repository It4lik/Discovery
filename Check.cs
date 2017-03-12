using System.Net.Sockets;

namespace discovery
{
    public class Check
    {
        public enum CheckType { custom, tcp };
        private CheckType _type;
        private string _customCheck;
        private TcpClient _tcp;

        public Check(CheckType type) {
            _type = type;
            _tcp = new TcpClient();
        }
        public Check(CheckType type, string command) {
            _type = type;
            _customCheck = command;
        }

        public bool TCPTestHost(string hostIPAddress, int hostTargetedPort) {
            bool isHostReachable;
            try
            {
                isHostReachable = _tcp.ConnectAsync(hostIPAddress, hostTargetedPort).Wait(10000);
                System.Console.WriteLine("Host " + hostIPAddress + " is REACHABLE on port " + hostTargetedPort + " using TCP check.");
                isHostReachable = true;
            }
            catch (System.Exception)
            {
                System.Console.WriteLine("ERROR : Host " + hostIPAddress + " is NOT reachable on port " + hostTargetedPort + " using TCP check.");
                isHostReachable = false;
            }
            return isHostReachable;
        }
    }
}