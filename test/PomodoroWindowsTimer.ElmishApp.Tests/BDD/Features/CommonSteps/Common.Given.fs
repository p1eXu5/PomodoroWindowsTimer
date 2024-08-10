module PomodoroWindowsTimer.ElmishApp.Tests.Features.CommonSteps.Given

open Microsoft.Extensions.DependencyInjection

open p1eXu5.AspNetCore.Testing
open p1eXu5.AspNetCore.Testing.MockRepository

open PomodoroWindowsTimer.ElmishApp.Tests
open PomodoroWindowsTimer.ElmishApp.Tests.ScenarioCE
open PomodoroWindowsTimer.ElmishApp.Tests.Features
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer
open PomodoroWindowsTimer.Abstractions

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

let ``Stored CurrentWork`` (work) =
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
