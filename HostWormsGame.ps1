$yourSecretSlackToken = 'ENTER YOUR SECRET SLACK TOKEN' # Get from https://api.slack.com/web#authentication

$channel = '#games-worms'
$optionsRepoUrl = 'https://api.github.com/repos/TheEadie/WormsLeague/releases/latest'
$installDirPath = 'C:\Program Files (x86)\Steam\steamapps\common\Worms Armageddon\'
$schemesDirPath = Join-Path $installDirPath '\User\Schemes\'

function Send-Slack() {
	$ip = (ipconfig | grep "10.120" -m 1).Substring(39)
	$messageText = "Hosting at: $ip"
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
	Start-Process (Join-Path $installDirPath "WA.exe") "/host"
}

Send-Slack
Update-Options
Start-Worms