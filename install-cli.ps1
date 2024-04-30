$installDirPath = Join-Path $env:LOCALAPPDATA '\Programs\Worms'
$downloadUrl = 'https://worms.davideadie.dev/api/v1/files/cli/windows'

function Get-Cli() {

    if(!(test-path $installDirPath))
    {
        New-Item -ItemType Directory -Force -Path $installDirPath | Out-Null
    }

    $zipPath = Join-Path $installDirPath 'file.zip'

    $AllProtocols = [System.Net.SecurityProtocolType]'Tls12'
    [System.Net.ServicePointManager]::SecurityProtocol = $AllProtocols
    Invoke-WebRequest -Uri $downloadUrl -OutFile $zipPath
    Expand-Archive -Path $zipPath -DestinationPath $installDirPath
    Remove-Item -Path $zipPath
}

function Update-Path() {
    if (!($env:Path -like $installDirPath))
    {
        $env:Path += ";$installDirPath"
        [Environment]::SetEnvironmentVariable("Path", [Environment]::GetEnvironmentVariable("Path", [EnvironmentVariableTarget]::User) + ";$installDirPath", [EnvironmentVariableTarget]::User)
    }
}

Get-Cli
Update-Path
