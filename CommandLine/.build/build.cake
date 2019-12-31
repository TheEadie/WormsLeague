var target = Argument("target", "Build");

Task("Build")
  .Does(() =>
{
  DotNetCoreBuild(@"../src/Worms.csproj");
});

RunTarget(target);