namespace PomodoroWindowsTimer.Storage.Tests

open System
open System.IO

open NUnit.Framework
open FsUnit
open p1eXu5.FSharp.Testing.ShouldExtensions
open FsUnitTyped
open FsToolkit.ErrorHandling
open Faqt
open Faqt.Operators

open PomodoroWindowsTimer.Storage
open PomodoroWindowsTimer.Testing.Fakers
open PomodoroWindowsTimer.Types


[<Category("DB. WorkEvent")>]
module WorkEventRepositoryTests =

    let private dbFileName = "work_event_test.db"

    let mutable private workId1 = Unchecked.defaultof<uint64>
    let mutable private work1 = Unchecked.defaultof<Work>
    let mutable private workId2 = Unchecked.defaultof<uint64>
    let mutable private work2 = Unchecked.defaultof<Work>

    let mutable private atpId1 = Unchecked.defaultof<TimePointId>
    let mutable private atpId2 = Unchecked.defaultof<TimePointId>

    let private workRepository () = TestDb.workRepository dbFileName
    let private workEventRepository () = TestDb.workEventRepository dbFileName
    let private activeTimePointRepository () = TestDb.activeTimePointRepository dbFileName

    let private createWork () =
        task {
            let repo = workRepository ()
            let number = generateNumber ()
            let title = generateTitle ()

            let! res = repo.InsertAsync number title ct
            match res with
            | Ok (id, createdAt) ->
                return 
                    {
                        Id = id
                        Number = number
                        Title = title
                        CreatedAt = createdAt
                        UpdatedAt = createdAt
                        LastEventCreatedAt = None
                    }
                    |> Ok
            | Error err -> return Error err
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

            match! workEventRepository () :?> WorkEventRepository |> _.CreateActualTableAsync(ct) with
            | Ok _ -> ()
            | Error err -> raise (InvalidOperationException(err))

            let! res = createWork ()
            match res with
            | Ok (w1) ->
                workId1 <- w1.Id
                work1 <- w1
            | Error err -> raise (InvalidOperationException(err))

            let! res = createWork ()
            match res with
            | Ok (w2) ->
                workId2 <- w2.Id
                work2 <- w2
            | Error err ->
                assertionExn err

            let atp1 = ActiveTimePoint.generate ()
            let! res = (activeTimePointRepository ()).InsertAsync atp1 ct
            match res with
            | Ok () ->
                atpId1 <- atp1.Id
            | Error err ->
                assertionExn err

            let atp2 = ActiveTimePoint.generate ()
            let! res = (activeTimePointRepository ()).InsertAsync atp2 ct
            match res with
            | Ok () ->
                atpId2 <- atp2.Id
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

            let! id1 = workEventRepo.InsertAsync workId1 (WorkEvent.generateWith atpId1) ct
            let! id2 = workEventRepo.InsertAsync workId1 (WorkEvent.generateWith atpId1) ct

            let ev = WorkEvent.BreakIncreased (faker.Date.RecentOffset(7), TimeSpan.FromMinutes(faker.Random.Int(1, 25)), None)
            let! id3 = workEventRepo.InsertAsync workId1 ev ct

            id1 |> shouldBeGreaterThan 0UL
            id2 |> shouldBeGreaterThan id1
            id3 |> shouldBeGreaterThan id2
        }
        |> TaskResult.runTest

    [<Test>]
    let ``02: FindByWorkIdAsync test`` () =
        taskResult {
            let workEventRepo = workEventRepository ()

            let workEvent = WorkEvent.generateWith atpId1
            let! _ = workEventRepo.InsertAsync workId1 workEvent ct

            let! rows = workEventRepo.FindByWorkIdAsync workId1 ct

            rows |> shouldContain workEvent
        }
        |> TaskResult.runTest

    [<Test>]
    let ``03: FindByWorkIdByDateAsync test`` () =
        taskResult {
            let workEventRepo = workEventRepository ()

            let workEvent1 = WorkEvent.generateWith atpId1
            let workEvent2 = WorkEvent.generateWith atpId1
            let workEvent3 = WorkEvent.generateWith atpId1
            let workEvent4 = WorkEvent.generateWith atpId1
            let workEvent5 = WorkEvent.generateWith atpId1

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
    let ``04: FindByWorkIdByDateAsync date without event test`` () =
        taskResult {
            let workEventRepo = workEventRepository ()

            let workEvent1 = WorkEvent.generateWith atpId1
            let workEvent2 = WorkEvent.generateWith atpId1
            let workEvent3 = WorkEvent.generateWith atpId1
            let workEvent4 = WorkEvent.generateWith atpId1
            let workEvent5 = WorkEvent.generateWith atpId1

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
    let ``05: FindAllByPeriodAsync period with events test`` () =
        taskResult {
            let workEventRepo = workEventRepository ()

            let work1Events =
                [
                    WorkEvent.generateWith atpId1
                    WorkEvent.generateWith atpId1
                    WorkEvent.generateWith atpId1
                    WorkEvent.generateWith atpId1
                    WorkEvent.generateWith atpId1
                ]

            let work2Events =
                [
                    WorkEvent.generateWith atpId1
                    WorkEvent.generateWith atpId1
                    WorkEvent.generateWith atpId1
                    WorkEvent.generateWith atpId1
                    WorkEvent.generateWith atpId1
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

    type CaseData =
        {
            Date: DateOnly
            Events: WorkEvent list
        }


    [<Test>]
    let ``06: FindByActiveTimePointIdByDateAsync - start, stop and increase events exists - returns start and stop events`` () =
        let workCaseData () =
            let date = DateOnly.FromDateTime(System.TimeProvider.System.GetUtcNow().Date)
            {
                Date = date
                Events =
                    [
                        WorkEvent.generateWorkStartedWith date "12:00" |> WorkEvent.withActiveTimePointId atpId1
                        WorkEvent.generateStoppedWith date "12:10"
                        WorkEvent.generateWorkIncreasedWith date "12:20"

                        WorkEvent.generateWorkStartedWith date "12:30" |> WorkEvent.withActiveTimePointId atpId2
                        WorkEvent.generateWorkIncreasedWith date "12:31" 
                        WorkEvent.generateWorkIncreasedWith date "12:32" |> WorkEvent.withActiveTimePointId atpId2
                        WorkEvent.generateStoppedWith date "12:40"

                        WorkEvent.generateWorkStartedWith date "12:50" |> WorkEvent.withActiveTimePointId atpId2
                        WorkEvent.generateStoppedWith date "13:00"
                    ]
            }

        taskResult {
            let caseData = workCaseData ()
            let workEventRepo = workEventRepository ()
            for ev in caseData.Events do
                let! _ = workEventRepo.InsertAsync workId1 ev ct
                ()

            // act
            let! rows = workEventRepo.FindByActiveTimePointIdByDateAsync atpId2 Kind.Work (WorkEvent.generateCreatedAt caseData.Date "12:40") ct

            // assert
            let expected =
                [
                    work1, caseData.Events[6] |> WorkEvent.trimMicroseconds
                    work1, caseData.Events[5] |> WorkEvent.trimMicroseconds
                    work1, caseData.Events[3] |> WorkEvent.trimMicroseconds
                ]

            do %rows.Should().Be(expected)
        }
        |> TaskResult.runTest
