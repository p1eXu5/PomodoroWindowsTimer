[<RequireQualifiedAccess>]
module rec PomodoroWindowsTimer.ElmishApp.Tests.Features.Works.Steps.Common

open System
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.MainModel
open PomodoroWindowsTimer.ElmishApp.Tests
open PomodoroWindowsTimer.ElmishApp.Tests.ScenarioCE
open PomodoroWindowsTimer.ElmishApp.Tests.Features.Helpers
open Elmish.Extensions

let ``SetCurrentWorkIfNone msg has been dispatched with`` (expectedWork: Work) =
    scenario { 
        do! Scenario.msgDispatchedWithin2Sec "SetCurrentWorkIfNone" (fun msg ->
            match msg with
            | MainModel.Msg.SetCurrentWorkIfNone (Ok work) ->
                work.Number = expectedWork.Number && work.Title = expectedWork.Title
            | _ -> false
        )
    }


let ``CreatingWork sub model has been shown`` () =
    scenario {
        do! Scenario.modelSatisfiesWithin2Sec "CreatingWork selector sub model expected" (fun mainModel ->
            match mainModel.WorkSelector with
            | Some workSelectorModel ->
                match workSelectorModel.SubModel with
                | WorkSelectorSubModel.CreatingWork _ -> true
                | _ -> false
            | _ -> false
        )
    }

let ``WorkList sub model has been shown`` (times: int<times>) =
    scenario {
        do! Scenario.modelSatisfiesWithin2Sec "WorkList selector sub model expected" (fun mainModel ->
            match mainModel.WorkSelector with
            | Some workSelectorModel ->
                match workSelectorModel.SubModel with
                | WorkSelectorSubModel.WorkList _ -> true
                | _ -> false
            | _ -> false
        )

        do! Scenario.msgDispatchedWithin2SecT "LoadWorkList Finish" times (function
            | MainModel.Msg.WorkSelectorModelMsg (
                WorkSelectorModel.Msg.WorkListModelMsg (
                    WorkListModel.Msg.LoadWorkList (AsyncOperation.Finish _)
                )) -> true
            | _ -> false
        )
    }
