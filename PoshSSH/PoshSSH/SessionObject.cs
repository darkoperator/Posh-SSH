using Renci.SshNet;
using System;


namespace SSH
{
    // Object for SSH Sessions
    public class SshSession
    {
        public Int32 SessionId;
        public string Host;
        public SshClient Session;
        public bool Connected
        {
            get { return Session.IsConnected; }
        }

        // Method for Connecing
        public void Connect()
        {
            Session.Connect();
        }

        // Method for disconecting session
        public void Disconnect()
        {
            Session.Disconnect();
        }
    }

    // Object for SFTP Sessions
    public class SftpSession
    {
        public Int32 SessionId;
        public string Host;
        public SftpClient Session;
        public bool Connected
        {
            get { return Session.IsConnected; }
        }
        public void Disconnect()
        {
            Session.Disconnect();
        }

        // Method for Connecing
        public void Connect()
        {
            Session.Connect();
        }
    }
}
