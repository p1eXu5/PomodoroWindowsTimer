open Microsoft.VisualBasic


#load "increase_versionf.fsx"
#load "pack_update_version.fsx"

#r "nuget: Xake, 2.2.0.5"
#r "nuget: Xake.Dotnet, 1.1.4.7-beta"

(*
    - what to pack is predefined
    - cmd file must contains mnemonic name of project or group of project and list
    - if pack is choosen then additional parameter must be set to determine increase function
*)

open System
open Xake
open Xake.Tasks
open Xake.Dotnet
open Pack_update_version
open Increase_versionf

[<Literal>]
let DEFAULT_VERSION = "0.0.1"

let private packageMask fileNameWithoutExtension =
    sprintf "%s.*.nupkg" fileNameWithoutExtension

let private regex fileNameWithoutExtension = System.Text.RegularExpressions.Regex($"{fileNameWithoutExtension}.(\d+).(\d+).(\d+).nupkg$")

let private nextVersion nugetPath increaseVersionf (projectFile: File) =
    let fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(projectFile.Name)
    let lastNuget =
        Fileset.listByMask nugetPath (Path.PathMask [Path.Part.FileMask (packageMask fileNameWithoutExtension)])
        |> Seq.choose (fun fn ->
            let m = regex fileNameWithoutExtension |> _.Match(fn)
            if m.Groups.Count > 3 then
                let v (ind: int) = m.Groups[ind] |> _.Value |> Int32.Parse
                Version(v 1, v 2, v 3) |> Some
            else
                None
        )
        |> Seq.sortByDescending id
        |> Seq.map (fun n ->
            printfn "listed nuget file: %s %A" fileNameWithoutExtension n
            n
        )
        |> Seq.tryHead

    match lastNuget, increaseVersionf with
    | Some ver, _ ->
        increaseVersionf
        |> IncreaseVersionf.value
        |> fun f -> f ver.Major ver.Minor ver.Build
    | None, IncreaseVersionf.Current _ ->
        getCurrentVersion (projectFile.FullName)
        |> Option.defaultValue DEFAULT_VERSION
    | None, _ ->
        DEFAULT_VERSION


let private pack nugetPath increaseVersionf projectFileset =
    recipe {
        let! (Filelist projectFiles) = projectFileset |> getFiles

        match projectFiles with
        | [] ->
            do! trace Error "NO files have been found!"
            return ()

        | _ ->
            if not <| System.IO.Directory.Exists(nugetPath) then
                System.IO.Directory.CreateDirectory(nugetPath) |> ignore

            for f in projectFiles do
                let nextVersion = nextVersion nugetPath increaseVersionf f
                do! trace Message "Pack %s -> v%s" f.Name nextVersion
                let! _ = shell {
                    cmd "dotnet pack"
                    workdir "./"
                    args [
                        f.Name
                        "-c Debug"
                        $"-p:PackageVersion={nextVersion}"
                        "--force"
                        $"-o {nugetPath}"
                        // "--version-suffix test"
                    ]
                    failonerror
                }
                updateProjectVersions f.FullName nextVersion

            return ()
    }

/// Grabbing wanted rule and increaseVersionf from command line arguments. For example:
///
/// dotnet fsi .\pack.fsx "rule" "increaseVersionf"
let wantedRuleIncreasef () =
    printfn "%A" fsi.CommandLineArgs

    let ruleName () =
        if not <| String.IsNullOrWhiteSpace(fsi.CommandLineArgs[1]) then
            let ruleName = fsi.CommandLineArgs[1].Trim()
            if not <| ruleName.StartsWith("-") then
                ruleName |> Ok
            else
                Result.Error "Rule name is unknown"
        else
            Result.Error "Rule name must not be empty"

    let increasef () =
        if not <| String.IsNullOrWhiteSpace(fsi.CommandLineArgs[2]) then
            let funcName = fsi.CommandLineArgs[2].Trim()
            if not <| funcName.StartsWith("-") then
                funcName |> IncreaseVersionf.ofString |> Result.map (fun _ -> funcName)
            else
                Result.Error "Increase version function is unknown"
        else
            Result.Error "Increase version function must not be empty"

    if fsi.CommandLineArgs.Length >= 3 && not <| fsi.CommandLineArgs[2].StartsWith("-") then
        match ruleName (), increasef () with
        | Ok ruleName, Ok increasef -> ruleName, increasef
        | Result.Error err, _ -> raise (ArgumentException err)
        | _, Result.Error err -> raise (ArgumentException err)
    elif fsi.CommandLineArgs.Length >= 2 && not <| fsi.CommandLineArgs[1].StartsWith("-") then
        match ruleName () with
        | Ok ruleName -> ruleName, ""
        | Result.Error err -> raise (ArgumentException err)
    else
        "main", ""

/// Creates $"pack_%s{ruleNamePostfix}" ExecContext Rule.
let packRule nugetPath ruleNamePostfix projectFileset : ExecContext Rule =
    $"pack_%s{ruleNamePostfix}" => recipe {
        do! alwaysRerun ()

        let! increasefName = getVar "increasef"
        let increasef = increasefName.Value |> IncreaseVersionf.ofString |> function Ok f -> f | _ -> failwith "Increase version function is unknown"

        do! projectFileset |> pack nugetPath increasef

        return ()
    }