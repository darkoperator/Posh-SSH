#Description
Powershell module that leverages the SSH.NET Library (http://sshnet.codeplex.com/) to provide basic SSH functionalyty in Powershell.

# Know Issues
* If SSH server is configured with only Keyboard-Interactive Authentication the Module will fail to authenticate (ESXi 5.x).
* No Port Fordwarding at the moment.
* No Proxy support at the moment.
* No Compression support at the moment.
* No interactives SSH and SFTP Shells at the moment.