# Posh-SSH

## Description

Windows Powershell module that leverages a custom version of the [SSH.NET Library](https://github.com/sshnet/SSH.NET) to provide basic SSH functionality in Powershell.
The main purpose of the module is to facilitate automating actions against one or multiple SSH enabled servers.

This module is for Windows PowerShell 3.0 or above. It is compiled for .NET Framework 4.5.

## Install

To install the module including all source code you can just run in a PowerShell v3 the following command:

``` PowerShell
Install-Module -Name Posh-SSH
```

[![Install and New Sessions](https://www.youtube.com/watch?v=aZT5L_0aepE&list=PLFAOQ2hOvfsSL0N2kD_CyqHKlIA0byDL3/hqdefault.jpg)](https://www.youtube.com/watch?v=aZT5L_0aepE&list=PLFAOQ2hOvfsSL0N2kD_CyqHKlIA0byDL3)

## Support

* Provides functionality for automating SSH, SFTP and SCP actions.
* Supports diffie-hellman-group-exchange-sha256, diffie-hellman-group-exchange-sha1, diffie-hellman-group14-sha1 and diffie-hellman-group1-sha1 key exchange methods.
* Supports 3des-cbc, aes128-cbc, aes192-cbc, aes256-cbc, aes128-ctr, aes192-ctr, aes256-ctr, blowfish-cbc, cast128-cbc, arcfour and twofish encryptions.
* Supports hmac-md5, hmac-sha1, hmac-ripemd160, hmac-sha2-256, hmac-sha2-256-96, hmac-md5-96 and hmac-sha1-96 hashing algorithms.
* Supports publickey, password and keyboard-interactive authentication methods
* Supports RSA and DSA private key
* Supports DES-EDE3-CBC, DES-EDE3-CFB, DES-CBC, AES-128-CBC, AES-192-CBC and AES-256-CBC algorithms for private key encryption
* Supports SOCKS4, SOCKS5 and HTTP Proxy
* Remote, dynamic and local port forwarding
