## This script remains for updates from versions before 0.27.0
$installDirPath = Join-Path $env:LOCALAPPDATA '\Programs\Worms\'
$updateDirPath = $PSScriptRoot + '\*'

function Install-Cli() {
    if(!(test-path $installDirPath))
    {
        New-Item -ItemType Directory -Force -Path $installDirPath | Out-Null
    }

    Copy-item -Force -Recurse $updateDirPath -Destination $installDirPath
}

Install-Cli
