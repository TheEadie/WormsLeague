$versionInfo = gitversion
$versionInfo | Out-File "release/version.json"

$version = $versionInfo.MajorMinorPatch

dotnet SchemeConverter/WormsSchemeConverter.dll "Schemes/Uber Coolest Options.txt" "release/Uber.Coolest.Options.$version.wsc"
dotnet SchemeConverter/WormsSchemeConverter.dll "Schemes/Speed Worms.txt" "release/Speed.Worms.$version.wsc"