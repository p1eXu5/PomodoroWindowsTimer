namespace PomodoroWindowsTimer.Storage.Tests

open System
open System.IO

open NUnit.Framework
open FsUnit
open p1eXu5.FSharp.Testing.ShouldExtensions
open FsUnitTyped
open FsToolkit.ErrorHandling

open PomodoroWindowsTimer.Storage
open PomodoroWindowsTimer.Testing.Fakers
open PomodoroWindowsTimer.Types


[<Category("DB. WorkEvent")>]
module WorkEventRepositoryTests =

    let private dbFileName = "work_event_test.db"

    let mutable private workId1 = Unchecked.defaultof<uint64>
    let mutable private workId2 = Unchecked.defaultof<uint64>

    let mutable private activeTimePointId = TimePointId.generate ()

    let private workRepository () = TestDb.workRepository dbFileName
    let private workEventRepository () = TestDb.workEventRepository dbFileName
    let private activeTimePointRepository () = TestDb.activeTimePointRepository dbFileName

    let private createWork () =
        task {
            let repo = workRepository ()
            return! repo.InsertAsync (generateNumber ()) (generateTitle ()) ct
        }

    [<OneTimeSetUp>]
    let Setup () =
        task {
            match! workRepository () :?> WorkRepository |> _.CreateTableAsync(ct) with
            | Ok _ -> ()
            | Error err -> raise (InvalidOperationException(err))

            match! activeTimePointRepository () :?> ActiveTimePointRepository |> _.CreateTableAsync(ct) with
            | Ok _ -> ()
            | Error err -> raise (InvalidOperationException(err))

            match! workEventRepository () :?> WorkEventRepository |> _.CreateTableAsync(ct) with
            | Ok _ -> ()
            | Error err -> raise (InvalidOperationException(err))

            let! res = createWork ()
            match res with
            | Ok (id, _) ->
                workId1 <- id
            | Error err -> raise (InvalidOperationException(err))

            let! res = createWork ()
            match res with
            | Ok (id, _) ->
                workId2 <- id
            | Error err ->
                assertionExn err

            let atp = ActiveTimePoint.generate ()
            let! res = (activeTimePointRepository ()).InsertAsync atp ct
            match res with
            | Ok () ->
                activeTimePointId <- atp.Id
            | Error err ->
                assertionExn err
        }

    [<OneTimeTearDown>]
    let TearDown () =
        task {
            let dataSource = dbFileName |> dataSource
            if File.Exists(dataSource) then
                File.Delete(dataSource)
        }

    
    [<Test>]
    let ``01: InsertAsync test`` () =
        taskResult {
            let workEventRepo = workEventRepository ()

            let! id1 = workEventRepo.InsertAsync workId1 (WorkEvent.generateWith activeTimePointId) ct
            let! id2 = workEventRepo.InsertAsync workId1 (WorkEvent.generateWith activeTimePointId) ct

            id1 |> shouldBeGreaterThan 0UL
            id2 |> shouldBeGreaterThan id1
        }
        |> TaskResult.runTest

    [<Test>]
    let ``02: FindByWorkIdAsync test`` () =
        taskResult {
            let workEventRepo = workEventRepository ()

            let workEvent = WorkEvent.generateWith activeTimePointId
            let! _ = workEventRepo.InsertAsync workId1 workEvent ct

            let! rows = workEventRepo.FindByWorkIdAsync workId1 ct

            rows |> shouldContain workEvent
        }
        |> TaskResult.runTest

    [<Test>]
    let ``03: FindByWorkIdByDateAsync test`` () =
        taskResult {
            let workEventRepo = workEventRepository ()

            let workEvent1 = WorkEvent.generateWith activeTimePointId
            let workEvent2 = WorkEvent.generateWith activeTimePointId
            let workEvent3 = WorkEvent.generateWith activeTimePointId
            let workEvent4 = WorkEvent.generateWith activeTimePointId
            let workEvent5 = WorkEvent.generateWith activeTimePointId

            let! _ = workEventRepo.InsertAsync workId1 workEvent1 ct
            let! _ = workEventRepo.InsertAsync workId1 workEvent2 ct
            let! _ = workEventRepo.InsertAsync workId1 workEvent3 ct
            let! _ = workEventRepo.InsertAsync workId1 workEvent4 ct
            let! _ = workEventRepo.InsertAsync workId1 workEvent5 ct

            let! rows = workEventRepo.FindByWorkIdByDateAsync workId1 (workEvent3 |> WorkEvent.dateOnly) ct

            rows |> shouldContain workEvent3
        }
        |> TaskResult.runTest

    [<Test>]
    let ``findByWorkIdByDate date without event test`` () =
        taskResult {
            let workEventRepo = workEventRepository ()

            let workEvent1 = WorkEvent.generateWith activeTimePointId
            let workEvent2 = WorkEvent.generateWith activeTimePointId
            let workEvent3 = WorkEvent.generateWith activeTimePointId
            let workEvent4 = WorkEvent.generateWith activeTimePointId
            let workEvent5 = WorkEvent.generateWith activeTimePointId

            let! _ = workEventRepo.InsertAsync workId1 workEvent1 ct
            let! _ = workEventRepo.InsertAsync workId1 workEvent2 ct
            let! _ = workEventRepo.InsertAsync workId1 workEvent3 ct
            let! _ = workEventRepo.InsertAsync workId1 workEvent4 ct
            let! _ = workEventRepo.InsertAsync workId1 workEvent5 ct

            let date = DateOnly.FromDateTime(System.TimeProvider.System.GetUtcNow().DateTime.AddDays(1))

            let! rows = workEventRepo.FindByWorkIdByDateAsync workId1 date ct

            rows |> should be Empty
        }
        |> TaskResult.runTest

    [<Test>]
    let ``findAllByPeriod period with events test`` () =
        taskResult {
            let workEventRepo = workEventRepository ()

            let work1Events =
                [
                    WorkEvent.generateWith activeTimePointId
                    WorkEvent.generateWith activeTimePointId
                    WorkEvent.generateWith activeTimePointId
                    WorkEvent.generateWith activeTimePointId
                    WorkEvent.generateWith activeTimePointId
                ]

            let work2Events =
                [
                    WorkEvent.generateWith activeTimePointId
                    WorkEvent.generateWith activeTimePointId
                    WorkEvent.generateWith activeTimePointId
                    WorkEvent.generateWith activeTimePointId
                    WorkEvent.generateWith activeTimePointId
                ]

            for i in 0 .. 4 do
                let! _ = workEventRepo.InsertAsync workId1 work1Events[i] ct
                ()

            for i in 0 .. 4 do
                let! _ = workEventRepo.InsertAsync workId2 work2Events[i] ct
                ()

            let minDate =
                DateOnly.FromDateTime(
                    (work1Events @ work2Events)
                    |> List.map WorkEvent.createdAt
                    |> List.min
                    |> fun dt -> dt.DateTime
                )

            let maxDate =
                DateOnly.FromDateTime(
                    (work1Events @ work2Events)
                    |> List.map WorkEvent.createdAt
                    |> List.max
                    |> fun dt -> dt.DateTime
                )

            let! rows = workEventRepo.FindAllByPeriodAsync ({ Start = minDate; EndInclusive = maxDate }) ct

            let list = rows |> Seq.toList
            list.Length |> should greaterThanOrEqualTo 2
            list[0].Events.Length |> should greaterThanOrEqualTo 5
            list[1].Events.Length |> should greaterThanOrEqualTo 5
        }
        |> TaskResult.runTest

