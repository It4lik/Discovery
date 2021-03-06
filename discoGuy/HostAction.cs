using System;
using Renci.SshNet;

namespace discovery
{
    public class HostAction
    {
        public enum ActionType { SSHExec, HTTPRequest };
        private ActionType _actionType;
        private ConnectionInfo _connInfo;
        private string _SSHCommand;
        private string _host;
        private int _port;
        public HostAction(ActionType actionType, string host, int port, string username, string password, string SSHCommand) {
            // Constructor of HostAction for SSH exec with password authentication
            if (actionType == ActionType.SSHExec) {
                _connInfo = new ConnectionInfo(host, port, username,
                    new AuthenticationMethod[]{
                        new PasswordAuthenticationMethod(username, password)
                    }
                );
                _actionType = actionType;
                _SSHCommand = SSHCommand;
                _host = host;
                _port = port;
            }
        }
        public void Execute(string currentScannedHost, string threadName) {
            // Execute action
            if (_actionType == ActionType.SSHExec) {
                // If action is SSHExec, initiate a new SSH connection and issue command

                // Replace host by given value
                _SSHCommand = _SSHCommand.Replace("<HOST>", currentScannedHost);
                // Replace time by current time
                string curTime = DateTime.Now.ToString("h:mm:ss tt");
                _SSHCommand = _SSHCommand.Replace("<TIME>", curTime);

                // TODO : Need better handling of different SSH errors
                try {
                    using (var sshclient = new SshClient(_connInfo)){
                        // Connect to remote host. Can fail. 
                        sshclient.Connect();
                        using(var cmd = sshclient.CreateCommand(_SSHCommand)){
                            cmd.Execute();
                            Console.WriteLine("INFO: {3} on {0}: Command {1} exited with {2} exit code through SSH connection.", currentScannedHost, cmd.CommandText, cmd.ExitStatus, threadName); 
                            }
                            sshclient.Disconnect();
                    }
                }
                catch (System.Exception) {
                        Console.WriteLine("ERROR: Can't issue {0} on host {1} via SSH on port {2}.", _SSHCommand, _host, _port);
                }
                // Replace given host by <HOST> (for next iteration). Same for TIME
                _SSHCommand = _SSHCommand.Replace(currentScannedHost, "<HOST>");
                _SSHCommand = _SSHCommand.Replace(curTime, "<TIME>");
            }
        }
    }
}
