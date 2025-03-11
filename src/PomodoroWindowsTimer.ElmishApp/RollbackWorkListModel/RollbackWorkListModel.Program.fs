namespace PomodoroWindowsTimer.ElmishApp.RollbackWorkListModel

open Elmish.Extensions

open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.RollbackWorkListModel
open PomodoroWindowsTimer.ElmishApp.Abstractions

module Program =

    let private withNoIntent m = (m, Intent.None)

    let update updateRollbackWorkModel msg model =
        match msg with
        | Msg.RollbackWorkModelMsg (workId, smsg) ->
            model.RollbackList
            |> List.mapFirstIntent (_.Work.Id >> (=) workId) (updateRollbackWorkModel smsg) RollbackWorkModel.Intent.None
            |> fst
            |> flip withRollbackList model
            |> withNoIntent

        | Msg.ApplyAndClose ->
            model
            , Intent.ProcessRollbackAndClose

        | Msg.Close ->
            model
            , Intent.Close
