# .ExternalHelp Posh-SSH.psm1-Help.xml
function Get-SSHTrustedHost
{
    [CmdletBinding()]
    [OutputType([int])]
    Param()

    [SSH.TrustedKeyMng]::GetKeys()
}


# .ExternalHelp Posh-SSH.psm1-Help.xml
function New-SSHTrustedHost
{
   [CmdletBinding()]
    Param
    (
        # IP Address of FQDN of host to add to trusted list.
        [Parameter(Mandatory=$true,
                   ValueFromPipelineByPropertyName=$true,
                   Position=0)]
        $SSHHost,

        # SSH Server Fingerprint.
        [Parameter(Mandatory=$true,
                   ValueFromPipelineByPropertyName=$true,
                   Position=1)]
        [ValidateScript({ ($_ -split "-|:").count -eq 16 })]
        $Fingerprint
    )

    if ($Fingerprint -match "\-")
    {
       $Fingerprint = $Fingerprint -replace "-", ":"
    }

    Write-Verbose "Value to be used: $SSHHost - $fingerprint"

    [SSH.TrustedKeyMng]::SetKey($SSHHost, $Fingerprint)
}

# .ExternalHelp Posh-SSH.psm1-Help.xml
function Remove-SSHTrustedHostFingerprint
{
    [CmdletBinding()]
    Param
    (
        # IP Address of FQDN of host to add to trusted list.
        [Parameter(Mandatory=$true,
                   ValueFromPipelineByPropertyName=$true,
                   Position=0)]
        $SSHHost,

        # SSH Server Fingerprint.
        [Parameter(Mandatory=$true,
                   ValueFromPipelineByPropertyName=$true,
                   Position=1)]
        [ValidatePattern({ ($_ -split "-|:").count -eq 16 })]
        $Fingerprint
    )

    if ($Fingerprint -match "\-")
    {
       $Fingerprint = $Fingerprint -replace "-", ":"
    }

    [SSH.TrustedKeyMng]::RemoveHostKey($SSHHost, $Fingerprint)
}

# .ExternalHelp Posh-SSH.psm1-Help.xml
function Remove-SSHTrustedHost
{
    [CmdletBinding()]
    Param
    (
        # IP Address of FQDN of host to add to trusted list.
        [Parameter(Mandatory=$true,
                   ValueFromPipelineByPropertyName=$true,
                   Position=0)]
        $SSHHost
    )

    [SSH.TrustedKeyMng]::RemoveHost($SSHHost)
}