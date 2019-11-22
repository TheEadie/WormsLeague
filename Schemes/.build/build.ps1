$versionInfo = gitversion | Out-String
$versionInfo | Out-File "release/version.json"

$json = $versionInfo | ConvertFrom-Json
$version = $json.MajorMinorPatch

dotnet ../SchemeConverter/WormsSchemeConverter.dll "Uber Coolest Options.txt" "release/Uber.Coolest.Options.$version.wsc"
dotnet ../SchemeConverter/WormsSchemeConverter.dll "Speed Worms.txt" "release/Speed.Worms.$version.wsc"
dotnet ../SchemeConverter/WormsSchemeConverter.dll "Christmas 2019.txt" "release/Christmas.$version.wsc"
