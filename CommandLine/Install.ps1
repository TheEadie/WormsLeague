$installDirPath = Join-Path $env:LOCALAPPDATA '\Programs\Worms\'
$updateDirPath = $PSScriptRoot + '\*'

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
        [Environment]::SetEnvironmentVariable("Path", [Environment]::GetEnvironmentVariable("Path", [EnvironmentVariableTarget]::Machine) + ";$installDirPath", [EnvironmentVariableTarget]::User)
    }
}

function Update-PsProfile() {
    if(!(Test-Path $profile))
    {
        New-Item -Path $profile -ItemType "file" -Force | Out-Null
    }

    if ($null -eq (Select-String -Path $profile -Pattern "Install-WormsCli"))
    {
        $wormsUpdateScriptPath = Join-Path $env:LOCALAPPDATA '\Programs\Worms\.update\Install.ps1'
        Add-Content -Path $profile -Value "Set-Alias -Name Install-WormsCli -Value $wormsUpdateScriptPath"
    }
}

Install-Cli
Update-Path
Update-PsProfile