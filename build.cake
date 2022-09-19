var target = Argument("target", "Package");

public record BuildData(
    DirectoryPath Src,
    string Version,
    DirectoryPath Artifact
    )
    {
        public DotNetMSBuildSettings MSBuildSettings { get; } = new DotNetMSBuildSettings {
            Version = Version
        };
    }


Setup(
    context => new BuildData(
        "src",
        DateTime.UtcNow.ToString("yyyy.MM.dd"),
        "./artfact"
        )
);

Task("Clean")
    .Does<BuildData>((ctx, data)=>{
        CleanDirectory(data.Artifact);
});

Task("Restore")
    .IsDependentOn("Clean")
    .Does<BuildData>((ctx, data)=>{
        DotNetRestore(data.Src.FullPath, new DotNetRestoreSettings {
    });
    });
    
    
    
    

Task("Build")
    .IsDependentOn("Restore")
      .Does<BuildData>((ctx, data)=>{
        DotNetBuild(data.Src.FullPath,
        new DotNetBuildSettings{
            NoRestore = true,
            MSBuildSettings = data.MSBuildSettings 
    });
 });
Task("Test")
    .IsDependentOn("Build")
      .Does<BuildData>((ctx, data)=>{
        DotNetTest(data.Src.FullPath,
        new DotNetTestSettings {
            NoBuild = true,
            NoRestore = true
    });
 });

Task("Package")
    .IsDependentOn("Test")
      .Does<BuildData>((ctx, data)=>{
        DotNetPack(data.Src.FullPath, new DotNetPackSettings{
            NoRestore = true,
            MSBuildSettings = data.MSBuildSettings,
            OutputDirectory= data.Artifact
    });
 });


Task("Publish")
     .IsDependentOn("Package")
     .Does<BuildData>(async (ctx, data)=>{
        await GitHubActions.Commands.UploadArtifact(
            data.Artifact,
            $"Swetugg{ctx.Environment.Platform.Family}"
            );
 });

 Task("GitHubActions")
    .IsDependentOn("Publish");

RunTarget(target);