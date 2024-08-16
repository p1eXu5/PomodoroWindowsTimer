module rec PomodoroWindowsTimer.ElmishApp.Tests.Features.CommonSteps.Then

open Microsoft.Extensions.DependencyInjection

open Faqt
open Faqt.Operators

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp.Abstractions

open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.MainModel

open PomodoroWindowsTimer.ElmishApp.Tests
open PomodoroWindowsTimer.ElmishApp.Tests.ScenarioCE
open PomodoroWindowsTimer.ElmishApp.Tests.Features

let ``Looper TimePointStarted event has been despatched with`` newTimePoint oldTimePoint =
    scenario {
        do! Scenario.msgDispatchedWithin2Sec "Finish of LoadRecipeCards" (fun msg ->
            match msg with
            | MainModel.Msg.LooperMsg (LooperMsg.TimePointStarted ({ NewActiveTimePoint = newTp; OldActiveTimePoint = oldTp}))
                when newTp = newTimePoint && oldTp = oldTimePoint -> true
            | _ -> false
        )
    }
    |> Scenario.log $"Then.{nameof ``Looper TimePointStarted event has been despatched with``}"

let ``No errors are emitted`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let mainErrorMessageQueue = sut.ServiceProvider.GetRequiredKeyedService<IErrorMessageQueue>("main") :?> ErrorMessageQueueStub.T
        let dialogErrorMessageQueue = sut.ServiceProvider.GetRequiredKeyedService<IErrorMessageQueue>("dialog") :?> ErrorMessageQueueStub.T

        %mainErrorMessageQueue.ErrorQueue.Should().BeEmpty()
        %dialogErrorMessageQueue.ErrorQueue.Should().BeEmpty()
    }
    |> Scenario.log $"Then.{nameof ``No errors are emitted``}"

