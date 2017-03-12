using System;
using System.Text.RegularExpressions;

namespace discovery
{
    public class Subnet : IPv4Address
    {
        // Class used to make IPv4 operations
        // This is gonna be exploded into an interface and a "Subnet" class that we can instantiate with one subnet (CIDR format) 
        private enum UsefulIPs {broadcast, firstFree, lastFree, network};
        private string _CIDRAddress; // Subnet address, CIDR format. Ex : "192.168.1.13/24"
        private  string _inputIP; // Input IP (splitted CIDR with /). Ex : "192.168.1.13"
        private  string _networkIP; // Only subnet address. Ex : "192.168.1.0"
        private int _maskCIDR; // Only mask, CIDR format. Ex : 24
        private string _netmask; // Only mask, binary format, NO DOTS, 32 chars. Ex : "11111111111111111100000000000000"
        private string _firstFreeIP; // First free IP address in subnet, binary, WITH DOTS, ends with 1. Ex : "11010111.11010011.11011000.0000001". 
        private string _lastFreeIP; // Last free IP address in subnet, binary, WITH DOTS, ends with 0. Ex : "11010111.11010011.11011000.11111110".
        private string _broadcastIP; // Last IP address of subnet, binary, WITH DOTS, ends with 1. Ex : "11010111.11010011.11011000.11111111".

        
        public Subnet(string addr) {
            _CIDRAddress = addr;
            if (this.verifyAddressCIDR(_CIDRAddress)) {
                _maskCIDR = System.Convert.ToInt32(_CIDRAddress.Split('/')[1]);
                _networkIP = this.getNetworkIP(_maskCIDR, _CIDRAddress);
                this.getNetmask(_inputIP, _maskCIDR);
                setFirstAndLastIP(_inputIP, _maskCIDR);
            }
        }

        private string getNetmask(string netIP, int maskCIDR) {
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

        private string getNetworkIP(int netmask, string ipAddress) {
            string binIpAdress = decimalIPtoBinIP(ipAddress);
            string networkIpAddress = string.Empty;
            for (int i = 0; i < netmask; i++)
            {
                networkIpAddress = String.Concat(networkIpAddress, ipAddress[i]);
            }
            for (int i = netmask; i < 32; i++)
            {
                networkIpAddress = String.Concat(networkIpAddress, '0');
            }
            return networkIpAddress;
        }

        private void setFirstAndLastIP(string networkIP, int maskCIDR) {
            // get first free address, last free address and the broadcast address
            string networkBinIP = decimalIPtoBinIP(networkIP);
            char[] tempFirstIPChar = networkBinIP.ToCharArray(0, 32);
            char[] tempLastIPChar = networkBinIP.ToCharArray(0, 32);

            for (int i = maskCIDR; i < 31; i++) // 31 because we don't need to set the last bit : it will always be 1 for the first address (with, it's the subnet address)
            {
                tempFirstIPChar[i] = '0';
                tempLastIPChar[i] = '1';
            }
            // Get first free IP : firt IP after subnet's IP
            tempFirstIPChar[31] = '1';
            _firstFreeIP = binIPtoBinIPWithDots(new string(tempFirstIPChar));
            // Get last free IP : last free IP before broadcast address
            tempLastIPChar[31] = '0';
            _lastFreeIP = binIPtoBinIPWithDots(new string(tempLastIPChar));
            // et broadcast address : last address of subnet
            tempLastIPChar[31] = '1';
            _broadcastIP = binIPtoBinIPWithDots(new string(tempLastIPChar));
        }
        private void setUsefulIP(string networkIP, int maskCIDR, UsefulIPs wantedIPType) {
            // Used to get firstFree, lastFree or broadcast address from subnet address
            string networkBinIP = decimalIPtoBinIP(networkIP);
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
                   _broadcastIP = binIPtoBinIPWithDots(new string(tempLastIPChar));
                   break;
                case UsefulIPs.firstFree:
                    // Get first free IP : firt IP after subnet's IP
                    tempFirstIPChar[31] = '1';
                    _firstFreeIP = binIPtoBinIPWithDots(new string(tempFirstIPChar));
                   break;
                case UsefulIPs.lastFree:
                    // Get last free IP : last free IP before broadcast address
                    tempLastIPChar[31] = '0';
                    _lastFreeIP = binIPtoBinIPWithDots(new string(tempLastIPChar));
                   break;
                default:
                    break;
            }
        }
        public void iterateOnSubnet() {
            string tempBinIP = _firstFreeIP;
            string endBinIP = _broadcastIP;
            while (tempBinIP != endBinIP) {
                Console.WriteLine(binIPtoDecimalIP(tempBinIP));
                tempBinIP = incrementIP(tempBinIP);
            }
        }
        private bool verifyAddressCIDR(string CIDRAddress) {
            // Used to verify that a string is a subnet IPv4 address formatted in CIDR (as in "192.168.1.0/24)
            Regex CIDRRegex = new Regex(@"^(([1-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\/(([1-9])|([12][0-9])|(3[0-2]))$");
            return CIDRRegex.IsMatch(CIDRAddress);
        }
    }
}