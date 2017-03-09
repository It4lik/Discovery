using System.Net;
using System.Text.RegularExpressions;
using System;
namespace discovery
{
    public class Subnet
    {
        // Class used to make IPv4 operations
        // This is gonna be exploded into an interface and a "Subnet" class that we can instantiate with one subnet (CIDR format) 
        private string _CIDRAddress;
        private  string _networkIP;
        private int _maskCIDR; // like "24"
        private string _netmask; // like "1111111111...100000..0000"
        private string _firstIP; // binary WITH dots
        private string _lastIP; // binary WITH dots
        
        public Subnet(string addr) {
            _CIDRAddress = addr;
            if (verifyAddressCIDR(_CIDRAddress)) {
                _networkIP = _CIDRAddress.Split("/")[0];
                _maskCIDR = System.Convert.ToInt32(_CIDRAddress.Split("/")[1]);
                getNetmask(_networkIP, _maskCIDR);
                getFirstAndLastIP(_networkIP, _maskCIDR);

            }
            System.Console.WriteLine(_firstIP);
            System.Console.WriteLine("That was the first ip");
            System.Console.WriteLine(_lastIP);
            System.Console.WriteLine("That was the last ip");
            System.Console.WriteLine(_netmask);         
            System.Console.WriteLine(binIPtoDecimalIP(_firstIP));
            
            iterateOnSubnet(_firstIP, _lastIP, _maskCIDR);

        }
        public bool verifyAddressCIDR(string CIDRAddress) {
            Regex CIDRRegex = new Regex(@"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\/(([0-9])|([12][0-9])|(3[0-2]))$");
            return CIDRRegex.IsMatch(CIDRAddress);
        }

        private void getNetmask(string netIP, int maskCIDR) {
            for (int i = 0; i < maskCIDR; i++)
            {
                _netmask = System.String.Concat(_netmask, "1");
            }
            for (int i = maskCIDR; i < 32; i++)
            {
                _netmask = System.String.Concat(_netmask, "0");
            }
        }

        private string decimalToBin(int stringNumberToConvert) {
            return Convert.ToString(stringNumberToConvert, 2);
        }
        private string decimalToBin(string stringNumberToConvert) {
            return Convert.ToString(Convert.ToInt32(stringNumberToConvert), 2);
        }

        private string binToDecimal(string stringNumberToConvert) {
            return Convert.ToInt32(stringNumberToConvert, 2).ToString();
        }

        private string decimalIPtoBinIP(string ipToConvert) {
            // convert a "192.168.1.0" address to his binary equivalet WITHOUT the dots (eg. a 32 characters long string (binary))
            string tempIP = string.Empty; string toConcat = string.Empty;
            for (int i = 0; i < ipToConvert.Split(".").Length; i++)
            {
                toConcat = decimalToBin(ipToConvert.Split(".")[i]);
                while (toConcat.Length < 8)
                {
                    toConcat = String.Concat('0', toConcat);
                }
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
            string networkBinIP = decimalIPtoBinIP(networkIP);
            char[] tempFirstIPChar = networkBinIP.ToCharArray(0, 32);
            char[] tempLastIPChar = networkBinIP.ToCharArray(0, 32);

            for (int i = maskCIDR; i < 31; i++) // 31 because we do'nt need to set the last bit : it will always be 1 for the first address (with, it's the subnet address)
            {
                tempFirstIPChar[i] = '0';
                tempLastIPChar[i] = '1';
            }
            tempFirstIPChar[31] = '1';
            tempLastIPChar[31] = '0';
            _firstIP = binIPtoBinIPWithDots(new string(tempFirstIPChar));
            _lastIP = binIPtoBinIPWithDots(new string(tempLastIPChar));
        }

        private string[] IPtoStringArray(string iPtoConvert) {
            return iPtoConvert.Split('.');
        }

        private string stringArrayToIP(string [] stringArray) {
            return String.Concat(stringArray[0], stringArray[1], stringArray[2], stringArray[3]);
        }

        private string incrementIP(string ipToInc) {
            // argument must be an addresse binary formatted with dots as 00010001.10110110.00101011.00110101
            string[] splittedIP = ipToInc.Split('.');
            if (splittedIP[3] == "11111111") {
                if (splittedIP[2] == "11111111") {
                    if (splittedIP[1] == "11111111") {
                        if (splittedIP[0] == "11111111") {
                            return ipToInc; // max value reached : 255.255.255.255. This method does not treat a maxValue (expect max value that an IPv4 can hold)
                        }
                        else {
                            splittedIP[3] = "00000000";
                            splittedIP[2] = "00000000";
                            splittedIP[1] = "00000000";
                            splittedIP[0] = decimalToBin(binToDecimal(splittedIP[0] + 1));
                        }
                    }
                    else {
                        splittedIP[3] = "00000000";
                        splittedIP[2] = "00000000";
                        splittedIP[1] = decimalToBin(binToDecimal(splittedIP[1] + 1));
                    }
                }
                else {
                    splittedIP[3] = "00000000";
                    splittedIP[2] = decimalToBin(binToDecimal(splittedIP[2] + 1));
                }
            }
            else {
                Console.WriteLine("splitted 3 c'est");
                Console.WriteLine(splittedIP[3]);
                Console.WriteLine(binToDecimal(splittedIP[3]));
                Console.WriteLine(binToDecimal(splittedIP[3]) +1 );
                Console.WriteLine("=============\n");

                splittedIP[3] = decimalToBin(binToDecimal(splittedIP[3]) + 1);
                //Console.WriteLine(splittedIP[3]);
            }
            return stringArrayToIP(splittedIP);
        }

        private void iterateOnSubnet(string firstIP, string lastIP, int maskCIDR) {
            string tempBinIP = firstIP;
            string endBinIP = lastIP;
            while (tempBinIP != endBinIP) {
                Console.WriteLine(binIPtoDecimalIP(tempBinIP));
                incrementIP(tempBinIP);
            }
        }
    }
}