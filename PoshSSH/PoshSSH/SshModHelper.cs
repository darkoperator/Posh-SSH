using Renci.SshNet;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;

namespace SSH
{
    // Class for creating PS Custom Objects
    public class SshModHelper
    {
        // Create Custom Object from Hashtable
        public static PSObject CreateCustom(Hashtable properties)
        {
            var obj = new PSObject();
            foreach (DictionaryEntry noteProperty in properties)
            {
                obj.Properties.Add(new PSNoteProperty(noteProperty.Key.ToString(), noteProperty.Value));
            }
            return obj;
        }

        public static SshSession AddToSshSessionCollection(SshClient sshclient, SessionState pssession)
        {
            //Set initial variables
            var obj = new SshSession();
            var sshSessions = new List<SshSession>();
            var index = 0;

            // Retrieve existing sessions from the global variable.
            var sessionvar = pssession.PSVariable.GetValue("Global:SshSessions") as List<SshSession>;

            // If sessions exist we set the proper index number for them.
            if (sessionvar != null)
            {
                sshSessions.AddRange(sessionvar);
                index = sshSessions.Count;
            }

            // Create the object that will be saved
            obj.SessionId = index;
            obj.Host = sshclient.ConnectionInfo.Host;
            obj.Session = sshclient;
            sshSessions.Add(obj);

            // Set the Global Variable for the sessions.
            pssession.PSVariable.Set((new PSVariable("Global:SshSessions", sshSessions, ScopedItemOptions.AllScope)));

            return obj;
        }

        public static SftpSession AddToSftpSessionCollection(SftpClient sftpclient, SessionState pssession)
        {
            //Set initial variables
            var obj = new SftpSession();
            var sftpSessions = new List<SftpSession>();
            var index = 0;

            // Retrive existing sessions from the globla variable.
            var sessionvar = pssession.PSVariable.GetValue("Global:SFTPSessions") as List<SftpSession>;

            // If sessions exist we set the proper index number for them.
            if (sessionvar != null)
            {
                sftpSessions.AddRange(sessionvar);
                index = sftpSessions.Count;
            }

            // Create the object that will be saved
            obj.SessionId = index;
            obj.Host = sftpclient.ConnectionInfo.Host;
            obj.Session = sftpclient;
            sftpSessions.Add(obj);

            // Set the Global Variable for the sessions.
            pssession.PSVariable.Set((new PSVariable("Global:SFTPSessions", sftpSessions, ScopedItemOptions.AllScope)));
            return obj;
        }
    }
}
