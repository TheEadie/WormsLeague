$yourSecretSlackToken = 'ENTER YOUR SECRET SLACK TOKEN' # Get from https://api.slack.com/web#authentication
$channel = '#games-worms'

$optionsRepoUrl = 'https://api.github.com/repos/TheEadie/WormsLeague/releases/latest'

$installDirPath = (Get-ItemProperty HKCU:\SOFTWARE\Team17SoftwareLTD\WormsArmageddon).PATH
$wa = Join-Path $installDirPath 'WA.exe'
$schemesDirPath = Join-Path $installDirPath '\User\Schemes\'

#Gets the ip that'd actually be used rather than random virtual netadapters.
function Get-Ip() {
	$pingResponse = (ping -4 -n 1 $env:computername)[1]
	$ipStart = $pingResponse.IndexOf("[") + 1 # Hopefully the machine name doesn't contain "[" or "]"
	$ipLength = $pingResponse.IndexOf("]") - $ipStart
	return $pingResponse.SubString($ipStart, $ipLength)
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
	Start-Process (Join-Path $installDirPath "WA.exe") "/host"
}

Send-Slack
Update-Options
Start-Worms