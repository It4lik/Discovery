using System.Net;
using System.Text.RegularExpressions;
using System;
namespace discovery
{
    public class Subnet
    {
        // Class used to make IPv4 operations
        // This is gonna be exploded into an interface and a "Subnet" class that we can instantiate with one subnet (CIDR format) 
        private string _CIDRAddress; // Subnet address, CIDR format. Ex : "192.168.1.0/24"
        private  string _networkIP; // Only subnet address. Ex : "192.168.1.0"
        private int _maskCIDR; // Only mask, CIDR format. Ex : 24
        private string _netmask; // Only mask, binary format, NO DOTS, 32 chars. Ex : "11111111111111111100000000000000"
        private string _firstFreeIP; // First free IP address in subnet, binary, WITH DOTS, ends with 1. Ex : "11010111.11010011.11011000.0000001". 
        private string _lastFreeIP; // Last free IP address in subnet, binary, WITH DOTS, ends with 0. Ex : "11010111.11010011.11011000.11111110".
        private string _broadcastIP; // Last IP address of subnet, binary, WITH DOTS, ends with 1. Ex : "11010111.11010011.11011000.11111111".

        
        public Subnet(string addr) {
            _CIDRAddress = addr;
            if (verifyAddressCIDR(_CIDRAddress)) {
                _networkIP = _CIDRAddress.Split("/")[0];
                _maskCIDR = System.Convert.ToInt32(_CIDRAddress.Split("/")[1]);
                getNetmask(_networkIP, _maskCIDR);
                getFirstAndLastIP(_networkIP, _maskCIDR);
            }
        }
        public bool verifyAddressCIDR(string CIDRAddress) {
            // Used to verify that a string is a subnet IPv4 address formatted in CIDR (as in "192.168.1.0/24)
            Regex CIDRRegex = new Regex(@"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\/(([1-9])|([12][0-9])|(3[0-2]))$");
            return CIDRRegex.IsMatch(CIDRAddress);
        }

        private void getNetmask(string netIP, int maskCIDR) {
            // Get netmask : iterate on subnet IPv4 address ||| from 0 to maskCIDR and fill with 1 ||| from maskCIDR to 32 (bits number in an IPv4 address) and fill with 0
            for (int i = 0; i < maskCIDR; i++)
            {
                _netmask = System.String.Concat(_netmask, "1");
            }
            for (int i = maskCIDR; i < 32; i++)
            {
                _netmask = System.String.Concat(_netmask, "0");
            }
        }

        private string decimalToBin(string stringNumberToConvert) {
            // convert a decimal int to a binary int (only strings)
            string returnedString; 
            returnedString = Convert.ToString(Convert.ToInt32(stringNumberToConvert), 2);
            while (returnedString.Length < 8) {
                returnedString = String.Concat('0', returnedString);
            }
            return returnedString;
        }

        private string decimalToBin(int stringNumberToConvert) {
            // overload : pass an int instead of a string as an argument      
            string currentInt = stringNumberToConvert.ToString();
            return decimalToBin(currentInt);
        }

        private string binToDecimal(string stringNumberToConvert) {
            // convert a binary int to a decimal int (only strings)
            return Convert.ToInt32(stringNumberToConvert, 2).ToString();
        }

        private string decimalIPtoBinIP(string ipToConvert) {
            // convert a "192.168.1.0" address to his binary equivalet WITHOUT the dots (eg. a 32 characters long string (binary))
            string tempIP = string.Empty; string toConcat = string.Empty;
            for (int i = 0; i < ipToConvert.Split(".").Length; i++)
            {
                toConcat = decimalToBin(ipToConvert.Split(".")[i]);
                tempIP = String.Concat(tempIP, toConcat); 
            }
            return tempIP;
        }


        private string binIPtoBinIPWithDots(string binIPWithoutDots) {
            return String.Concat(binIPWithoutDots.Substring(0, 8), '.', binIPWithoutDots.Substring(8, 8), '.', binIPWithoutDots.Substring(16, 8), '.', binIPWithoutDots.Substring(24, 8));
        }

        private string binIPtoBinIPWithoutDots(string binIPWithDots) {
            return Regex.Replace(binIPWithDots, @"\.", "");
        }

        private string binIPtoDecimalIP(string stringNumberToConvert) {
            // argument is ip address, as a string, binary formatted, with dots
            // convert a 32-character long string (binary IP address without dots) to a "192.168.1.0" format (decimal)
            // takes four 8-character block, convert them to decimal and concatenate them (using a dot as separator)
            stringNumberToConvert = binIPtoBinIPWithoutDots(stringNumberToConvert);
            return String.Concat(binToDecimal(stringNumberToConvert.Substring(0, 8)), '.', binToDecimal(stringNumberToConvert.Substring(8, 8)), '.', binToDecimal(stringNumberToConvert.Substring(16, 8)), '.', binToDecimal(stringNumberToConvert.Substring(24, 8)));
        }

        private void getFirstAndLastIP(string networkIP, int maskCIDR) {
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

        private string[] IPtoStringArray(string iPtoConvert) {
            // convert an IP address string to a string array using dots as separators
            return iPtoConvert.Split('.');
        }

        private string stringArrayToIP(string [] stringArray) {
            // takes a string array and returns the concatenations of all the array and dots (array with values 192,168,0,1 as paramater returns "192.168.0.1")
            return String.Concat(stringArray[0], ".", stringArray[1], ".", stringArray[2], ".", stringArray[3]);
        }

        private string incrementIP(string ipToInc) {
            // Argument must be an address binary formatted with dots as 00010001.10110110.00101011.00110101
            string[] splittedIP = ipToInc.Split('.');
            // For each byte
            for (var i = 3 ; i >= 0 ; i--){
                // This byte has reached max value
                if(splittedIP[i] == "11111111")
                { 
                    // This is the bigger byte
                    if (i == 0 ) 
                        throw new Exception("Max value reached : 255.255.255.255. This method does not treat a maxValue (expect max value that an IPv4 can hold)");
                    else {
                        // Init rigth's bytes
                        splittedIP[i] = "00000000";
                        // Next byte
                        continue;
                    }
                }
                else
                {
                    // Inc current byte
                    splittedIP[i] = decimalToBin(Convert.ToInt32(binToDecimal(splittedIP[i])) +1 );
                    // this returns a 32 bit long string  (ip, binary, without dots)
                    return stringArrayToIP(splittedIP);
                }
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
    }
}