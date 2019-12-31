#tool "nuget:?package=GitVersion.CommandLine&version=5.1.3"

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
    CleanDirectory(artifactPath);
    var versionInfo = GitVersion();

    var version = versionInfo.MajorMinorPatch;

    Information($"Publishing version - {version}");

    var winSettings = new DotNetCorePublishSettings
    {
        Configuration = "Release",
        OutputDirectory = artifactPath,
        Runtime = "win-x64",
        SelfContained = true,
        ArgumentCustomization = args=>args.Append($"/p:PublishSingleFile=true /p:Version={version}")
    };

    var linuxSettings = new DotNetCorePublishSettings
    {
        Configuration = "Release",
        OutputDirectory = artifactPath,
        Runtime = "linux-x64",
        SelfContained = true,
        ArgumentCustomization = args=>args.Append($"/p:PublishSingleFile=true /p:Version={version}")
    };

    DotNetCorePublish(projectPath, winSettings);
    DotNetCorePublish(projectPath, linuxSettings);
});

RunTarget(target);