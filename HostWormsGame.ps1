$yourSecretSlackToken = 'ENTER YOUR SECRET SLACK TOKEN' # Get from https://api.slack.com/web#authentication
$channel = '#games-worms'

$optionsRepoUrl = 'https://api.github.com/repos/TheEadie/WormsLeague/releases/latest'

$installDirPath = (Get-ItemProperty HKCU:\SOFTWARE\Team17SoftwareLTD\WormsArmageddon).PATH
$wa = Join-Path $installDirPath 'WA.exe'
$schemesDirPath = Join-Path $installDirPath '\User\Schemes\'

function Get-Ip() {
	return Get-NetAdapter -Physical | 
        where {$_.Name -notlike '*VMWare*'} | 
        Get-NetIPAddress -AddressFamily IPv4 |
        select -ExpandProperty IPAddress
}

function Send-Slack() {
	$ip = Get-Ip
	$messageText = "<!here> Hosting at: $ip"
	$message = @{token=$yourSecretSlackToken; channel=$channel; text=$messageText; as_user=$true}
	Invoke-RestMethod -Uri https://slack.com/api/chat.postMessage -Body $message
}

function Update-Options() {
	$latestrelease = invoke-restmethod $optionsRepoUrl
	$optionsFiles = $latestrelease.assets | Where-Object {$_.name -like "*.wsc"}
	foreach ($file in $optionsFiles) { 
		Write-Host $file.name
		curl $file.browser_download_url -OutFile (Join-Path $schemesDirPath $file.name)
	}
}

function Start-Worms() {
    # See http://worms2d.info/Command-line_options
    #
    # The scheme parameter is documented here:
    # http://worms2d.info/WormNET_(Worms_Armageddon)#Channels
    #
    # 'We' means 4 worms per team (don't ask)
	& $wa wa:host?scheme=We
}

Send-Slack
Update-Options
Start-Worms