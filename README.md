# Discovery application

## Goal
Wanna scale some apps ? Want to automatically reconfigure loadbalancers ? Clusters ? :o

Give this app some arguments and it'll do the work :
- subnet (CIDR formatted)
- a check to issue on all hosts of specified subnet (see *Checks*)
- action to do if a host is found (see *Actions*)
- action to do if a host disappeared (see *Actions*)

**I'll need a Redis endpoint to work !** 
- [redis.io](https://redis.io/) 
- [Redis docker container](https://hub.docker.com/_/redis/)

## Checks
- **Supported:**
    - TCP connection
- *Planned:*
    - HTTP request

## Actions
- **Supported:**
    - Remote shell exec via SSH
- *Planned:*
    - HTTP request

**Actions support substituing :**
- **\<HOST>** is a specific host targeted by a check
- **\<TIME>** will be the time when the action was triggered

## Basic usage
```C++
// Subnet on which do the tests : <SUBNET_ADDRESS> CIDR-formatted like "192.168.1.0/24"
Subnet yourNetwork = new Subnet(<SUBNET_ADDRESS>);

// Redis connection to use
Stocker redis = new Stocker(<REDIS_IP_ADDRESS>, <REDIS_PORT>);

// Action to do if a host is up
HostAction actionIfUp = new HostAction(HostAction.ActionType.SSHExec,
     <SSH_HOST>, <SSH_PORT>, <SSH_USER>, <SSH_PASS>,
     "echo \"<HOST> became UP at <TIME>\"  >> /log/file");

// Action to do if a host is down
HostAction actionIfDown = new HostAction(HostAction.ActionType.SSHExec,
     <SSH_HOST>, <SSH_PORT>, <SSH_USER>, <SSH_PASS>, 
     "echo \"<HOST> became DOWN at <TIME>\"  >> /log/file");

// Issue discovery
Discover disco = new Discover(Discover.CheckType.tcp, yourNetwork, 6379, actionIfUp, actionIfDown, redis, "SuperNetworkName");
disco.startDiscovery();
```