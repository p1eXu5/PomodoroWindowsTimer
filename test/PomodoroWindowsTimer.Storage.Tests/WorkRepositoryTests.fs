namespace PomodoroWindowsTimer.Storage.Tests

open System
open System.IO

open NUnit.Framework
open FsUnit.TopLevelOperators
open FsUnitTyped.TopLevelOperators
open FsToolkit.ErrorHandling
open p1eXu5.FSharp.Testing.ShouldExtensions

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions
open PomodoroWindowsTimer.Testing.Fakers


[<Category("DB. Work")>]
module WorkRepositoryTests =

    let private dbFileName = $"work_test_{Guid.NewGuid()}.db"

    let mutable _repositoryFactory = Unchecked.defaultof<IRepositoryFactory>

    let private workRepository () = _repositoryFactory.GetWorkRepository ()
    let private workEventRepository () = _repositoryFactory.GetWorkEventRepository ()
    let private activeTimePointRepository () = _repositoryFactory.GetActiveTimePointRepository ()

    [<OneTimeSetUp>]
    let Setup () =
        task {
            _repositoryFactory <- repositoryFactory dbFileName
            do! seedDataBase _repositoryFactory
            do applyMigrations dbFileName
        }

    [<OneTimeTearDown>]
    let TearDown () =
        task {
            let dataSource = dbFileName |> dataSource
            if File.Exists(dataSource) then
                File.Delete(dataSource)
        }

    [<SetUp>]
    let SetWriters () =
        tcw.SetWriters(TestContext.Progress, TestContext.Out)

    [<Test>]
    let ``01: InsertAsync -> by default -> inserts work row`` () =
        task {
            let workRepo = workRepository ()

            let! res1 = workRepo.InsertAsync (generateNumber ()) (generateTitle ()) ct
            let! res2 = workRepo.InsertAsync (generateNumber ()) (generateTitle ()) ct

            match res1, res2 with
            | Error err, _ -> failAssert err
            | _, Error err -> failAssert err
            | Ok (id1, _), Ok (id2, _) ->
                id1 |> shouldBeGreaterThan 0UL
                id2 |> shouldBeGreaterThan id1
        }

    [<Test>]
    let ``02: ReadAllAsync - no events - returns with none latest event created_at`` () =
        taskResult {
            let workRepo = workRepository ()

            let number1 = generateNumber ()
            let title1 = generateTitle ()

            let! _ = workRepo.InsertAsync number1 title1 ct

            let! rows = workRepo.ReadAllAsync(ct)

            rows |> Seq.map (fun r -> r.Number, r.Title) |> shouldContain (number1, title1)
        }
        |> TaskResult.runTest

    [<Test>]
    let ``03: ReadAllAsync - events exists - returns with latest ecvent created_at`` () =
        taskResult {
            let workRepo = workRepository ()
            let workEventRepo = workEventRepository ()
            let atpRepo = activeTimePointRepository ()

            let atp = ActiveTimePoint.generate ()
            do! atpRepo.InsertAsync atp ct

            let number1 = generateNumber ()
            let title1 = generateTitle ()

            let! (workId1, _) = workRepo.InsertAsync number1 title1 ct

            let workEvents =
                [
                    WorkEvent.generate () |> WorkEvent.withActiveTimePointId atp.Id
                    WorkEvent.generate () |> WorkEvent.withActiveTimePointId atp.Id
                    WorkEvent.generate () |> WorkEvent.withActiveTimePointId atp.Id
                ]

            let! _ = workEventRepo.InsertAsync workId1 workEvents[0] ct
            let! _ = workEventRepo.InsertAsync workId1 workEvents[1] ct
            let! _ = workEventRepo.InsertAsync workId1 workEvents[2] ct

            let number2 = generateNumber ()
            let title2 = generateTitle ()

            let! _ = workRepo.InsertAsync number2 title2 ct

            let! rows = workRepo.ReadAllAsync(ct)

            rows
            |> Seq.map (fun r -> r.Number, r.Title, (r.LastEventCreatedAt |> Option.map _.ToUnixTimeMilliseconds()))
            |> shouldContain (number1, title1, (workEvents |> List.map WorkEvent.createdAt |> List.max).ToUnixTimeMilliseconds() |> Some)
            
            rows |> Seq.map (fun r -> r.Number, r.Title, r.LastEventCreatedAt) |> shouldContain (number2, title2, None)
        }
        |> TaskResult.runTest

    [<Test>]
    let ``04: SearchByNumberOrTitleAsync test`` () =
        taskResult {
            let workRepo = workRepository ()

            let number = generateNumber ()
            let title = generateTitle ()

            let! _ = workRepo.InsertAsync number title ct
            let! rows = workRepo.SearchByNumberOrTitleAsync title[..3] ct

            rows |> Seq.map (fun r -> r.Number, r.Title) |> shouldContain (number, title)
        }
        |> TaskResult.runTest


    [<Test>]
    let ``05: UpdateAsync -> work exists -> updates work`` () =
        taskResult {
            let workRepo = workRepository ()
        
            let! (workId, _) = workRepo.InsertAsync (generateNumber ()) (generateTitle ()) ct
            let! work = workRepo.FindByIdAsync workId ct

            let number = generateNumber ()
            let title = generateTitle ()

            let updatedWork =
                {
                    work.Value with
                        Number = number
                        Title = title
                }

            // action
            let! _ = workRepo.UpdateAsync updatedWork ct

            // assert
            let! foundWorks = workRepo.SearchByNumberOrTitleAsync title ct
            foundWorks |> Seq.map (fun r -> r.Number, r.Title) |> shouldContain (number, title)
        }
        |> TaskResult.runTest

    [<Test>]
    let ``06: DeleteAsync -> deletes db work`` () =
        taskResult {
            let workRepo = workRepository ()

            let work = Work.generate ()

            let! (workId, createdAt) = workRepo.InsertAsync (generateNumber ()) (generateTitle ()) ct
            
            do!
                workRepo.DeleteAsync
                    { work with Id = workId; UpdatedAt = createdAt; }
                    ct

            let! dbWork = workRepo.FindByIdAsync workId ct
            dbWork |> should be (ofCase <@ Option<Work>.None @>)
        }
        |> TaskResult.runTest

