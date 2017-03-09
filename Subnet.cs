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
        private int _maskCIDR;
        private string _netmask;
        private string _firstIP;
        private string _lastIP;
        
        public Subnet(string addr) {
            _CIDRAddress = addr;
            if (verifyAddressCIDR(_CIDRAddress)) {
                _networkIP = _CIDRAddress.Split("/")[0];
                _maskCIDR = System.Convert.ToInt32(_CIDRAddress.Split("/")[1]);
                getNetmask(_networkIP, _maskCIDR);
                getFirstIP(_networkIP, _maskCIDR);

            }
            System.Console.WriteLine(_firstIP);
            System.Console.WriteLine("That was the first ip");
            System.Console.WriteLine(_lastIP);
            System.Console.WriteLine("That was the last ip");
            System.Console.WriteLine(_netmask);         
            System.Console.WriteLine(binIPtoDecimalIP(_firstIP));
            
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

        private string binIPtoDecimalIP(string stringNumberToConvert) {
            // convert a 32-character long string (binary IP address without dots) to a "192.168.1.0" format (decimal)
            // takes four 8-character block, convert them to decimal and concatenate them (using a dot as separator)
            return String.Concat(binToDecimal(stringNumberToConvert.Substring(0, 8)), '.', binToDecimal(stringNumberToConvert.Substring(8, 8)), '.', binToDecimal(stringNumberToConvert.Substring(16, 8)), '.', binToDecimal(stringNumberToConvert.Substring(24, 8)));
        }

        private void getFirstIP(string networkIP, int maskCIDR) {
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
            _firstIP = new string(tempFirstIPChar);
            _lastIP = new string(tempLastIPChar);
        }

        /*private int[] getLastIP() {
            
        }*/
    }
}