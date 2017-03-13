using System;
using Renci.SshNet;

namespace discovery
{
    class HostAction
    {
        public enum ActionType { SSHExec, HTTPRequest };
        private ActionType _actionType;
        private ConnectionInfo _connInfo;
        private string _SSHCommand;
        private string _host;
        public HostAction(ActionType actionType, string host, int port, string username, string password, string SSHCommand) {
            
            if (actionType == ActionType.SSHExec) {
                _connInfo = new ConnectionInfo(host, port, username,
                    new AuthenticationMethod[]{
                        new PasswordAuthenticationMethod(username, password)
                    }
                );
                _SSHCommand = SSHCommand;
                _host = host;    
            }
        }
        public void Execute() {
            using (var sshclient = new SshClient(_connInfo)){
                sshclient.Connect();
                using(var cmd = sshclient.CreateCommand(_SSHCommand)){
                    cmd.Execute();
                    Console.WriteLine("[{0}]: Command {1} exited with {2} exit code", _host, cmd.CommandText, cmd.ExitStatus); 
                }
                sshclient.Disconnect();
            }
        }
    }
}
