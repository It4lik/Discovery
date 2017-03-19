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
            
            /*

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

             /*   _checks.Add(new Discover(
                    Enum.Parse(Discover.CheckType, checkType),
                    new Subnet(subnet),
                    scannedPort,

                    )*/
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
            HostAction action;
            string SSHHost;
            try {
            // Get action type from JSON if it is a member of Discover.CheckType enum
            if (Enum.IsDefined(typeof(Discover.CheckType), actionToConvert["ActionType"].ToString()))
                Enum.TryParse(actionToConvert["ActionType"].ToString(), out Discover.CheckType checkType);
            if (IPv4Address.IsIPAddress(actionToConvert["SSHHost"].ToString()))
                SSHHost = actionToConvert["SSHHost"].ToString();

            }
            Console.WriteLine(actionToConvert["ActionType"]);
            Console.WriteLine(actionToConvert.ToString());

            //return action;
        }
        /// Verify the JSON configuration file syntax
        private bool CheckConfigFileSyntax(JObject configurationFile) {
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

            try {
                foreach(KeyValuePair<string, JToken> check in (JObject)_configuration["checks"]) {
                    foreach(string currentKey in _configuration["checks"].Values()) {
                        Console.WriteLine(currentKey);
                    }
                    
                    
                    
                    if ((string)_configuration["checks"][check.Key]["name"].ToString() == String.Empty) {
                        Console.WriteLine($"CheckName of {check.Key} can NOT be empty.");
                        return false;
                    }
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
                }
            }

            return isValid;
        }
    }
}