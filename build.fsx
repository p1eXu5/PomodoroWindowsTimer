open System.IO.Compression

#r "nuget: Fake.Api.GitHub, 6.1.3"
#r "nuget: Fake.DotNet.Cli, 6.1.3"
#r "nuget: Fake.IO.FileSystem, 6.1.3"
#r "nuget: Fake.Core.Target, 6.1.3"
#r "nuget: Fake.Core.Vault, 6.1.3"
#r "nuget: Fake.Core.ReleaseNotes, 6.1.3"
#r "nuget: Fake.BuildServer.GitHubActions, 6.1.3"
#r "nuget: Fake.IO.Zip, 6.1.3"
#r "nuget: Fake.Tools.Git, 6.1.3"
#r "nuget: MSBuild.StructuredLogger, 2.2.386" // MSBuild log version fix
#r "nuget: System.Formats.Asn1, 9.0.0" // vulnerabilities

#load "./utils/scripts/project_file_versions.fsx"

open System.IO
open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators
open Project_file_versions
open Fake.BuildServer
open Fake.Tools
open Fake.Api

// -------------------
//   Bootstrap Fake
// -------------------
// To run script without fake.exe (no need multiple .NET sdk versions) - https://fake.build/guide/fake-debugging.html#Run-script-without-fake-exe-via-fsi
Fake.Core.Context.setExecutionContextFromCommandLineArgs __SOURCE_FILE__

// ------------------
//    Properties
// ------------------
let [<Literal>] project = "./src/PomodoroWindowsTimer.WpfClient/PomodoroWindowsTimer.WpfClient.csproj"

let [<Literal>] framework = "net8.0-windows"
let [<Literal>] runtime = "win-x64"

let [<Literal>] releaseRootDir = "./release"
let collectedArtifactsDir = releaseRootDir </> "artifacts"

/// releaseRootDir/publish
let publishRootDir = releaseRootDir </> "publish"

/// releaseRootDir/upload
let uploadRootDir = releaseRootDir </> "upload"

let [<Literal>] versionStringKey = "VersionString"

let release = ReleaseNotes.load "RELEASE_NOTES.md"

let isGitHubActions = Environment.hasEnvironVar "GITHUB_ACTIONS"

// ------------------
//      Secrets
// ------------------
let mutable secrets = []

let vault = Vault.fromFakeEnvironmentVariable()

let getVarOrDefaultFromVault name defaultValue =
    match vault.TryGet name with
    | Some v -> v
    | None -> Environment.environVarOrDefault name defaultValue

let releaseSecret replacement name =
    let secret =
        lazy
            let env =
                match getVarOrDefaultFromVault name "default_unset" with
                | "default_unset" -> failwithf "variable '%s' is not set" name
                | s -> s
            TraceSecrets.register replacement env
            env

    secrets <- secret :: secrets
    secret

let githubReleaseUser = getVarOrDefaultFromVault "GITHUB_ACTOR" "p1eXu5"
let gitName = getVarOrDefaultFromVault "REPOSITORY_NAME_GITHUB" "PomodoroWindowsTimer"

let githubToken = releaseSecret "<githubtoken>" "GITHUB_TOKEN"
// let githubNugetToken = releaseSecret "<githubtoken>" "GITHUB_NUGET_TOKEN"

do Environment.setEnvironVar "COREHOST_TRACE" "0"

// https://fake.build/guide/buildserver.html
BuildServer.install [
    GitHubActions.Installer
]

// CoreTracing.ensureConsoleListener () // duplicates logs on ci


// ------------------
//    Privates
// ------------------
/// publishRootDir/versionString/runtime
let private publishDir versionString =
    publishRootDir </> versionString </> runtime

let private uploadDir versionString =
    uploadRootDir </> versionString

let private zipFileName versionString =
    "pwt_" + (runtime.Replace("-", "_")) + versionString + ".zip"

// ------------------
//    Targets
// ------------------
Target.create "CheckReleaseSecrets" (fun p ->
    // Environment.environVars ()
    // |> Seq.sortBy fst
    // |> Seq.iter (fun (k, v) -> Trace.log (sprintf "%s: %s" k v))
    // 
    // Trace.log (sprintf "TargetParamer.Context.Arguments: %A" (p.Context.Arguments))

    for secret in secrets do
        secret.Force() |> ignore
)

Target.create "SetupNuGet" (fun _ ->
    let token = githubToken.Value
    let nugetConfig = """
    <configuration>
      <packageSources>
        <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
        <add key="github" value="https://nuget.pkg.github.com/your-github-username/index.json" />
      </packageSources>
      <packageSourceCredentials>
        <github>
          <add key="Username" value="your-github-username" />
          <add key="ClearTextPassword" value="{0}" />
        </github>
      </packageSourceCredentials>
    </configuration>
    """
    let configContent = System.String.Format(nugetConfig, token)
    System.IO.File.WriteAllText("NuGet.Config", configContent)
    Trace.log "NuGet source configured."
)

