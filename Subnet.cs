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
        /// Only subnet address. Ex : "192.168.1.0"
        private  string _networkIP;
        /// Only mask, CIDR format. Ex : 24
        private int _maskCIDR;
        /// Used for subnet shrinking
        private ShrinkedSubnetType _type;
        /// Only mask, binary format, NO DOTS, 32 chars. Ex : "11111111111111111100000000000000"
        private string _netmask;
        /// First free IP address in subnet, binary, WITH DOTS, ends with 1. Ex : "11010111.11010011.11011000.0000001".
        private string _firstFreeIP;
        /// Last free IP address in subnet, binary, WITH DOTS, ends with 0. Ex : "11010111.11010011.11011000.11111110".
        private string _lastFreeIP;
        /// Last IP address of subnet, binary, WITH DOTS, ends with 1. Ex : "11010111.11010011.11011000.11111111".
        private string _broadcastIP; 

        /// Create a new Subnet using its CIDR formatted address (like "192.168.1.0/24")
        public Subnet(string _CIDRAddress) {
            // Verify that address is correctly formatted
            if (this.verifyAddressCIDR(_CIDRAddress)) {
                _maskCIDR = System.Convert.ToInt32(_CIDRAddress.Split('/')[1]);
                _networkIP = this.DetermineNetworkIP(_maskCIDR, _CIDRAddress.Split('/')[0]);
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
                _networkIP = this.DetermineNetworkIP(_maskCIDR, _CIDRAddress.Split('/')[0]);
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

        private string DetermineNetworkIP(int netmask, string ipAddress) {
        // this will return the first IP of a network eg the network address in decimal format

            string binIpAddress = DecimalIPtoBinIP(ipAddress);
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
            string networkIP = this.DetermineNetworkIP(maskCIDR, CIDRSubnet.Split('/')[0]);
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
        /*public List<Subnet> shrinkSubnetInSpecifiedSize(Subnet CIDRNetworkToShrink, int maskCIDR) {
            // The specified mask must be inferior to the original mask
            if ()
            
            // Return value
            List<Subnet> shrunkSubnets = new List<Subnet>();
            
            // This is the "fix part" of the subnet address : every bits in the initial mask. This will never change. 

            return shrunkSubnets;
        }*/
        private bool verifyAddressCIDR(string CIDRAddress) {
            // Used to verify that a string is a subnet IPv4 address formatted in CIDR (as in "192.168.1.0/24)
            Regex CIDRRegex = new Regex(@"^(([1-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.)(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){2}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\/(([1-9])|([12][0-9])|(3[0-2]))$");
            return CIDRRegex.IsMatch(CIDRAddress);
        }
    }
}
