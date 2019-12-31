const string projectPath = "../src/worms.csproj";
const string artifactPath = "../.artifacts/";

var target = Argument("target", "Build");

Task("Build")
  .Does(() =>
{
    DotNetCoreBuild(projectPath);
});

Task("Publish")
  .Does(() =>
{
    var winSettings = new DotNetCorePublishSettings
    {
        Configuration = "Release",
        OutputDirectory = artifactPath,
        Runtime = "win-x64",
        SelfContained = true,
        ArgumentCustomization = args=>args.Append($"/p:PublishSingleFile=true")
    };

    var linuxSettings = new DotNetCorePublishSettings
    {
        Configuration = "Release",
        OutputDirectory = artifactPath,
        Runtime = "linux-x64",
        SelfContained = true,
        ArgumentCustomization = args=>args.Append($"/p:PublishSingleFile=true")
    };

    CleanDirectory(artifactPath);
    DotNetCorePublish(projectPath, winSettings);
    DotNetCorePublish(projectPath, linuxSettings);
});

RunTarget(target);