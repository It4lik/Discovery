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

## Docker environment
**Intended for test purposes only**

Use the docker-compose.yml file to spin up three containers and a dedicated network
- a Redis host
- an alpine-based SSH server to receive actions
    - the Discovery service will write in the homedir of a user called John (/home/John) when a up is up or down
- a dotnet container with application mounted in /app

```shell
$ cd Discovery
// Switch on docker_tests branch
$ git checkout docker_tests

$ docker-compose up -d

// This is manually done. Intended for test purposes. 
$ docker exec -it discovery_disco_1 bash
// Cd in application directory
root@disco:/# cd /app
// Get the libs
root@disco:/app# dotnet restore
// Run the app. The subnet will be scanned onlyn once for test purposes.
root@disco:/app# dotnet run

// You can spin up additional containers to scan with the following command (or another service who is listening on port TCP 6379 (see Program.cs))
$ docker run -d -p 6379:6379 --network discovery_disco_net redis

// You can SSH with John's account on SSH server to see the logfile :
$ ssh john@localhost -p 4444
sshserver:~$ ls
```


## TODO
- Redis : use database object only
- Threads : better implementation (subnet shrinking)