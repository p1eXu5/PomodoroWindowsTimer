#r "nuget: Fake.DotNet.Cli, 6.1.3"
#r "nuget: Fake.IO.FileSystem, 6.1.3"
#r "nuget: Fake.Core.Target, 6.1.3"
#r "nuget: MSBuild.StructuredLogger, 2.2.386"

open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators

// Boilerplate
System.Environment.GetCommandLineArgs()
|> Array.skip 2 // skip fsi.exe; build.fsx
|> Array.toList
|> Fake.Core.Context.FakeExecutionContext.Create false __SOURCE_FILE__
|> Fake.Core.Context.RuntimeContext.Fake
|> Fake.Core.Context.setExecutionContext

// Define the project and output directories
let project = "./src/PomodoroWindowsTimer.WpfClient/PomodoroWindowsTimer.WpfClient.csproj"
let outputDir = "./publish/win-x64"

// Define the target framework and runtime
let framework = "net8.0-windows"
let runtime = "win-x64"

Target.create "Clean" (fun _ ->
    !! "src/**/bin" ++ "src/**/obj"
    |> Shell.cleanDirs)

// Restore target
Target.create "Restore" (fun _ ->
    DotNet.restore
        // (fun o ->
        //     { o with
        //         MSBuildParams = { o.MSBuildParams with DisableInternalBinLog = true }
        // })
        id
        project
)

// Build target
Target.create "Build" (fun _ ->
    DotNet.build (fun opts ->
        { opts with
            Configuration = DotNet.BuildConfiguration.Release
            NoRestore = true
        }) project
)

// Test target
Target.create "Test" (fun _ ->
    DotNet.test (fun opts ->
        { opts with
            Configuration = DotNet.BuildConfiguration.Release
        }) "."
)

// GetVersion target
Target.create "GetVersion" (fun _ ->
    let versionString = "???" // getVersionString()
    Trace.log $"Version string: {versionString}"
    // Store the version string in the context for later use
    FakeVar.set "VersionString" "foo"
    //Context. "VersionString" versionString
)

// Publish target
Target.create "Publish" (fun _ ->
    let versionString = FakeVar.get "VersionString" |> Option.defaultValue "v0.0.0"
    Trace.log $"Publishing version: {versionString}"

    // let outputDir = System.IO.Path.Combine(baseOutputDir, versionString)
    // 
    // DotNet.publish (fun opts ->
    //     { opts with
    //         Configuration = DotNet.BuildConfiguration.Release
    //         Framework = Some framework
    //         Runtime = Some runtime
    //         OutputPath = Some outputDir
    //         SelfContained = Some true
    //     }) project
)



Target.create "All" ignore

"Clean"
    ==> "Restore"
    ==> "Build"
    ==> "Test"
    // ==> "GetVersion"
    ==> "Publish"

Target.runOrDefault "Test"
