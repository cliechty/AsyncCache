#tool "nuget:?package=GitVersion.CommandLine"

var target = Argument("Target", "Build");
var configuration = Argument("Configuration", "Release");

var solution = "AsyncCache.sln";

Task("Restore")
	.Does(() =>
	{
		NuGetRestore(solution);
	});

Task("UpdateAssemblyInfo")
	.Does(() =>
	{
		GitVersion(new GitVersionSettings {
			UpdateAssemblyInfo = true,
			EnvironmentVariables = new Dictionary<string, string> 
			{ 
				{ "assembly-informational-format", "{MajorMinorPatch}+{BranchName}" }
			}
		});
	});

Task("Clean")
	.Does(() =>
	{
		CleanDirectories("./AsyncCache/bin");
		CleanDirectories("./AsyncCache/obj");
	});

Task("Compile")
	.IsDependentOn("Restore")
	.IsDependentOn("Clean")
	.IsDependentOn("UpdateAssemblyInfo")
	.Does(() =>
	{
		MSBuild(solution,
				settings => settings.SetConfiguration(configuration)
									.WithTarget("Build"));
	});

Task("Test")
	.IsDependentOn("Compile")
	.Does(() =>
	{
		MSTest("./AsyncCache.Specs/bin/Release/AsyncCache.Specs.dll");
	});

Task("Package")
	.IsDependentOn("Test")
	.Does(() =>
	{
		NuGetPack("./AsyncCache/AsyncCache.csproj", new NuGetPackSettings 
		{
			Properties = new Dictionary<string, string> 
			{
				{ "Configuration", configuration }
			},
			OutputDirectory = "./"
		});
	});

Task("Build")
	.IsDependentOn("Test")
	.Does(() => {});


RunTarget(target);
