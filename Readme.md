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
### Video Tutorials

#### What is Posh-SSH and Install
[![What is Posh-SSH and Install](https://i.ytimg.com/vi/83g6Yy6eTbo/hqdefault.jpg)](https://youtu.be/83g6Yy6eTbo?si=0s6Mx5GRzIt14BQI)

#### Creating and Managing Sessions
[![Creating and Managing Sessions](https://i.ytimg.com/vi/fNJgQT8wCOs/hqdefault.jpg)](https://youtu.be/fNJgQT8wCOs?si=uD5Vdpgs_oE843EO)
 
#### Managing Known Hosts
[![Managing Known Hosts](https://i.ytimg.com/vi/WyGPO1GPJQ4/hqdefault.jpg)](https://youtu.be/WyGPO1GPJQ4?si=DozUZlmUx1ghd1L4)

#### Command Execution
[![Executin Commands](https://i.ytimg.com/vi/ThOeR0kSSEY/hqdefault.jpg)](https://youtu.be/ThOeR0kSSEY)

## Support

* Provides functionality for automating SSH, SFTP and SCP actions.
* Supports SOCKS4, SOCKS5 and HTTP Proxy
* Remote, dynamic and local port forwarding

The following encryption methods are supported:
* aes128-ctr
* aes192-ctr
* aes256-ctr
* aes128-cbc
* aes192-cbc
* aes256-cbc
* 3des-cbc
* blowfish-cbc
* twofish-cbc
* twofish192-cbc
* twofish128-cbc
* twofish256-cbc
* arcfour
* arcfour128
* arcfour256
* cast128-cbc


The following key exchange methods are supported:
* curve25519-sha256
* curve25519-sha256<span></span>@libssh.org
* ecdh-sha2-nistp256
* ecdh-sha2-nistp384
* ecdh-sha2-nistp521
* diffie-hellman-group-exchange-sha256
* diffie-hellman-group-exchange-sha1
* diffie-hellman-group16-sha512
* diffie-hellman-group14-sha256
* diffie-hellman-group14-sha1
* diffie-hellman-group1-sha1


The module supports the following private key formats:
* RSA in OpenSSL PEM ("BEGIN RSA PRIVATE KEY") and ssh.com ("BEGIN SSH2 ENCRYPTED PRIVATE KEY") format
* DSA in OpenSSL PEM ("BEGIN DSA PRIVATE KEY") and ssh.com ("BEGIN SSH2 ENCRYPTED PRIVATE KEY") format
* ECDSA 256/384/521 in OpenSSL PEM format ("BEGIN EC PRIVATE KEY")
* ECDSA 256/384/521, ED25519 and RSA in OpenSSH key format ("BEGIN OPENSSH PRIVATE KEY")

Private keys can be encrypted using one of the following cipher methods:
* DES-EDE3-CBC
* DES-EDE3-CFB
* DES-CBC
* AES-128-CBC
* AES-192-CBC
* AES-256-CBC

The module supports the following host key algorithms:
* ssh-ed25519
* ecdsa-sha2-nistp256
* ecdsa-sha2-nistp384
* ecdsa-sha2-nistp521
* rsa-sha2-512
* rsa-sha2-256
* ssh-rsa
* ssh-dss

The module supports the following MAC algorithms:
* hmac-sha2-256
* hmac-sha2-512
* hmac-sha2-512-96
* hmac-sha2-256-96
* hmac-sha1
* hmac-sha1-96
* hmac-md5
* hmac-md5-96
* hmac-sha2-256-etm<span></span>@openssh.com
* hmac-sha2-512-etm<span></span>@openssh.com
* hmac-sha1-etm<span></span>@openssh.com
* hmac-sha1-96-etm<span></span>@openssh.com
* hmac-md5-etm<span></span>@openssh.com
* hmac-md5-96-etm<span></span>@openssh.com

## Donate

If you find the project useful and wish to support it.
<a href="https://www.paypal.com/donate/?hosted_button_id=RL9PJH2XTDKSJ">
  <img src="https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif" alt="Donate with PayPal" />
</a>
