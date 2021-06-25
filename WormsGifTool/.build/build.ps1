dotnet test ./GifTool.Tests/GifTool.Tests.csproj
dotnet publish ./GifTool/GifTool.csproj -r win10-x64 --self-contained -c release -o ./release /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true

if (-not (dotnet tool list -g | Select-String "gitversion")) {
    dotnet tool install -g gitversion.tool
}

dotnet-gitversion | Out-File "release/version.json"