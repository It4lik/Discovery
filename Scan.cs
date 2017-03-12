using System;
using System.Collections.Generic;

// Used for TCP connections
using System.Net.Sockets;
// Used to issue shell commands
using System.Diagnostics;


namespace discovery
{
    public static class Scan
    {
        public static bool TCPTestHost(string hostIPAddress, int hostTargetedPort) {
            // Test a host using a simple TCP connection (TcpClient.ConnectAsync method)

            // Create the TcpClient used to connect
            TcpClient connection = new TcpClient();

            bool isHostReachable;
            try
            {
                // The Wait method will return True if the connection was successful, OR throw an exception if the connection fails. 
                isHostReachable = connection.ConnectAsync(hostIPAddress, hostTargetedPort).Wait(1000);
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

        public static bool CustomTest(string hostIPAddress, int hostTargetedPort, string shellCommand, string args) {
            // Used to issue custom checks using a shell on server-side. 
            // The user who will run these commands is the same wha run this program.
            
            // Replace host and port with their proper values
            shellCommand.Replace("%HOST%", hostIPAddress);
            shellCommand.Replace("%PORT%", hostTargetedPort.ToString());

            // Print warning if OS is not Linux : custom checks are basically used to issue bash or sh commands
            if ( ! System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux)) {
                Console.WriteLine("WARNING : Custom checks are only supported on Linux. It's experimental on other platforms.");
            }

            // Return value
            bool isHostReachable;

            // Create the ProcessStartInfo instance used to describe a process who run the check
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = shellCommand;
            psi.Arguments = args;
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;

            // Create the process used to actually issue the command
            Process proc = new Process();
            proc.StartInfo = psi;
            // Start the process (eg issue the check)
            try
            {
                proc.Start();
                // If the process success, the check has succeed. Print succeed message and set return value to true. 
                Console.WriteLine("Process executed successfully.");
                isHostReachable = true;
            }
            catch (Exception ex)
            {
                // If the command fails, prints original error and set return value to false
                Console.WriteLine("ERROR : commmand " + proc.StartInfo.FileName + " failed !");
                Console.WriteLine(ex.Message);
                isHostReachable = false;
            }
            // Cleanup
            proc.Dispose();
            return isHostReachable;
        }
      
        public static List<string> TCPscan(List<string> targetedIPs, int targetedPort) {
            // Total number of scanned hosts (253 for a /24 subnet)
            int totalScannedHosts = 0; 
            // Total number of alive host (eg TCP check succeeded)
            int aliveHostsNumber = 0;
            // List that will contain the IP address of all successful hosts
            List<string> aliveHosts = new List<string>();

            // Iterate on an IP address list returned from iterateOnSubnet method
            foreach (string currentIp in targetedIPs) {
                // If a host is up at currentIp and has targetedPort open for TCP
                if (Scan.TCPTestHost(currentIp, targetedPort)) {
                    aliveHostsNumber ++;
                    // Add current host ro returned list
                    aliveHosts.Add(currentIp);        
                }
                totalScannedHosts ++;
            }

            // Debug console output
            Console.WriteLine("Number of hosts scanned : " + totalScannedHosts + ". Alive hosts : " + aliveHostsNumber + ".");

            return aliveHosts;
        }
    }
}