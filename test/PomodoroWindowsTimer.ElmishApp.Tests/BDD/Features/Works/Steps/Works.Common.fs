[<RequireQualifiedAccess>]
module rec PomodoroWindowsTimer.ElmishApp.Tests.Features.Works.Steps.Common

open Elmish.Extensions

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Tests.ScenarioCE
open PomodoroWindowsTimer.ElmishApp.Tests.Features.Helpers

let ``SetCurrentWorkIfNone msg has been dispatched with`` (expectedWork: Work) =
    scenario { 
        do! Scenario.msgDispatchedWithin2Sec "SetCurrentWorkIfNone" (fun msg ->
            match msg with
            | MainModel.Msg.CurrentWorkModelMsg (CurrentWorkModel.Msg.SetCurrentWorkIfNone (Ok work)) ->
                work.Number = expectedWork.Number && work.Title = expectedWork.Title
            | _ -> false
        )
    }
    |> Scenario.log "Common.``SetCurrentWorkIfNone msg has been dispatched with``"

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
    |> Scenario.log "Common.``CreatingWork sub model has been shown``"

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
    |> Scenario.log "Common.``WorkList sub model has been shown``"


let ``UpdatingWork sub model has been shown`` () =
    scenario {
        do! Scenario.modelSatisfiesWithin2Sec "UpdatingWork selector sub model expected" (fun mainModel ->
            match mainModel.WorkSelector with
            | Some workSelectorModel ->
                match workSelectorModel.SubModel with
                | WorkSelectorSubModel.UpdatingWork _ -> true
                | _ -> false
            | _ -> false
        )
    }
    |> Scenario.log "Common.``UpdatingWork sub model has been shown``"
