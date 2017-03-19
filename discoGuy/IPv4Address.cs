using System;
using System.Text.RegularExpressions;

namespace discovery
{
    public abstract class IPv4Address
    {
        /// Converts a "192.168.1.0" address to his binary equivalent WITHOUT the dots (eg. a 32 characters long string (binary))
        protected string DecimalIPtoBinIP(string ipToConvert) {
            string tempIP = string.Empty; string toConcat = string.Empty;
            for (int i = 0; i < ipToConvert.Split('.').Length; i++)
            {
                toConcat = BinaryTools.decimalToBin(ipToConvert.Split('.')[i]);
                tempIP = String.Concat(tempIP, toConcat); 
            }
            return tempIP;
        }

        protected string BinIPtoBinIPWithDots(string binIPWithoutDots) {
            // Convert a binary IP without dots to a binary with dots
            // Like 01000010111010100101011010101001 to 01001001.10020101.11111011.11001011
            return String.Concat(binIPWithoutDots.Substring(0, 8), '.', binIPWithoutDots.Substring(8, 8), '.', binIPWithoutDots.Substring(16, 8), '.', binIPWithoutDots.Substring(24, 8));
        }
        /// Convert a binary IP with dots to a binary without dots. Like 01001001.10020101.11111011.11001011 to 01000010111010100101011010101001
        protected string BinIPtoBinIPWithoutDots(string binIPWithDots) {
            return binIPWithDots.Replace(".", "");
        }

        /// Argument is an IP address, as a string, binary formatted, with dots like "11000000.10101000.00000001.00000000". It returns a decimal IP address like "192.168.1.0" format (decimal)
        protected string BinIPtoDecimalIP(string stringNumberToConvert) {

            stringNumberToConvert = BinIPtoBinIPWithoutDots(stringNumberToConvert);
            return String.Concat(BinaryTools.binToDecimal(stringNumberToConvert.Substring(0, 8)), '.', BinaryTools.binToDecimal(stringNumberToConvert.Substring(8, 8)), '.', BinaryTools.binToDecimal(stringNumberToConvert.Substring(16, 8)), '.', BinaryTools.binToDecimal(stringNumberToConvert.Substring(24, 8)));
        }
        protected string StringArrayToIP(string [] stringArray) {
            // takes a string array and returns the concatenations of all the array and dots (array with values 192,168,0,1 as paramater returns "192.168.0.1")
            return String.Concat(stringArray[0], ".", stringArray[1], ".", stringArray[2], ".", stringArray[3]);
        }
        /// Argument must be an address binary formatted with dots as 00010001.10110110.00101011.00110101
        protected string IncrementIP(string ipToInc) {
            string[] splittedIP = ipToInc.Split('.');
            // For each byte
            for (var i = 3 ; i >= 0 ; i--) {
                // This byte has reached max value
                if(splittedIP[i] == "11111111") { 
                    // This is the bigger byte
                    if (i == 0 ) 
                        throw new Exception("Max value reached : 255.255.255.255. This method does not treat a maxValue (expect max value that an IPv4 can hold)");
                    else {
                        // Init right's bytes
                        splittedIP[i] = "00000000";
                        // Next byte
                        continue;
                    }
                }
                else {
                    // Inc current byte
                    splittedIP[i] = BinaryTools.incrementBin(splittedIP[i]);
                    break;
                }
            }
            // this returns a 32 bit long string  (ip, binary, without dots)
            return StringArrayToIP(splittedIP);
        }
        /// Used to verify that a string is an IPv4 address 
        public static bool IsIPAddress(string IPAddress) {
            Regex CIDRRegex = new Regex(@"^(([1-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.)(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){2}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$");
            return CIDRRegex.IsMatch(IPAddress);
        }

    }
}