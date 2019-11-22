dotnet SchemeConverter/WormsSchemeConverter.dll "Schemes/Uber Coolest Options.txt" "release/Uber.Coolest.Options.$(date +'%Y%m%d').wsc"
dotnet SchemeConverter/WormsSchemeConverter.dll "Schemes/Speed Worms.txt" "release/Speed.Worms.$(date +'%Y%m%d').wsc"

gitversion | Out-File "release/version.json"