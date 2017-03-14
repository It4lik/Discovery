using System;
using System.Collections.Generic;
using System.Linq;

// Used to issue Redis connections
using StackExchange.Redis;

namespace discovery
{
    public class Stocker
    {
        // Class used to issue Redis operations 
        // Primarily used to read/write simple key/value pairs

        // We use string.Concat(prefix, "_", host) to always have Redis keys with a prefix (subnetIdentifier) and a name which is the host's name

        private string _redisHost; // Redis instance IP address
        private ConnectionMultiplexer _redis; // Used to establish Redis connection 
        private IDatabase _db; // IDatabase instance used to read/write in Redis' db instance (eg in a cluster)
        private IServer _server; // IServer instance used to read/write in Redis' server instance (eg NOT in a cluster)

        public Stocker(string redisHost, int redisPort) {
            // The constructor is basically used to initialize Redis' connection
            // That means that each instance of this class will instantiate a new Redis' connection (to same Redis instance, or not)
            _redisHost = string.Concat(redisHost, ":", redisPort);
            try
            {
                _redis = ConnectionMultiplexer.Connect(_redisHost);
                _db = _redis.GetDatabase();
                _server = _redis.GetServer(redisHost, redisPort);
            }
            catch (System.Exception)
            {
                Console.WriteLine("FATAL: Can't connect to Redis. Exiting.");
                System.Environment.Exit(3);
            }

        }
        
        public string GetRedisHost() {
            // Getter for the Redis' address
            return _redisHost;
        }

        public List<string> GetSubnetHosts(string prefix) {
            List<string> currentHosts = new List<string>();
            // searchedString will contain the prefix search in Redis : "Subnet_"
            string searchedString = String.Concat(prefix, "_*");
            // Add each key that matches the pattern "Subnet_" to currentHosts (return value)
            foreach (var key in _server.Keys(pattern: searchedString)) {
                currentHosts.Add(key.ToString().Split('_').Last());
            }
            return currentHosts;
        }

        public void Write(string prefix, string host) {
            // Insert a new host in Redis : new key/value : prefix_host:valueUp
            RedisKey hostUp = string.Concat(prefix, "_", host);
            _db.StringSet(hostUp, "UP");
        }

        public string ReadHost(string prefix, string host) {
            // Try to read an host in Redis : prefix_host is read
            RedisKey rKey = string.Concat(prefix, "_", host);
            return _db.StringGet(rKey);
        }

        public void MarkHostDown(string prefix, string host) {
            // Mark a host as down : prefix_host:
            string hostDown = String.Concat(prefix, "_", host);
            _db.StringSet(hostDown, "DOWN");
        }

        public bool DoesKeyExist(string prefix, string host) {
            // Tell if a host exists in Redis : prefix_host is searched
            RedisKey rKey = string.Concat(prefix, "_", host);
            return _db.KeyExists(string.Concat(prefix, "_", host));
        }
    }
}