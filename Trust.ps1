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
        [ValidatePattern({ ($_ -split "-|:").count -eq 16 })]
        $FingerPrint
    )

    if ($FingerPrint -match "\-")
    {
       $FingerPrint = $FingerPrint -replace "-", ":"
    }

    [SSH.TrustedKeyMng]::SetKey($SSHHost, $FingerPrint)
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
        $FingerPrint
    )

    if ($FingerPrint -match "\-")
    {
       $FingerPrint = $FingerPrint -replace "-", ":"
    }

    [SSH.TrustedKeyMng]::RemoveHostKey($SSHHost, $FingerPrint)
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