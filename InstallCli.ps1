$installDirPath = Join-Path $env:LOCALAPPDATA '\Programs\Worms\.update'
$releasesUrl = 'https://api.github.com/repos/TheEadie/WormsLeague/releases'
$installScript = Join-Path $installDirPath 'Install.ps1'

function Get-Cli() {
    if(!(test-path $installDirPath))
    {
        New-Item -ItemType Directory -Force -Path $installDirPath | Out-Null
    }

    $AllProtocols = [System.Net.SecurityProtocolType]'Ssl3,Tls,Tls11,Tls12'
    [System.Net.ServicePointManager]::SecurityProtocol = $AllProtocols
    $releases = Invoke-RestMethod $releasesUrl
    $latestrelease = $releases | Where-Object {$_.tag_name -like "cli/v*"} | Sort-Object -Descending {$_.published_at} | Select-Object -First 1
    $latestrelease.assets |
        Where-Object {$_.name -like "*.*"} |
        ForEach-Object {
            Write-Host $_.name
            Invoke-WebRequest $_.browser_download_url -OutFile (Join-Path $installDirPath $_.name)
        }
}

Get-Cli
& $installScript