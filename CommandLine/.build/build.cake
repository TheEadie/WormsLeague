#module nuget:?package=Cake.DotNetTool.Module&Version=0.4.0
#tool dotnet:?package=GitVersion.Tool&Version=5.1.3
#addin nuget:?package=Newtonsoft.Json&version=12.0.3

using Newtonsoft.Json;

// Constants
const string wormsCliPath = "../src/Worms.Cli/worms.csproj";
const string updateCliPath = "../src/Update.Cli/update.csproj";
const string artifactPath = "../.artifacts/";

// Command line arguments
var target = Argument("target", "Publish");

// State
var versionInfo = new GitVersion();

Task("CalculateVersion")
  .Does(() =>
{
    versionInfo = GitVersion( new GitVersionSettings { WorkingDirectory = "../", ToolPath = Context.Tools.Resolve("dotnet-gitversion") } );

    Information($"Version - {versionInfo.MajorMinorPatch}");
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
        ArgumentCustomization = args=>args.Append($"/p:PublishSingleFile=true /p:Version={version} /p:PublishTrimmed=true")
    };

    var linuxSettings = new DotNetCorePublishSettings
    {
        Configuration = "Release",
        OutputDirectory = artifactPath,
        Runtime = "linux-x64",
        SelfContained = true,
        ArgumentCustomization = args=>args.Append($"/p:PublishSingleFile=true /p:Version={version} /p:PublishTrimmed=true")
    };

    CleanDirectory(artifactPath);

    var versionInfoFilePath = System.IO.Path.Combine(artifactPath, "version.json");
    var versionJson = JsonConvert.SerializeObject(versionInfo, Formatting.Indented);
    System.IO.File.WriteAllText(versionInfoFilePath, versionJson);

    DotNetCorePublish(wormsCliPath, winSettings);
    DotNetCorePublish(updateCliPath, winSettings);
    DotNetCorePublish(wormsCliPath, linuxSettings);
    DotNetCorePublish(updateCliPath, linuxSettings);
});

RunTarget(target);