#Description
Powershell module that leverages a custom version of the SSH.NET Library (http://sshnet.codeplex.com/) to provide basic SSH functionalyty in Powershell.

#Install
To install the module including all source code you can just run in a PowerShell v2 or v3 the following command:
<pre>
iex (New-Object Net.WebClient).DownloadString("https://gist.github.com/darkoperator/6152630/raw/c67de4f7cd780ba367cccbc2593f38d18ce6df89/instposhsshdev")
</pre>

#ChangeLog
##Version 1.1
* Added functions for managing SSH Trusted Host list.
* SCP, SSH Session and SFTP Session cmdlets now verify the SSH Host Fingerprint.
* Complete refactor of the cmdlets for SSH Session, SFTP Session and SCP.
* Added Download and Upload Progress to SCP cmdlets.
* Patched the Renci SSH .Net library to correct problems when uploading using SCP.
* Included source for patched Renci SSH .Net library.

# ToDo
* Add progress for SFTP upload and Download


# Know Issues
* No Compression support at the moment.
* Once a Download or Upload is started only way to cancel is by closing PowerShell.
