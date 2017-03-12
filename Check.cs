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

        public bool TCPTestHost(string hostIPAddress, int hostTargetedPort, TcpClient tcp) {
            //tcp.ConnectAsync
            
            return false;
        }
    }
}