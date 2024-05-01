module PomodoroWindowsTimer.ElmishApp.Tests.Features.CommonSteps.Given

open Microsoft.Extensions.DependencyInjection
open PomodoroWindowsTimer.ElmishApp.Tests
open PomodoroWindowsTimer.ElmishApp.Tests.ScenarioCE
open PomodoroWindowsTimer.ElmishApp.Tests.Features
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer

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
