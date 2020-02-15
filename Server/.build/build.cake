#module nuget:?package=Cake.DotNetTool.Module&Version=0.4.0
#tool dotnet:?package=GitVersion.Tool&Version=5.1.3
#addin nuget:?package=Newtonsoft.Json&version=12.0.3
#addin "Cake.Docker&version=0.11.0"

using Newtonsoft.Json;

// Constants
const string projectPath = "../src/Worms.Gateway/Worms.Gateway.csproj";
const string solutionPath = "../";
const string artifactPath = "../.artifacts/";

// Command line arguments
var target = Argument("target", "Docker-Build");

// State
var versionInfo = new GitVersion();

Task("CalculateVersion")
  .Does(() =>
{
    versionInfo = GitVersion(new GitVersionSettings 
      { 
        WorkingDirectory = "../",
        ToolPath = Context.Tools.Resolve("dotnet-gitversion")
      });

    Information($"Version - {versionInfo.MajorMinorPatch}");
});

Task("Publish")
  .IsDependentOn("CalculateVersion")
  .Does(() =>
{
    var version = versionInfo.MajorMinorPatch;
    Information($"Publishing version - {version}");

    var linuxSettings = new DotNetCorePublishSettings
    {
        Configuration = "Release",
        OutputDirectory = artifactPath,
        Runtime = "linux-x64",
        SelfContained = true,
        ArgumentCustomization = args=>args.Append($"/p:Version={version}")
    };

    CleanDirectory(artifactPath);

    var versionInfoFilePath = System.IO.Path.Combine(artifactPath, "version.json");
    var versionJson = JsonConvert.SerializeObject(versionInfo, Formatting.Indented);
    System.IO.File.WriteAllText(versionInfoFilePath, versionJson);

    DotNetCorePublish(projectPath, linuxSettings);
});

Task("Docker-Build")
  .IsDependentOn("Publish")
  .Does(() =>
{
    var settings = new DockerImageBuildSettings
    {
        WorkingDirectory = "../",
        File = ".build/dockerfile",
        Tag= new string[]
            {
              "worms-gateway:latest",
              $"worms-gateway:{versionInfo.Major}",
              $"worms-gateway:{versionInfo.Major}.{versionInfo.Minor}",
              $"worms-gateway:{versionInfo.Major}.{versionInfo.Minor}.{versionInfo.Patch}"
            }
    };

    DockerBuild(settings, ".");
});

RunTarget(target);