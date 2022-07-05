$installDirPath = Join-Path $env:LOCALAPPDATA '\Programs\Worms\'
$updateDirPath = $PSScriptRoot + '\*'
$wormsUpdateScriptPath = Join-Path $env:LOCALAPPDATA '\Programs\Worms\.update\Install.ps1'

function Install-Cli() {
    if(!(test-path $installDirPath))
    {
        New-Item -ItemType Directory -Force -Path $installDirPath | Out-Null
    }

    Copy-item -Force -Recurse $updateDirPath -Destination $installDirPath
}

function Update-Path() {
    if (!($env:Path -like $installDirPath))
    {
        $env:Path += ";$installDirPath"
        [Environment]::SetEnvironmentVariable("Path", [Environment]::GetEnvironmentVariable("Path", [EnvironmentVariableTarget]::User) + ";$installDirPath", [EnvironmentVariableTarget]::User)
    }
}

function Update-PsProfile() {
    if(!(Test-Path $profile))
    {
        New-Item -Path $profile -ItemType "file" -Force | Out-Null
    }

    if ($null -eq (Select-String -Path $profile -Pattern "Install-WormsCli"))
    {
        Add-Content -Path $profile -Value "`r`nSet-Alias -Name Install-WormsCli -Value $wormsUpdateScriptPath"
        Set-Alias -Name Install-WormsCli -Value $wormsUpdateScriptPath -Scope Global
    }
}

Install-Cli
Update-Path
Update-PsProfile