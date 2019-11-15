dotnet test ./GifTool.Tests/GifTool.Tests.csproj
dotnet publish ./GifTool/GifTool.csproj -r win10-x64 --self-contained -c release -o ./release /p:PublishSingleFile=true

gitversion | Out-File "release/version.json"