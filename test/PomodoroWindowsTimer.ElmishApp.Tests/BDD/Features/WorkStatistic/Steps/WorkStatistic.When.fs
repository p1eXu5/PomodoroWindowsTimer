module rec PomodoroWindowsTimer.ElmishApp.Tests.BDD.Features.WorkStatistic.Steps.When

open Elmish.Extensions

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models

open PomodoroWindowsTimer.ElmishApp.Tests
open PomodoroWindowsTimer.ElmishApp.Tests.ScenarioCE
open PomodoroWindowsTimer.ElmishApp.Tests.Features.Helpers

let ``User opens work statistic window`` () =
    Common.``User opens work statistic window`` ()
    |> Scenario.log $"When.``{nameof ``User opens work statistic window``}``"

let ``Work daily statistic is shown`` (workId: WorkId) =
    Common.``Work daily statistic is shown`` workId
    |> Scenario.log $"When.``{nameof ``Work daily statistic is shown``} for work id {workId}``"

