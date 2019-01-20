if (Get-Module -ListAvailable -Name platyPS) {
    Write-Host "platyPS is installed." -ForegroundColor Green
    Import-Module platyPS
} 
else {
    Write-Host "Module platyPS is not installed. Can not generate docs" -ForegroundColor Red
    exit
}

Write-Host "Turning markdown documents to External Help files" -ForegroundColor green
New-ExternalHelp .\docs -OutputPath .\Release\en-US -Force -verbose
Write-Host "Updates markdown files" -ForegroundColor Green
Write-Host "Importing release version of module." -ForegroundColor Green
Import-Module .\Release\Posh-SSH.psd1 -verbose -Force
Write-Host "Updating Markdown" -ForegroundColor Green
New-MarkdownHelp -Module Posh-ssh -Force -OutputFolder .\docs
