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
        private IDatabase _db; // IDatabase instance used to read/write in Redis' instance

        public Stocker(string redisHost, int redisPort) {
            // The constructor is basically used to initialize Redis' connection
            // That means that each instance of this class will instantiate a new Redis' connection (to same Redis instance, or not)
            _redisHost = string.Concat(redisHost, ":", redisPort);
            _redis = ConnectionMultiplexer.Connect(_redisHost);
            _db = _redis.GetDatabase();
        }
        
        public string GetHost() {
            // Getter for the Redis' address
            return _redisHost;
        }

        public void Write(string prefix, string host, string valueUp) {
            // Insert a new host in Redis : new key/value : prefix_host:valueUp
            RedisKey rKey = string.Concat(prefix, "_", host);
            RedisValue rValue = valueUp;
            _db.StringSet(rKey, rValue);
        }

        public string Read(string prefix, string host) {
            // Try to read an host in Redis : prefix_host is read
            RedisKey rKey = string.Concat(prefix, "_", host);
            return _db.StringGet(rKey);
        }

        public bool doesKeyExist(string prefix, string host) {
            // Tell if a host exists in Redis : prefix_host is searched
            RedisKey rKey = string.Concat(prefix, "_", host);
            return _db.KeyExists(string.Concat(prefix, "_", host));
        }
    }
}