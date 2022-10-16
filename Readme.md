# Posh-SSH

## Description

Windows Powershell module that leverages a custom version of the [SSH.NET Library](https://github.com/sshnet/SSH.NET) to provide basic SSH functionality in Powershell.
The main purpose of the module is to facilitate automating actions against one or multiple SSH enabled Linux servers from a Windows Host. As of version 3.x the module can be used in Linux and Mac OS using .Net Standard.

This module is for Windows PowerShell 5.1 or PowerShell 7.x., On Windows Server, version 1709 or older .Net Framework 4.8 or above is required for the proper loading of the module. 

Except as represented in this agreement, all work product by Developer is provided ​“AS IS”. Developer makes no other warranties, express or implied, and hereby disclaims all implied warranties, including any warranty of merchantability and warranty of fitness for a particular purpose.

## Install

To install the module run the command command:

``` PowerShell
Install-Module -Name Posh-SSH
```

[![Install and New Sessions](https://i.ytimg.com/vi/aZT5L_0aepE/hqdefault.jpg)](https://www.youtube.com/watch?v=aZT5L_0aepE&list=PLFAOQ2hOvfsSL0N2kD_CyqHKlIA0byDL3)

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
