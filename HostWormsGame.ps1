param(
	$yourSecretSlackToken, # Get from https://api.slack.com/web#authentication
	$channel = '#games-worms',
	$optionsRepoUrl = 'https://api.github.com/repos/TheEadie/WormsLeague/releases/latest'
)

$installDirPath = (Get-ItemProperty HKCU:\SOFTWARE\Team17SoftwareLTD\WormsArmageddon).PATH

function Get-Ip() {
    return Get-NetAdapter -Physical | 
        where -Property Status -EQ Up | 
        Sort-Object -Property Speed -Descending |
        Get-NetIPAddress -AddressFamily IPv4 |
        select -ExpandProperty IPAddress -First 1 
}

function Send-Slack() {
    $ip = Get-Ip
    $messageText = "<!here> Hosting at: wa://$ip"
    $message = @{token=$yourSecretSlackToken; channel=$channel; text=$messageText; as_user=$true}
    Invoke-RestMethod -Uri https://slack.com/api/chat.postMessage -Body $message
}

function Update-Options() {
    $schemesDirPath = Join-Path $installDirPath '\User\Schemes\'
    $latestrelease = Invoke-RestMethod $optionsRepoUrl
    $latestrelease.assets | 
        where {$_.name -like "*.wsc"} |
        foreach {
            Write-Host $_.name
            Invoke-WebRequest $_.browser_download_url -OutFile (Join-Path $schemesDirPath $_.name)
        }
}

function Start-Worms() {
    $wa = Join-Path $installDirPath 'WA.exe'
    & $wa 'wa://'
}

Send-Slack
Update-Options
Start-Worms
