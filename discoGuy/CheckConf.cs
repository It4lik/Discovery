using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Runtime.InteropServices;

namespace discovery
{
    public class CheckConf
    { 
        /// JSON Object received as constructor parameter. Usually that is discoguy.json content.
        JObject _configuration;
        /// List of Discover object (one for each check found in configuration file)
        List<Discover> _checks;
        public CheckConf(string configurationFilePath) {
            // Get the JSON formatted configuration file in JSON Object
            try {
                _configuration = JObject.Parse(File.ReadAllText(getConfigurationFileContent()));
            }
            catch (System.Exception ex) {
                Console.WriteLine($"FATAL: Configuration file is not correctly formatted as JSON : {ex.ToString()}");
                Environment.Exit(10); // TODO : this should be non-blocking but just exit the creation of the class
            }
            
            CheckConfigFileSyntax();

            /*/

            // We are going to iterate on all checks asked by configuration file. 
            // Each comports every parameter we need to instanciate a new Discover object, and start scanning. 
            
            // These are the different parameters we need to get
            string checkName;     // Arbitrary name
            string subnet;        // Subnet address, CIDR formatted like "192.168.1.0/24"
            int scannedPort;      // Port targeted by the scan
            string redisHost;     // Redis host address and port, formatted like "192.168.1.10:6379"
            string checkType;     // Type of the check. This will be converted to a member of Discover.CheckType enum
            int subSize;          // This is the size of shrinked subnets. The initial network is split into multiple smaller subnets. Multiple threads can be spawn to iterate faster on every subnet.
            int maxThreads;       // Maximum number of threads running in parallel to scan the specified subnet
            JObject actionIfUp;   // Contains a JSON snippet describing an action to do when a host spawns on network
            JObject actionIfDown; //  Contains a JSON snippet describing an action to do when a host disappeared from network

            // Create a check object for each "checks" child in JSON conf
            foreach(KeyValuePair<string, JToken> check in (JObject)_configuration["checks"]) {
                checkName = (string)_configuration["checks"][check.Key]["name"];
                subnet = (string)_configuration["checks"][check.Key]["subnet"];
                scannedPort = (int)_configuration["checks"][check.Key]["port"];
                redisHost = (string)_configuration["checks"][check.Key]["redisHost"];
                checkType = (string)_configuration["checks"][check.Key]["checkType"];
                if ((string)_configuration["checks"][check.Key]["wantedSubnetSize"] != String.Empty)
                    subSize = (int)_configuration["checks"][check.Key]["wantedSubnetSize"];
                if ((string)_configuration["checks"][check.Key]["maxThreads"] != String.Empty)
                    maxThreads = (int)_configuration["checks"][check.Key]["maxThreads"];
                actionIfUp = (JObject)_configuration["checks"][check.Key]["actionIfUp"];
                actionIfDown = (JObject)_configuration["checks"][check.Key]["actionIfDown"];

                ActionJObjectToHostAction(actionIfDown);

               // subnetObject = new Subnet()
            }

            //Console.WriteLine(_checkName);
            */
        }

        private string getConfigurationFileContent(string configurationFilePath = "none") {
            if (configurationFilePath != "none")
                return configurationFilePath;
            else {
                // Determine if we need / or \ in paths : Windows or something else ? 
                char pathSeparator;
                if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows) == true)
                    pathSeparator = '\\';
                else 
                    pathSeparator = '/';

