module PomodoroWindowsTimer.ElmishApp.Tests.BDD.Features.WorkStatistic.Steps.Common

open Elmish.Extensions

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models

open PomodoroWindowsTimer.ElmishApp.Tests
open PomodoroWindowsTimer.ElmishApp.Tests.ScenarioCE
open PomodoroWindowsTimer.ElmishApp.Tests.Features.Helpers

let ``User opens work statistic window`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState

        do sut.Dispatcher.Dispatch (MainModel.Msg.SetIsWorkStatisticShown true)

        do!
            Scenario.msgDispatchedWithin2Sec
                "DailyStatisticListModel.Msg.SetIsWorkSelectorLoaded"
                MainModel.Msg.StatisticMainModel.DailyStatisticList.``Is finish of LoadDailyStatistics``
    }

let ``Work daily statistic is shown`` (workId: WorkId) =
    scenario {
        let! (sut: ISut) = Scenario.getState

        do! Scenario.modelSatisfiesWithin2Sec "DailyStatisticModel exists" (fun model ->
            match model.StatisticMainModel with
            | Some statModel ->
                match statModel.DailyStatisticListModel with
                | Some dailyListModel ->
                    match dailyListModel.DailyStatistics with
                    | AsyncDeferred.Retrieved dailyStatistics ->
                        dailyStatistics
                        |> List.exists (fun stat ->
                            match stat.WorkStatistics with
                            | AsyncDeferred.Retrieved workStatistics ->
                                workStatistics
                                |> List.exists (fun workStat -> workStat.WorkId = workId)
                            | _ -> false
                        )
                    | _ -> false
                | _ -> false
            | _ -> false
        )

        do!
            Scenario.msgDispatchedWithin2Sec
                "DailyStatisticListModel.Msg.SetIsWorkSelectorLoaded"
                MainModel.Msg.StatisticMainModel.DailyStatisticList.``Is finish of LoadDailyStatistics``
    }
