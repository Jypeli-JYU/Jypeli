// Original buildscript by github.com/Jjagg

#tool nuget:?package=vswhere&version=2.6.7

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("build-target", "Default");
var templateversion = "1.7.0";
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

MSBuildSettings msPackSettings, mdPackSettings;
DotNetCoreBuildSettings dnBuildSettings;
DotNetCorePackSettings dnPackSettings;

private void PackMSBuild(string filePath)
{
    MSBuild(filePath, msPackSettings);
}

private void PackDotnet(string filePath)
{
    DotNetCorePack(filePath, dnPackSettings);
}

private bool GetMSBuildWith(string requires)
{
    if (IsRunningOnWindows())
    {
        DirectoryPath vsLatest = VSWhereLatest(new VSWhereLatestSettings { Requires = requires });

        if (vsLatest != null)
        {
            var files = GetFiles(vsLatest.FullPath + "/**/MSBuild.exe");
            if (files.Any())
            {
                msPackSettings.ToolPath = files.First();
                return true;
            }
        }
    }

    return false;
}

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Prep")
    .Does(() =>
{
    msPackSettings = new MSBuildSettings();
    msPackSettings.Verbosity = Verbosity.Minimal;
    msPackSettings.Configuration = configuration;
    msPackSettings.Restore = true;
    msPackSettings.WithTarget("Pack");

    mdPackSettings = new MSBuildSettings();
    mdPackSettings.Verbosity = Verbosity.Minimal;
    mdPackSettings.Configuration = configuration;
    mdPackSettings.WithTarget("PackageAddin");

    dnBuildSettings = new DotNetCoreBuildSettings
    {
        Configuration = configuration,
    };
    dnPackSettings = new DotNetCorePackSettings();
    dnPackSettings.Verbosity = DotNetCoreVerbosity.Minimal;
    dnPackSettings.Configuration = configuration;
});

Task("BuildJypeli")
    .IsDependentOn("Prep")
    .Does(() =>
{
    var path = "../Jypeli/Jypeli.csproj";
    DotNetCoreRestore(path);
    DotNetCoreBuild(path);
    PackDotnet(path);
});

Task("BuildPhysics2d")
    .IsDependentOn("BuildJypeli")
    .Does(() =>
{
    var path = "../Physics2d/Jypeli.Physics2d.csproj";
    DotNetCoreRestore(path);
    DotNetCoreBuild(path);
    PackDotnet(path);
});

Task("BuildFarseer")
    .IsDependentOn("BuildJypeli")
    .Does(() =>
{
    var path = "../FarseerPhysics/FarseerPhysics.csproj";
    DotNetCoreRestore(path);
    DotNetCoreBuild(path);
    PackDotnet(path);
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
{
    DotNetCoreTest("../Jypeli.NET.sln", new DotNetCoreTestSettings
    {
        Configuration = configuration
    });
});

Task("PackDotNetTemplates")
    .IsDependentOn("Prep")
    .Does(() =>
{
    DotNetCoreRestore("../projektimallit/Jypeli.Templates/Jypeli.Templates.csproj");
    PackDotnet("../projektimallit/Jypeli.Templates/Jypeli.Templates.csproj");
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Build")
    .IsDependentOn("BuildJypeli")
    .IsDependentOn("BuildFarseer");

Task("Pack")
    .IsDependentOn("PackDotNetTemplates");

Task("All")
    .IsDependentOn("Build")
    .IsDependentOn("Test")
    .IsDependentOn("Pack");

Task("Default")
    .IsDependentOn("All");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
