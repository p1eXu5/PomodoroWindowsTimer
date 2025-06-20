(*
    Script for clearing.

    Using:

        a) put script file to target folder and run
          `dotnet fsi .\clear-bins.fsx`

        b) use command line argument:
          `dotnet fsi .\clear-bins.fsx "D:\TEKHO\xpay-tsm\NET"
*)

#if INTERACTIVE
#else
module ClearBins =
#endif

open System
open System.IO
open System.Text
open System.Threading.Tasks


let rec iter root =

    let enumerateDirs searchPattern =
        Directory.EnumerateDirectories(root, searchPattern, SearchOption.AllDirectories)

    let delete searchPattern =
        try 
            enumerateDirs searchPattern
            |> Seq.iter (fun dirName ->
                printfn "Deleting %s... " dirName
                Directory.Delete(dirName, true)
                // printfn "Done."
            )
        with 
        | ex -> 
            printfn "%s" ex.Message

    task {
        // printfn "Processing %s..." root
        
        Parallel.ForEach(
            seq {
                "obj"
                "bin"
                "node_modules"
            }
            , delete
        ) |> ignore
        // delete "bin"
        // delete "node_modules"

        let! _ =
            Directory.EnumerateDirectories(root)
            |> Seq.map iter
            |> Task.WhenAll

        // printfn "%s processed." root
        return ()
    }
    


do 

    let sourceRoot =
        if fsi.CommandLineArgs.Length > 1 && Directory.Exists(fsi.CommandLineArgs[1])
        then
            fsi.CommandLineArgs[1]
        else
            __SOURCE_DIRECTORY__


    iter sourceRoot
    |> Async.AwaitTask
    |> Async.RunSynchronously