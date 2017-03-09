using StackExchange.Redis;

namespace discovery
{
    public class Stocker
    {
        // Class used to issue Redis operations 
        // Primarily used to read/write simple key/value pairs
        private string _redisHost;
        private ConnectionMultiplexer _redis;
        private IDatabase _db;

        public Stocker(string redisHost, int redisPort) {
            _redisHost = string.Concat(redisHost, ":", redisPort);
            _redis = ConnectionMultiplexer.Connect("127.0.0.1:6379");
            _db = _redis.GetDatabase();
        }
        
        public string GetHost() {
            return _redisHost;
        }

        public void Write(string key, string value) {
            RedisKey rKey = key;
            RedisValue rValue = value;
            _db.StringSet(rKey, rValue);
        }

        public string Read(string key) {
            RedisKey rKey = key;
            return _db.StringGet(rKey);
        }

        public bool doesKeyExist(string key) {
            RedisKey rKey = key;
            return _db.KeyExists(key);
        }
    }
}