using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace discovery
{
    public class Subnet : IPv4Address
    {
        // Class used to make IPv4 operations
        // This is gonna be exploded into an interface and a "Subnet" class that we can instantiate with one subnet (CIDR format)
        private enum UsefulIPs {broadcast, firstFree, lastFree, network};
        /// This is used for subnet shrinking. A "first" network doesn't have a broadcast address. A "last" network doesn't have broadcast address. A "none" have NONE of these two addresses (eg getAllIPsInSubnet() will return ALL IPS). When shrinking a /22 in four /24, there are still only one network address and one broadcast address from the /22 perspective. A "normal" get both subnet and broadcast address 
        public enum ShrinkedSubnetType {first, none, last, normal}; 
        /// CIDR address of network (used as argument in constructor) like "192.168.1.0/24")
        public string _networkIP { get; set; }
        /// Only mask, CIDR format. Ex : 24
        public int _maskCIDR { get; set; }
        /// Used for subnet shrinking
        public ShrinkedSubnetType _type { get; set; }
        /// Only mask, binary format, NO DOTS, 32 chars. Ex : "11111111111111111100000000000000"
        private string _netmask;
        /// First free IP address in subnet, binary, WITH DOTS, ends with 1. Ex : "11010111.11010011.11011000.0000001".
        private string _firstFreeIP;
        /// Last free IP address in subnet, binary, WITH DOTS, ends with 0. Ex : "11010111.11010011.11011000.11111110".
        private string _lastFreeIP;
        /// Last IP address of subnet, binary, WITH DOTS, ends with 1. Ex : "11010111.11010011.11011000.11111111".
        private string _broadcastIP; 

        /// Create a new Subnet using its CIDR formatted address (like "192.168.1.0/24")
        public Subnet(string CIDRAddress) {
            // Verify that address is correctly formatted
            if (this.verifyAddressCIDR(CIDRAddress)) {
                _maskCIDR = System.Convert.ToInt32(CIDRAddress.Split('/')[1]);
                _networkIP = this.DetermineNetworkIP(CIDRAddress.Split('/')[0], _maskCIDR);
                _netmask = this.DetermineNetmask(_networkIP, _maskCIDR);
                _type = this._type = ShrinkedSubnetType.normal;
                // Set _firstFreeIP, _lastFreeIP and _broadcastIP properties
                this.setAllUsefulIPs(_networkIP, _maskCIDR);
            }
            else {
                Console.WriteLine("FATAL: This subnet is not a CIDR-formatted IPv4 subnet. Like '192.168.1.0/24'. Exiting.");
                System.Environment.Exit(5);
            }
        }
        /// Overload used to specify a ShrinkedSubnetType (only used internally)
        public Subnet(string _CIDRAddress, ShrinkedSubnetType type) {
            // Verify that address is correctly formatted
            if (this.verifyAddressCIDR(_CIDRAddress)) {
                _maskCIDR = System.Convert.ToInt32(_CIDRAddress.Split('/')[1]);
                _networkIP = this.DetermineNetworkIP(_CIDRAddress.Split('/')[0], _maskCIDR);
                _netmask = this.DetermineNetmask(_networkIP, _maskCIDR);
                _type = this._type = type;
                // Set _firstFreeIP, _lastFreeIP and _broadcastIP properties
                this.setAllUsefulIPs(_networkIP, _maskCIDR);
            }
            else {
                Console.WriteLine("FATAL: This subnet is not a CIDR-formatted IPv4 subnet. Like '192.168.1.0/24'. Exiting.");
                System.Environment.Exit(5);
            }
        }

        private string DetermineNetmask(string netIP, int maskCIDR) {
            // Get netmask : iterate on subnet IPv4 address ||| from 0 to maskCIDR and fill with 1 ||| from maskCIDR to 32 (bits number in an IPv4 address) and fill with 0
            string netmask = string.Empty;
            for (int i = 0; i < maskCIDR; i++)
            {
                netmask = System.String.Concat(netmask, "1");
            }
            for (int i = maskCIDR; i < 32; i++)
            {
                netmask = System.String.Concat(netmask, "0");
            }
            return netmask;
        }
        /// This will return the first IP of a network, eg the network address in decimal format. Netmask is an int ('24' in "192.168.1.10/24")
        private string DetermineNetworkIP(string IPAddress, int netmask) {
            string binIpAddress = DecimalIPtoBinIP(IPAddress);
            string networkBinIpAddress = string.Empty;
            for (int i = 0; i < netmask; i++)
            {
                networkBinIpAddress = String.Concat(networkBinIpAddress, binIpAddress[i]);
            }
            for (int i = netmask; i < 32; i++)
            {
                networkBinIpAddress = String.Concat(networkBinIpAddress, '0');
            }
            return BinIPtoDecimalIP(BinIPtoBinIPWithDots(networkBinIpAddress));
        }
        private void setAllUsefulIPs(string networkIP, int maskCIDR) {
            // get first free address, last free address and the broadcast address
            string networkBinIP = DecimalIPtoBinIP(networkIP);
            char[] tempFirstIPChar = networkBinIP.ToCharArray(0, 32);
            char[] tempLastIPChar = networkBinIP.ToCharArray(0, 32);

            for (int i = maskCIDR; i < 31; i++) // 31 because we don't need to set the last bit : it will always be 1 for the first address (with, it's the subnet address)
            {
                tempFirstIPChar[i] = '0';
                tempLastIPChar[i] = '1';
            }
            // Get first free IP : firt IP after subnet's IP
            tempFirstIPChar[31] = '1';
            _firstFreeIP = BinIPtoBinIPWithDots(new string(tempFirstIPChar));
            // Get last free IP : last free IP before broadcast address
            tempLastIPChar[31] = '0';
            _lastFreeIP = BinIPtoBinIPWithDots(new string(tempLastIPChar));
            // et broadcast address : last address of subnet
            tempLastIPChar[31] = '1';
            _broadcastIP = BinIPtoBinIPWithDots(new string(tempLastIPChar));
        }
        
        /// Get broadcast address formatted in decimal like 192.168.1.0 (from a decimal networkIP and a CIDR mask (like '24' in "192.168.1.0/24")). 
        private string DetermineBroadcastAddress(string networkIP, int maskCIDR) {
            string networkBinIP = DecimalIPtoBinIP(networkIP);
            char[] tempLastIPChar = networkBinIP.ToCharArray(0, 32);

            for (int i = maskCIDR; i < 31; i++) // 31 because we don't need to set the last bit : it will always be 1 for the first address (with, it's the subnet address)
            {
                tempLastIPChar[i] = '1';
            }
            // broadcast address : last address of subnet
            tempLastIPChar[31] = '1';
            return BinIPtoDecimalIP(BinIPtoBinIPWithDots(new string(tempLastIPChar)));
        }
        private void setUsefulIP(string networkIP, int maskCIDR, UsefulIPs wantedIPType) {
            // Used to get firstFree, lastFree or broadcast address from subnet address
            string networkBinIP = DecimalIPtoBinIP(networkIP);
            char[] tempFirstIPChar = networkBinIP.ToCharArray(0, 32);
            char[] tempLastIPChar = networkBinIP.ToCharArray(0, 32);

            for (int i = maskCIDR; i < 31; i++) // 31 because we don't need to set the last bit : it will always be 1 for the first address (with, it's the subnet address)
            {
                tempFirstIPChar[i] = '0';
                tempLastIPChar[i] = '1';
            }

            switch (wantedIPType)
            {
                case UsefulIPs.broadcast:
                    // broadcast address : last address of subnet
                   tempLastIPChar[31] = '1';
                   _broadcastIP = BinIPtoBinIPWithDots(new string(tempLastIPChar));
                   break;
                case UsefulIPs.firstFree:
                    // Get first free IP : firt IP after subnet's IP
                    tempFirstIPChar[31] = '1';
                    _firstFreeIP = BinIPtoBinIPWithDots(new string(tempFirstIPChar));
                   break;
                case UsefulIPs.lastFree:
                    // Get last free IP : last free IP before broadcast address
                    tempLastIPChar[31] = '0';
                    _lastFreeIP = BinIPtoBinIPWithDots(new string(tempLastIPChar));
                   break;
                default:
                    break;
            }
        }

        /// This will return a string list of all IPs in a network (including network's one and broadcast's)
        public List<string> getAllFreeIPsFromSubnet(string CIDRSubnet) {
        
            // Verify that subnet is correctly formatted
            if ( ! verifyAddressCIDR(CIDRSubnet)) {                
                Console.WriteLine("FATAL: This subnet is not a CIDR-formatted IPv4 subnet. Like '192.168.1.0/24'. Exiting.");
                System.Environment.Exit(5); 
            }

            // Get needed addresses
            int maskCIDR = System.Convert.ToInt32(CIDRSubnet.Split('/')[1]);
            string networkIP = this.DetermineNetworkIP(CIDRSubnet.Split('/')[0], maskCIDR);
            string broadcastIP = this.DetermineBroadcastAddress(networkIP, maskCIDR);

            // Temp variables used to iterate from the first to the last IP
            string currentBinIP = DecimalIPtoBinIP(networkIP); string endBinIP = DecimalIPtoBinIP(broadcastIP);

            // Return value. Will contain all IPs in the subnet
            List<string> IPs = new List<string>();

            // Iterate on all IPs = start from the first, increment, till the currentBinIP does not treach tha last IP, eg the endBinIP (often the broadcast address of a subnet)
            while (currentBinIP != endBinIP) {
                // Add current IP to the IPs list
                IPs.Add(BinIPtoDecimalIP(currentBinIP));

                // Console.WriteLine(binIPtoDecimalIP(currentBinIP)); // Debug console output
                // Increment current IP
                currentBinIP = BinIPtoBinIPWithoutDots(IncrementIP(BinIPtoBinIPWithDots(currentBinIP)));
            }
            // add the last one (broadcast address)
            IPs.Add(BinIPtoDecimalIP(endBinIP));
            return IPs;
        }
        public List<string> getAllIPsInSubnet() {
            string currentBinIP;
            string endBinIP = _broadcastIP;

            switch (_type)
            {
                // This is used to include the very first IP of the subnet in the returned list (used in subnet shrinking)
                case ShrinkedSubnetType.none:
                case ShrinkedSubnetType.last:
                    currentBinIP = BinIPtoBinIPWithDots(DecimalIPtoBinIP(_networkIP)); 
                    break;
                // this is used to exclude the very first IP of the subnet because it's the network address 
                case ShrinkedSubnetType.first:
                case ShrinkedSubnetType.normal:
                default:
                    currentBinIP = _firstFreeIP;
                    break;
            }
            // Return value. Will contain all IPs in the subnet
            List<string> IPs = new List<string>();

            // Iterate on all IPs = start from the first, increment, till the currentBinIP does not treach tha last IP, eg the endBinIP (often the broadcast address of a subnet)
            while (currentBinIP != endBinIP) {
                // Add current IP to the IPs list
                IPs.Add(BinIPtoDecimalIP(currentBinIP));

                // Console.WriteLine(binIPtoDecimalIP(currentBinIP)); // Debug console output

                // Increment current IP
                currentBinIP = IncrementIP(currentBinIP);
            }

            // If the subnet is a "first" or "none" subnet it doesn't have a broadcast address (used in subnet shrinking)
            if (_type == ShrinkedSubnetType.first || _type == ShrinkedSubnetType.none) 
                IPs.Add(BinIPtoDecimalIP(endBinIP));

            return IPs;
        }

        /// This method is used to shred networkToShrink in multiple subnets with a maskCIDR mask. (ie This will shrink 192.168.0.0/23 in 192.168.0.0/24 and 192.168.1.0/24)
        public List<Subnet> Shrink (int newMaskCIDR) {
            // The specified mask must be inferior to the original mask
            if (this._maskCIDR >= newMaskCIDR) {
                Console.WriteLine("FATAL: You can't shrink in a larger subnet or same size. Exiting.");
                System.Environment.Exit(7);
            }

            /// Return value
            List<Subnet> shrunkSubnets = new List<Subnet>();
            
            // IP address of initial network, binary formatted, without dots. Like "01011101010001100100100001000100"
            string initialNetworkBinIP = this.DecimalIPtoBinIP(_networkIP);

            // The "fix part" of the subnet address : every bits in the initial mask. These will never change. 
            string fixPart = initialNetworkBinIP.Substring(0, this._maskCIDR);
            // The "net part" are the bits who are going to be part of the new network adresses for each resulting shrunk subnet
            string netPart = initialNetworkBinIP.Substring(this._maskCIDR, (newMaskCIDR - this._maskCIDR));
            // The "ip range part" are the bits that are going to be out of the new masks
            string ipRangePart = initialNetworkBinIP.Substring(newMaskCIDR);
            
            /*
            // We are going to iterate on all shrinked subnets to store them in a list, more specifically, we store their CIDR formatted addresses (like "192.168.1.0/24")
            // To iterate on these network addresses, we just need to : 
            //   - keep the fix part (eg keep initialNetworkBinIP bits)
            //   - increment the net part
            //   - set each bit of IP range part to 0 (eg keep initialNetworkBinIP bits)
            //   - concatenate '/' and newMaskCIDR to the resulting address
            // We also need to calculate the last network address as an exit condition for the loop : 
            //   - keep the fix part (eg keep initialNetworkBinIP bits)
            //   - set each bit of network part to 1
            //   - set each but of IP range part to 0 (eg keep initialNetworkBinIP bits)
            */   

            char[] lastNetworkAddressArray = initialNetworkBinIP.ToCharArray();
            for (int i = this._maskCIDR; i < newMaskCIDR; i++)
            {
                lastNetworkAddressArray[i] = '1';
            }
            // Store the last address as a string
            string lastNetworkAddress = new string(lastNetworkAddressArray);

            // Incremented IP address all along the loop
            string currentBinIP = initialNetworkBinIP;


            // Add the first Subnet to the returned list using currentBinIP but correctly formatted : "192.168.1.0/26" (with new mask) to initialize the Subnet object
            shrunkSubnets.Add(new Subnet(String.Concat((BinIPtoDecimalIP(BinIPtoBinIPWithDots(currentBinIP))), '/', newMaskCIDR), ShrinkedSubnetType.first));
            // Increment netPart
            netPart = BinaryTools.incrementBin(netPart).Substring(BinaryTools.incrementBin(netPart).Length - netPart.Length, netPart.Length); // Substring is used because increment bin always return an 8 bit string (or any other 8 multiples)
            // Create the new currentBinIP by concatenating fixPart netPart and ipRangePart
            currentBinIP = String.Concat(fixPart, netPart, ipRangePart);

            do
            {
                // Add a new none Subnet to the returned list using currentBinIP but correctly formatted : "192.168.1.0/26" (with new mask) to initialize the Subnet object
                shrunkSubnets.Add(new Subnet(String.Concat((BinIPtoDecimalIP(BinIPtoBinIPWithDots(currentBinIP))), '/', newMaskCIDR), ShrinkedSubnetType.none));
                // Increment netPart
                netPart = BinaryTools.incrementBin(netPart).Substring(BinaryTools.incrementBin(netPart).Length - netPart.Length, netPart.Length); // Substring is used because increment bin always return an 8 bit string (or any other 8 multiples)
                // Create the new currentBinIP by concatenating fixPart netPart and ipRangePart
                currentBinIP = String.Concat(fixPart, netPart, ipRangePart);
            } while (currentBinIP != lastNetworkAddress);

            // Add the last Subnet to the returned list using currentBinIP but correctly formatted : "192.168.1.0/26" (with new mask) to initialize the Subnet object
            shrunkSubnets.Add(new Subnet(String.Concat((BinIPtoDecimalIP(BinIPtoBinIPWithDots(lastNetworkAddress))), '/', newMaskCIDR), ShrinkedSubnetType.last));

            return shrunkSubnets;
        }

        /// Determine if an IP is in current network
        public bool isInSubnet(string IPAddress) {
            // Determine network IP of both IPs
            string IPNetworkIP = DetermineNetworkIP(IPAddress, _maskCIDR);
            string networkNetworkIP = DetermineNetworkIP(_networkIP, _maskCIDR);
            // If the network addresses are the same, IPAddress does belong to networkIPCIDRAddress
            if (IPNetworkIP == networkNetworkIP)
                return true;
            else
                return false;
        }
        public static bool verifyAddressCIDR(string CIDRAddress) {
            // Used to verify that a string is a subnet IPv4 address formatted in CIDR (as in "192.168.1.0/24)
            Regex CIDRRegex = new Regex(@"^(([1-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.)(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){2}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\/(([1-9])|([12][0-9])|(3[0-2]))$");
            return CIDRRegex.IsMatch(CIDRAddress);
        }
    }
}
