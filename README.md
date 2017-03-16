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
### Main (Program.cs)

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

### Docker tests

You need to checkout on docker_tests branch for this to work.
You'll find a docker-compose.yml at the root of the repo. It will launch three containers : 
- **redis** : store hosts data
- **sshserver** : simple alpine container. Provide a SSH endpoint. A passwordless UNIX user "john" is already created for test purposes.
- **dotnet** : is using official microsoft/dotnet image (latest build atm). The application is mounted as a volume in /app.

```shell
# Run the test docker-compose
$ docker-compose up -d

# SSH in sshserver container (port 4444 is exposed on host machine)
$ ssh john@localhost -p 4444
sshserver:~$ 

# Open another shell, and get one in dotnet container
$ docker exec -it discovery_disco_1 bash

# Basically, you need to manually restore packages and manually run the app. Once again, this is intended for test purposes. 
root@disco:/# cd /app
root@disco:/app# dotnet restore
# Wait for the restore to complete... (this will not take long)

# Run the app. For test purposes, the discovery run only once. (not using a while (true {} block in the Thread definition))
root@disco:/# dotnet run

# Go back in you SSH session on sshserver
sshserver:~$ ls
sshserver:~$ cat logfile

# You can try to add/remove hosts. Default parameters are scaning hosts on port 6379. You can spin-up additional Redis hosts (Redis is listening by default on port 6379) with the following command :
# Do not forget --network to add them in the same network as the docker-compose containers.
$ docker run -d --network discovery_disco_net redis
```