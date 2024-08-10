module PomodoroWindowsTimer.ElmishApp.Tests.Features.CommonSteps.Given

open System.Threading
open Microsoft.Extensions.DependencyInjection

open p1eXu5.AspNetCore.Testing
open p1eXu5.AspNetCore.Testing.MockRepository
open PomodoroWindowsTimer.Testing.Fakers

open PomodoroWindowsTimer
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions
open PomodoroWindowsTimer.ElmishApp.Tests
open PomodoroWindowsTimer.ElmishApp.Tests.ScenarioCE
open PomodoroWindowsTimer.ElmishApp.Tests.Features
open PomodoroWindowsTimer.ElmishApp.Abstractions
open p1eXu5.FSharp.Testing.ShouldExtensions

let ``Initialized Program`` () =
    scenario {
        return! Scenario.replaceStateWith (Sut.run)
    }
    |> Scenario.log "Given.``Initialized Program``"


let ``Stored TimePoints`` (timePoints) =
    scenario {
        do!
            Scenario.replaceStateWith (fun f ->
                fun sut ->
                    let (sut': #ISut) = f sut
                    let userSettings = sut'.ServiceProvider.GetRequiredService<IUserSettings>()
                    let timePointsJson = JsonHelpers.Serialize(timePoints)
                    userSettings.TimePointSettings <- timePointsJson |> Some
                    sut'
            )
    }
    |> Scenario.log "Given.``Stored TimePoints``"

let ``CurrentWork in UserSettings`` (work) =
    scenario {
        do!
            Scenario.replaceStateWith (fun f ->
                fun sut ->
                    let (sut': #ISut) = f sut
                    let userSettings = sut'.ServiceProvider.GetRequiredService<IUserSettings>()
                    userSettings.CurrentWork <- work |> Some
                    sut'
            )
    }
    |> Scenario.log "Given.``Stored TimePoints``"

/// 1. Generates Work
///
/// 2. Storing Work in the WorkRepository
///
/// 3. Sets UserSettings.CurrentWork to created Work
///
/// 4. Adds created Work to the ScenarioContext
let ``Work in WorkRepository and UserSettings`` () =
    scenario {
        do!
            Scenario.replaceStateWith (fun f ->
                fun sut ->
                    let (sut': #ISut) = f sut
                    let workRepo = sut'.ServiceProvider.GetRequiredService<IWorkRepository>()
                    let work = Work.generate ()
                    let idRes = 
                        workRepo.InsertAsync work.Number work.Title CancellationToken.None   
                        |> Async.AwaitTask
                        |> Async.RunSynchronously
                    
                    match idRes with
                    | Ok (id, createdAt) ->
                        let createdWork = { work with Id = id; CreatedAt = createdAt; UpdatedAt = createdAt }
                        let userSettings = sut'.ServiceProvider.GetRequiredService<IUserSettings>()
                        userSettings.CurrentWork <- createdWork |> Some
                        sut'.ScenarioContext["CurrentWork"] <- createdWork
                    | Error err -> assertionExn err
                    sut'
            )
    }
    |> Scenario.log "Given.``Stored TimePoints``"



let ``WorkEventStore substitution`` () =
    scenario {
        do!
            Scenario.replaceStateWith (fun f ->
                fun sut ->
                    let (sut': #ISut) = f sut
                    let _ = sut'.MockRepository.TrySubstitute<IWorkEventRepository>()
                    sut'
            )
    }
