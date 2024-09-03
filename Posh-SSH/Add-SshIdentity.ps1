function Add-SshIdentity {
    [CmdletBinding(DefaultParameterSetName='FileWithoutPassphrase')]
    param(
        [Parameter(Mandatory=$true, ParameterSetName='FileWithoutPassphrase')]
        [Parameter(Mandatory=$true, ParameterSetName='FileWithPassphrase')]
        [string]$KeyPath,

        [Parameter(Mandatory=$true, ParameterSetName='StringWithoutPassphrase')]
        [Parameter(Mandatory=$true, ParameterSetName='StringWithPassphrase')]
        [string]$KeyString,

        [Parameter(Mandatory=$true, ParameterSetName='FileWithPassphrase')]
        [Parameter(Mandatory=$true, ParameterSetName='StringWithPassphrase')]
        [SecureString]$Passphrase,

        [Parameter(Mandatory=$true)]
        [ValidateSet('SshAgent', 'Pageant')]
        [string]$AgentType
    )

    begin {
    }

    process {
        try {
            # Create the appropriate agent
            $agent = switch ($AgentType) {
                'SshAgent' { [SshNet.Agent.SshAgent]::new() }
                'Pageant' { [SshNet.Agent.Pageant]::new() }
            }

            # Create the PrivateKeyFile object
            $privateKeyFile = if ($PSCmdlet.ParameterSetName -like 'File*') {
                if ($PSCmdlet.ParameterSetName -eq 'FileWithPassphrase') {
                    $passphraseString = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto([System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($Passphrase))
                    [Renci.SshNet.PrivateKeyFile]::new($KeyPath, $passphraseString)
                } else {
                    [Renci.SshNet.PrivateKeyFile]::new($KeyPath)
                }
            } else {
                $keyStream = [System.IO.MemoryStream]::new([System.Text.Encoding]::UTF8.GetBytes($KeyString))
                if ($PSCmdlet.ParameterSetName -eq 'StringWithPassphrase') {
                    $passphraseString = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto([System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($Passphrase))
                    [Renci.SshNet.PrivateKeyFile]::new($keyStream, $passphraseString)
                } else {
                    [Renci.SshNet.PrivateKeyFile]::new($keyStream)
                }
            }

            # Add the identity to the agent
            $agent.AddIdentity($privateKeyFile)

            Write-Verbose "SSH key successfully added to $AgentType"
        }
        catch {
            Write-Error "An error occurred: $_"
        }
        finally {
            if ($keyStream) {
                $keyStream.Dispose()
            }
        }
    }
}