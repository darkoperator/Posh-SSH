---
external help file: Posh-SSH-help.xml
online version: https://github.com/darkoperator/Posh-SSH/tree/master/docs
schema: 2.0.0
---

# New-SSHLocalPortForward

## SYNOPSIS
Redirects traffic from a local port to a remote host and port via a SSH Session.

## SYNTAX

### Index (Default)
```
New-SSHLocalPortForward [-BoundHost] <String> [-BoundPort] <Int32> [-RemoteAddress] <String>
 [-RemotePort] <Int32> [-SessionId] <Int32>
```

### Session
```
New-SSHLocalPortForward [-BoundHost] <String> [-BoundPort] <Int32> [-RemoteAddress] <String>
 [-RemotePort] <Int32> [-SSHSession] <SshSession>
```

## DESCRIPTION
Redirects TCP traffic from a local port to a remote host and port via a SSH Session.

## EXAMPLES

### -------------------------- EXAMPLE 1 --------------------------
```
Forward traffic from 0.0.0.0:8081 to 10.10.10.1:80 thru a SSH Session
```

PS C:\\\> New-SSHLocalPortForward -Index 0 -LocalAdress 0.0.0.0 -LocalPort 8081 -RemoteAddress 10.10.10.1 -RemotePort 80 -Verbose
 VERBOSE: Finding session with Index 0
 VERBOSE: 0
 VERBOSE: Adding Forward Port Configuration to session 0
 VERBOSE: Starting the Port Forward.
 VERBOSE: Forwarding has been started.

 PS C:\\\> Invoke-WebRequest -Uri http://localhost:8081


 StatusCode        : 200
 StatusDescription : OK
 Content           :
                     \<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN"
                             "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"\>
                     \<html\>
                         \<head\>
                             \<script type="text/javascript" src="/javascript/scri...
 RawContent        : HTTP/1.1 200 OK
                     Expires: Tue, 16 Apr 2013 03:43:18 GMT,Thu, 19 Nov 1981 08:52:00 GMT
                     Cache-Control: max-age=180000,no-store, no-cache, must-revalidate, post-check=0, pre-check=0
                     Set-Cookie: PHPSESS...
 Forms             : {iform}
 Headers           : {\[Expires, Tue, 16 Apr 2013 03:43:18 GMT,Thu, 19 Nov 1981 08:52:00 GMT\], \[Cache-Control, max-age=180000,no-store, no-cache,
                     must-revalidate, post-check=0, pre-check=0\], \[Set-Cookie, PHPSESSID=d53d3dc62ffac241112bcfd16af36bb8; path=/\], \[Pragma, no-cache\]...}
 Images            : {}
 InputFields       : {@{innerHTML=; innerText=; outerHTML=\<INPUT onchange=clearError(); onclick=clearError(); tabIndex=1 id=usernamefld class="formfld user"
                     name=usernamefld\>; outerText=; tagName=INPUT; onchange=clearError();; onclick=clearError();; tabIndex=1; id=usernamefld; class=formfld
                     user; name=usernamefld}, @{innerHTML=; innerText=; outerHTML=\<INPUT onchange=clearError(); onclick=clearError(); tabIndex=2
                     id=passwordfld class="formfld pwd" type=password value="" name=passwordfld\>; outerText=; tagName=INPUT; onchange=clearError();;
                     onclick=clearError();; tabIndex=2; id=passwordfld; class=formfld pwd; type=password; value=; name=passwordfld}, @{innerHTML=;
                     innerText=; outerHTML=\<INPUT tabIndex=3 class=formbtn type=submit value=Login name=login\>; outerText=; tagName=INPUT; tabIndex=3;
                     class=formbtn; type=submit; value=Login; name=login}}
 Links             : {}
 ParsedHtml        : mshtml.HTMLDocumentClass
 RawContentLength  : 5932

## PARAMETERS

### -BoundHost
{{Fill BoundHost Description}}

```yaml
Type: String
Parameter Sets: (All)
Aliases: 

Required: True
Position: 2
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -BoundPort
{{Fill BoundPort Description}}

```yaml
Type: Int32
Parameter Sets: (All)
Aliases: 

Required: True
Position: 3
Default value: 0
Accept pipeline input: False
Accept wildcard characters: False
```

### -RemoteAddress
{{Fill RemoteAddress Description}}

```yaml
Type: String
Parameter Sets: (All)
Aliases: 

Required: True
Position: 4
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -RemotePort
{{Fill RemotePort Description}}

```yaml
Type: Int32
Parameter Sets: (All)
Aliases: 

Required: True
Position: 5
Default value: 0
Accept pipeline input: False
Accept wildcard characters: False
```

### -SSHSession
{{Fill SSHSession Description}}

```yaml
Type: SshSession
Parameter Sets: Session
Aliases: Session

Required: True
Position: 1
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -SessionId
{{Fill SessionId Description}}

```yaml
Type: Int32
Parameter Sets: Index
Aliases: Index

Required: True
Position: 1
Default value: 0
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

## INPUTS

## OUTPUTS

## NOTES

## RELATED LINKS

