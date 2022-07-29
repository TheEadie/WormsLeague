. ".build/logging.ps1"
. ".build/version.ps1"

# Config
$nextVersion = [System.Version]::Parse("1.1")
$tag = "giftool/v"

$nextVersion = Get-NextVersion $tag $nextVersion

Write-Heading "Building $nextVersion"

dotnet test ./GifTool.Tests/GifTool.Tests.csproj
dotnet publish ./GifTool/GifTool.csproj -r win10-x64 --self-contained -c release -o ./.artifacts /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true /p:Version=$nextVersion /p:DebugType=none

$jsonBase = @{}
$jsonBase.Add("MajorMinorPatch", $nextVersion.ToString())
$jsonBase | ConvertTo-Json -Depth 10 | Out-File ".artifacts/version.json"