using System;
using System.
namespace discovery
{
    public class CheckConf
    { 

        /// Name of the check. This will be used to prefix data in Redis.
        private string _checkName;
        /// Subnet on which issue the checks. It must be CIDR-formatted (like "192.168.1.0/24)
        private string _targetedSubnet;
        /// Redis host formatted like : "address:port". Hostname not supported at the moment.
        private string _redisHost;
        /// Check type. Will be converted to a Subnet.CheckType  later on. 
        private string _checkType;
        /// Wanted size of scanned subnet by threads. Used to shrink initial subnets in multiple smaller ones.
        private int _subnetSize;
        /// Maximum simultaneously running threads.
        private int _maxThreads;
        /// Action to do is a up host is found
        private string actionUp;
        /// Action to do is a down host is found (and was previously up)
        private string actionDown;
    }
}