#Description
Powershell module that leverages the SSH.NET Library (http://sshnet.codeplex.com/) to provide basic SSH functionalyty in Powershell.

# ToDo
* Add SCP functions.
* Add function help comments.
* Add output formating PSXML1.
* Add update function using GIT.

# Know Issues
* If SSH server is configured with only Keyboard-Interactive Authentication the Module will fail to authenticate (ESXi 5.x).
* AES-128 Passphrased keys are not supported by the library at this moment.
* No Compression support at the moment.
* No interactives SSH and SFTP Shells at the moment.