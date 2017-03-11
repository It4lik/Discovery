using System;

namespace discovery
{
    public static class BinaryTools
    {
        public static string decimalToBin(string stringNumberToConvert) {
            // convert a decimal int to a binary int (only strings)
            string returnedString; 
            returnedString = Convert.ToString(Convert.ToInt32(stringNumberToConvert), 2);
            while (returnedString.Length < 8) {
                returnedString = String.Concat('0', returnedString);
            }
            return returnedString;
        }
        public static string decimalToBin(int stringNumberToConvert) {
            // overload : pass an int instead of a string as an argument      
            string currentInt = stringNumberToConvert.ToString();
            return decimalToBin(currentInt);
        }
        public static string binToDecimal(string stringNumberToConvert) {
            // convert a binary int to a decimal int (only strings)
            return Convert.ToInt32(stringNumberToConvert, 2).ToString();
        }
    }
}