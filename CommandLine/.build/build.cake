#tool "nuget:?package=GitVersion.CommandLine&version=5.1.3"

// Constants
const string projectPath = "../src/worms.csproj";
const string artifactPath = "../.artifacts/";

// Command line arguments
var target = Argument("target", "Publish");

// State
var versionInfo = new GitVersion();

Task("CalculateVersion")
  .Does(() =>
{
    versionInfo = GitVersion();

    Information($"Version - {version}");
});

Task("Publish")
  .IsDependentOn("CalculateVersion")
  .Does(() =>
{
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

    CleanDirectory(artifactPath);
    DotNetCorePublish(projectPath, winSettings);
    DotNetCorePublish(projectPath, linuxSettings);
});

RunTarget(target);