module PomodoroWindowsTimer.ElmishApp.Tests.Features.StopResume.Steps.Then

open System
open Microsoft.Extensions.DependencyInjection

open FsUnit
open p1eXu5.FSharp.Testing.ShouldExtensions

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Models

open PomodoroWindowsTimer.ElmishApp.Tests
open PomodoroWindowsTimer.ElmishApp.Tests.ScenarioCE

open PomodoroWindowsTimer.ElmishApp.Tests.Features.Helpers

let rec ``Active Point is set on`` (timePoint: TimePoint) =
    scenario {
        let! (sut: ISut) = Scenario.getState
        
        sut.MainModel.ActiveTimePoint
        |> Option.map (fun atp -> atp.Id = timePoint.Id)
        |> Option.defaultValue false
        |> shouldL be True (sprintf "%s:\n%A" (nameof ``Active Point is set on``) timePoint)
    }

let ``LooperState is`` (looperState: LooperState) =
    scenario {
        let! (sut: ISut) = Scenario.getState
        
        sut.MainModel.LooperState
        |> shouldL equal looperState ("LooperState is not Initialized")
    }

let ``Windows should not be minimized`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState
        
        sut.MainModel.IsMinimized
        |> shouldL be False "MainModel.IsMinimized is not false"
    }

let ``Windows should be minimized`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState
        
        sut.MainModel.IsMinimized
        |> shouldL be True "MainModel.IsMinimized is not true"
    }

let rec ``Telegrtam bot should not be notified`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let telegramBotStub = sut.ServiceProvider.GetRequiredService<ITelegramBot>() :?> TelegramBotStub

        telegramBotStub.MessageStack |> shouldL be Empty (nameof ``Telegrtam bot should not be notified``)
    }

let ``Telegrtam bot should be notified with`` (timePointName: string) =
    scenario {
        let! (sut: ISut) = Scenario.getState

        do! Scenario.msgDispatchedWithin2Sec "SendToChatBot" (fun msg ->
            match msg with
            | MainModel.Msg.SendToChatBot msg ->
                msg.Contains(timePointName, StringComparison.Ordinal)
            | _ -> false
        )

        let telegramBotStub = sut.ServiceProvider.GetRequiredService<ITelegramBot>() :?> TelegramBotStub
        telegramBotStub.MessageStack |> shouldL not' (be Empty) ("TelegramBot message stack is empty")
    }

let rec ``Active Point remaining time is equal to or less then`` (timePoint: TimePoint) =
    scenario {
        let! (sut: ISut) = Scenario.getState

        sut.MainModel.ActiveTimePoint
        |> Option.map (fun atp ->
            // need to add offset cause it can be added to the swithed TimePoint
            atp.TimeSpan <= timePoint.TimeSpan.Add(TimeSpan.FromMilliseconds(float Program.tickMilliseconds)))
        |> Option.defaultValue false
        |> shouldL be True (nameof ``Active Point remaining time is equal to or less then``)
    }


//let rec ``LooperState is Playing`` () =
//    model.LooperState
//    |> shouldL be (ofCase <@ LooperState.Playing @>) (nameof ``LooperState is Playing``)

//let rec ``LooperState is Stopped`` () =
//    model.LooperState
//    |> shouldL be (ofCase <@ LooperState.Stopped @>) (nameof ``LooperState is Stopped``)


