$installDirPath = Join-Path $env:LOCALAPPDATA '\Programs\Worms\'
$releasesUrl = 'https://api.github.com/repos/TheEadie/WormsLeague/releases'

function Install-Cli() {
    if(!(test-path $installDirPath))
    {
        New-Item -ItemType Directory -Force -Path $installDirPath | Out-Null
    }

    $AllProtocols = [System.Net.SecurityProtocolType]'Ssl3,Tls,Tls11,Tls12'
    [System.Net.ServicePointManager]::SecurityProtocol = $AllProtocols
    $releases = Invoke-RestMethod $releasesUrl
    $latestrelease = $releases | Where-Object {$_.tag_name -like "cli/v*"} | Sort-Object -Descending {$_.published_at} | Select-Object -First 1
    $latestrelease.assets |
        Where-Object {$_.name -like "*.exe"} |
        ForEach-Object {
            Write-Host $_.name
            Invoke-WebRequest $_.browser_download_url -OutFile (Join-Path $installDirPath $_.name)
        }
}

function Update-Path() {
    if (!($env:Path -like $installDirPath))
    {
        $env:Path += ";$installDirPath"
        [Environment]::SetEnvironmentVariable("Path", [Environment]::GetEnvironmentVariable("Path", [EnvironmentVariableTarget]::Machine) + ";$installDirPath", [EnvironmentVariableTarget]::User)
    }
}

function Update-PsProfile() {
    if(!(Test-Path $profile))
    {
        New-Item -Path $profile -ItemType "file" -Force | Out-Null
    }

    if ($null -eq (Select-String -Path $profile -Pattern "Update-Worms"))
    {
        Add-Content -Path $profile -Value '$wormsUpdateScriptPath = Join-Path $env:LOCALAPPDATA ''\Programs\Worms\.update\Install.ps1'''
        Add-Content -Path $profile -Value 'Set-Alias -Name Update-Worms -Value $wormsUpdateScriptPath'
    }
}

Install-Cli
Update-Path
Update-PsProfile