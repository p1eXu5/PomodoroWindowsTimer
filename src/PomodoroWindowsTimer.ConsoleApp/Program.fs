open System
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.TimePointQueue
open System.Threading
open PomodoroWindowsTimer.Looper
open PomodoroWindowsTimer.Abstrractions

let semaphore = new SemaphoreSlim(0, 1)

let timePoints =
    [
        { Id = Guid.NewGuid(); Name = "1"; TimeSpan = TimeSpan.FromMilliseconds(1000); Kind = Kind.Work }
        { Id = Guid.NewGuid(); Name = "2"; TimeSpan = TimeSpan.FromMilliseconds(2000); Kind = Kind.Break }
        { Id = Guid.NewGuid(); Name = "3"; TimeSpan = TimeSpan.FromMilliseconds(3000); Kind = Kind.Work }
        { Id = Guid.NewGuid(); Name = "4"; TimeSpan = TimeSpan.FromMilliseconds(4000); Kind = Kind.Break }
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
printfn "PomodoroWindowsTimer"

program ()
|> Async.RunSynchronously
