#module nuget:?package=Cake.DotNetTool.Module&Version=0.4.0
#tool dotnet:?package=GitVersion.Tool&Version=5.1.3
#addin nuget:?package=Newtonsoft.Json&version=12.0.3

using Newtonsoft.Json;

// Constants
const string projectPath = "../src/worms.csproj";
const string solutionPath = "../";
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

Task("Publish-Windows")
  .IsDependentOn("CalculateVersion")
  .Does(() =>
{
    var runtime = "win-x64";

    var version = versionInfo.MajorMinorPatch;
    Information($"Publishing version - {version}-{runtime}");
    Publish(runtime);

    // Windows install scripts
    var runtimeArtifactPath = System.IO.Path.Combine(artifactPath, runtime);
    CopyFiles($"{solutionPath}*.ps1", runtimeArtifactPath);
});

Task("Publish-Linux")
  .IsDependentOn("CalculateVersion")
  .Does(() =>
{
    var runtime = "linux-x64";

    var version = versionInfo.MajorMinorPatch;
    Information($"Publishing version - {version}-{runtime}");
    Publish(runtime);
});

Task("Publish")
  .IsDependentOn("Publish-Windows")
  .IsDependentOn("Publish-Linux");

public void Publish(string runtime)
{
    var runtimeArtifactPath = System.IO.Path.Combine(artifactPath, runtime);

    var settings = new DotNetCorePublishSettings
    {
        Configuration = "Release",
        OutputDirectory = runtimeArtifactPath,
        Runtime = runtime,
        SelfContained = true,
        ArgumentCustomization = args=>args.Append($"/p:PublishSingleFile=true /p:Version={versionInfo.MajorMinorPatch} /p:PublishTrimmed=true")
    };

    CleanDirectory(runtimeArtifactPath);

    var versionInfoFilePath = System.IO.Path.Combine(runtimeArtifactPath, "version.json");
    var versionJson = JsonConvert.SerializeObject(versionInfo, Formatting.Indented);
    System.IO.File.WriteAllText(versionInfoFilePath, versionJson);

    DotNetCorePublish(projectPath, settings);
}

RunTarget(target);