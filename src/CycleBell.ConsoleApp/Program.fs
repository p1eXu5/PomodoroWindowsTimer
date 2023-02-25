open System
open CycleBell.Types
open CycleBell.TimePointQueue
open System.Threading
open CycleBell.Looper
open CycleBell.Abstrractions

let semaphore = new SemaphoreSlim(0, 1)

let timePoints =
    [
        { Name = "1"; TimeSpan = TimeSpan.FromMilliseconds(1000) }
        { Name = "2"; TimeSpan = TimeSpan.FromMilliseconds(2000) }
        { Name = "3"; TimeSpan = TimeSpan.FromMilliseconds(3000) }
        { Name = "4"; TimeSpan = TimeSpan.FromMilliseconds(4000) }
    ]

let program () =
    async {
        use timePointQueue = new TimePointQueue()
        use looper = new Looper((timePointQueue :> ITimePointQueue), 300)
        let log =
            fun (ev: LooperEvent) ->
                async {
                    printfn "%A" ev
                    match ev with
                    | LooperEvent.TimePointStarted (_, Some oldTp) when oldTp.Name = "4" ->
                        semaphore.Release() |> ignore
                        return ()
                    | _ -> return ()
                }

        do
            looper.Start([log])
            timePointQueue.AddMany(timePoints)

        do semaphore.Wait()
    }

// For more information see https://aka.ms/fsharp-console-apps
printfn "CycleBell"

program ()
|> Async.RunSynchronously
