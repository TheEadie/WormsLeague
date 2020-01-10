$installDirPath = Join-Path $env:LOCALAPPDATA '\Programs\Worms\'
$releasesUrl = 'https://api.github.com/repos/TheEadie/WormsLeague/releases'

function Update-Options() {
    $AllProtocols = [System.Net.SecurityProtocolType]'Ssl3,Tls,Tls11,Tls12'
    [System.Net.ServicePointManager]::SecurityProtocol = $AllProtocols
    $releases = Invoke-RestMethod $releasesUrl
    $latestrelease = $releases | where {$_.tag_name -like "cli/v*"} | Sort-Object {$_.published_at} | Select -First 1
    $latestrelease.assets |
        foreach {
            Write-Host $_.name
            Invoke-WebRequest $_.browser_download_url -OutFile (Join-Path $installDirPath $_.name)
        }
}