Target.create "GetVersion" (fun _ ->
    let versionString = getCurrentVersion project |> Option.defaultValue release.AssemblyVersion

    if versionString <> release.AssemblyVersion then
        failwith (sprintf "Release notes for version %s has not been found" versionString)

    Trace.log $"Version string: {versionString}"
    // Store the version string in the context for later use
    FakeVar.set versionStringKey versionString
)

Target.create "Clean" (fun _ ->
    let versionString = FakeVar.get versionStringKey |> Option.get
    let publishDir = versionString |> publishDir
    let uploadDir = versionString |> uploadDir

    !! "src/**/bin" ++ "src/**/obj" ++ publishDir ++ uploadDir
    |> Shell.cleanDirs
)

// Restore target
Target.create "Restore" (fun _ ->
    DotNet.restore
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

// Build target
Target.create "Build_Debug" (fun _ ->
    DotNet.build (fun opts ->
        { opts with
            Configuration = DotNet.BuildConfiguration.Debug
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


// Publish target
Target.create "Publish" (fun _ ->
    let versionString = FakeVar.get versionStringKey |> Option.get
    Trace.log $"Publishing version: {versionString}"

    let publishFolder = versionString |> publishDir
    
    DotNet.publish (fun opts ->
        { opts with
            Configuration = DotNet.BuildConfiguration.Release
            Framework = Some framework
            Runtime = Some runtime
            OutputPath = Some publishFolder
            SelfContained = Some true
            VersionSuffix = Some versionString
            MSBuildParams =
                { MSBuild.CliArguments.Create ()
                    with
                        Properties = [ 
                            "PublishSingleFile", "true"
                            "PublishReadyToRun", "true"
                            "DebugType", "embedded"
                        ]
                }
        }) project
)

(*
    .publish
        - 0.0.0
            - win_x64

    .upload
        pwt_win_x64_0.0.0.zip
*)

Target.create "Compress" (fun _ ->
    let versionString = FakeVar.get versionStringKey |> Option.get
    let publishDir = versionString |> publishDir
    let uploadDir = versionString |> uploadDir
    let zipFileName = versionString |> zipFileName

    let files = !!(sprintf "%s/**" publishDir)

    Trace.log uploadDir
    Trace.log publishDir
    Trace.log (sprintf "%A" files)

    if not <| Directory.Exists uploadDir then
        Directory.CreateDirectory uploadDir |> ignore

    files |> Zip.zip publishDir (uploadDir </> zipFileName)
)

Target.create "GitHubRelease" (fun _ ->
    let token = githubToken.Value
    let auth = sprintf "%s:x-oauth-basic@" token
    let repoUrl = sprintf "https://%sgithub.com/%s/%s.git" auth githubReleaseUser gitName

    let versionString = FakeVar.get versionStringKey |> Option.get
    let uploadDir = versionString |> uploadDir

    let gitDirectory = getVarOrDefaultFromVault "GIT_DIRECTORY" ""
    if not BuildServer.isLocalBuild then
        Git.CommandHelper.directRunGitCommandAndFail gitDirectory "config user.email v1adp1exu5@gmail.com"
        Git.CommandHelper.directRunGitCommandAndFail gitDirectory "config user.name \"Vladimir Likhatskiy\""

    // Add Tag (To create a new release you must have a corresponding tag in the repository. See the git-database.md docs for details.)
    let tag = sprintf "v%s" versionString
    Git.Branches.tag gitDirectory tag
    Git.Branches.pushTag gitDirectory repoUrl tag

    GitHub.createClientWithToken token
    |> GitHub.draftNewRelease githubReleaseUser gitName tag (release.SemVer.PreRelease <> None) release.Notes
    |> GitHub.uploadFiles !!uploadDir
    |> GitHub.publishDraft
    |> Async.RunSynchronously
)

Target.create "All" ignore
Target.create "Local" ignore

Target.create "CheckRelease" (fun _ ->
    Trace.log (sprintf "%A" (System.Environment.GetCommandLineArgs()))
    Trace.log (sprintf "%A" release)
)

"GetVersion"
    ==> "Clean"
    ==> "Restore"

if isGitHubActions then
    "CheckReleaseSecrets" ==> "Build" ==> "SetupNuGet" ==> "Test"
else
    "Build" ==> "Test"

"Test"
    ==> "Publish"
    ==> "Compress"
    =?> ("GitHubRelease", isGitHubActions)
    ==> "All"

"Build"
    ==> "Build_Debug"
    ==> "Publish"
    ==> "Compress"
    ==> "Local"


Target.runOrDefaultWithArguments "All"