                // Path to configuration file (in project dir by default, Directory.GetCurrentDirectory() get it for us)
                return String.Concat(Directory.GetCurrentDirectory(), pathSeparator, "discoguy.json");
            }
        }

        private void ActionJObjectToHostAction(JObject actionToConvert) {
            // UNFINISHED
            HostAction action;
            string SSHHost;
            try {
            // Get action type from JSON if it is a member of Discover.CheckType enum
            if (Enum.IsDefined(typeof(Discover.CheckType), actionToConvert["ActionType"].ToString()))
                Enum.TryParse(actionToConvert["ActionType"].ToString(), out Discover.CheckType checkType);
            if (IPv4Address.IsIPAddress(actionToConvert["SSHHost"].ToString()))
                SSHHost = actionToConvert["SSHHost"].ToString();

            }
            catch (System.Exception) {

            }
            Console.WriteLine(actionToConvert["ActionType"]);
            Console.WriteLine(actionToConvert.ToString());

            //return action;
        }
        /// Verify the JSON configuration file syntax
        private bool CheckConfigFileSyntax() {
            bool isValid = true;

            // These are the different parameters we need to get
            string checkName;     // Arbitrary name
            string subnet;        // Subnet address, CIDR formatted like "192.168.1.0/24"
            int scannedPort;      // Port targeted by the scan
            string redisHost;     // Redis host address and port, formatted like "192.168.1.10:6379"
            string checkType;     // Type of the check. This will be converted to a member of Discover.CheckType enum
            int subSize;          // This is the size of shrinked subnets. The initial network is split into multiple smaller subnets. Multiple threads can be spawn to iterate faster on every subnet.
            int maxThreads;       // Maximum number of threads running in parallel to scan the specified subnet
            JObject actionIfUp;   // Contains a JSON snippet describing an action to do when a host spawns on network
            JObject actionIfDown; //  Contains a JSON snippet describing an action to do when a host disappeared from network

            // List of possible parameters
            List<string> mandatoryParameters = new List<string>(new string[] {"name", "subnet", "port", "redisHost", "checkType", "actionIfUp", "actionIfDown"});
            List<string> optionalParameters = new List<string>(new string[] {"maxThreads", "wantedSubnetSize"});

            try {
                // For each tests (each child property of "checks")
                foreach(KeyValuePair<string, JToken> check in (JObject)_configuration["checks"]) {
                    string currentCheck = check.Key.ToString();
                    // For each child property of current check (eg check parameters)
                    foreach(KeyValuePair<string, JToken> currentCheckParameter in (JObject)_configuration["checks"][check.Key]) {

                        // Get the value and the key of current check parameter
                        string value = currentCheckParameter.Value.ToString();
                        string key = currentCheckParameter.Key.ToString();

                        // If the paramater is not in possible paramaters, the configuration file is not valid
                        if ( ! (mandatoryParameters.Contains(key) && optionalParameters.Contains(key))) {
                            Console.WriteLine($"ERROR: {key} is not a valid parameter");
                            isValid = false;
                            continue;
                        }

                        // Test if values are empty (blocking if it's a mandatory parameter)
                        switch (value) {
                            case "":
                                if (mandatoryParameters.Contains(key)) {
                                    Console.WriteLine($"ERROR: {key} can NOT be empty.");
                                    isValid = false;
                                    continue;
                                } else if (optionalParameters.Contains(key)) {
                                    Console.WriteLine($"WARNING: {key} is empty.");
                                }
                                break;
                            default:
                                break;
                        }

                        // Test keys and verify their values
                        switch (key) {
                            // Subnet : must be CIDR formatted IPv4 network address
                            case "subnet":
                                if ( ! Subnet.verifyAddressCIDR(value)) {
                                    Console.WriteLine($"ERROR: {value} is not a valid CIDR network address (like '192.168.1.0/24') in {currentCheck}.");
                                    isValid = false;
                                    continue;
                                }
                                break;
                            // Port : must be an integer between 0 and 65535
                            case "port":
                                int n;
                                if (int.TryParse(value, out n)) {
                                    if (n > 65535 || n < 0) {
                                        Console.WriteLine($"ERROR: {value} is not set to a valid port in {currentCheck}.");
                                        isValid = false;
                                        continue;
                                    }   
                                }
                                else {
                                    Console.WriteLine($"ERROR: {value} is not set to a valid port in {currentCheck}.");
                                    isValid = false;
                                    continue;
                                }
                                break;
                            // RedisHost : must be an IPv4 formatted address and a port separated from the ':' character
                            case "redisHost":
                                if (value.Contains(":")) {
                                    if (value.Split(':').Length < 2 && value.Split(':').Length > 0) {
                                        string host = value.Split(':')[0]; string port = value.Split(':')[1];
                                        if ( ! IPv4Address.IsIPAddress(host)) {
                                            Console.WriteLine($"ERROR : Your Redis IP address {host} is not a valid IPv4 address (check {currentCheck}");
                                            isValid = false;
                                            continue;
                                        }
                                        if (int.TryParse(port, out n)) {
                                            if (n > 65535 || n < 0) {
                                                Console.WriteLine($"ERROR:  Your Redis port {port} is not a valid port (check {currentCheck}");
                                                isValid = false;
                                                continue;
                                            }
                                        }
                                        else { 
                                            Console.WriteLine($"ERROR:  Your Redis port {port} is not a valid port (check {currentCheck}");
                                            isValid = false;
                                            continue;
                                        }
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch (System.Exception) {

            }

            return isValid;
        }
    }
}