// Used for TCP connections
using System.Net.Sockets;
// Used to issue shell commands
using System.Diagnostics;

namespace discovery
{
    public static class Check
    {
        public static bool TCPTestHost(string hostIPAddress, int hostTargetedPort) {
            // Test a host using a simple TCP connection (TcpClient.ConnectAsync method)

            // Create the TcpClient used to connect
            TcpClient connection = new TcpClient();

            bool isHostReachable;
            try
            {
                // The Wait method will return True if the connection was successful, OR throw an exception if the connection fails. 
                isHostReachable = connection.ConnectAsync(hostIPAddress, hostTargetedPort).Wait(10000);
                // Print successful message
                System.Console.WriteLine("Host " + hostIPAddress + " is REACHABLE on port " + hostTargetedPort + " using TCP check.");
                // Set return value to true
                isHostReachable = true;
            }
            catch (System.Exception)
            {
                // An exception is catched if the Wait method fails (eg the TCP connection has failed)

                // Print error message
                System.Console.WriteLine("ERROR : Host " + hostIPAddress + " is NOT reachable on port " + hostTargetedPort + " using TCP check.");
                // Set return value to false
                isHostReachable = false;
            }
            // Cleanup
            connection.Dispose();

            return isHostReachable;
        }

        public static bool CustomTest(string hostIPAddress, int hostTargetedPort, string shellCommand) {
            // Used to issue custom checks using a shell on server-side. 
            // The user who will run these commands is the same wha run this program.
            
            // Return value
            bool isHostReachable;

            // Create the ProcessStartInfo instance used to describe a process who run the check
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = shellCommand;
            psi.UseShellExecute = false;

            // Create the process used to actually issue the command
            Process proc = new Process();
            proc.StartInfo = psi;

            // Start the process (eg issue the check)
            try
            {
                proc.Start();
                isHostReachable = true;
            }
            catch (System.Exception)
            {
                System.Console.WriteLine("ERROR : commmand " + proc.StartInfo.FileName + " failed !");
                System.Console.WriteLine(proc.);
                isHostReachable = false;
            }
            // Cleanup
            proc.Dispose();
            return isHostReachable;
        }
    }
}