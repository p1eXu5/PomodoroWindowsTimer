namespace PomodoroWindowsTimer.ElmishApp.Models

type RollbackWorkListModel =
    {
        RollbackList: RollbackWorkModel list
    }

module RollbackWorkListModel =

    open PomodoroWindowsTimer.Types

    type Msg =
        | RollbackWorkModelMsg of WorkId * RollbackWorkModel.Msg
        | Close

    //module MsgWith =

    //    let (|RollbackWorkModelMsg|_|) model msg =
    //        match msg, model.RollbackList |> 


    [<RequireQualifiedAccess>]
    type Intent =
        | None
        | ProcessRollbackAndClose
        | DefaultedAndClose

    let init workSpenTimeList time localStrategy =
        {
            RollbackList = workSpenTimeList |> List.map (fun wst -> RollbackWorkModel.init wst time localStrategy)
        }

    let withRollbackList rollbackList (model: RollbackWorkListModel) =
        { model with RollbackList = rollbackList }

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
            |> List.mapFirstIntent (_.WorkId >> (=) workId) (updateRollbackWorkModel smsg) RollbackWorkModel.Intent.None
            |> fst
            |> flip withRollbackList model
            |> withNoIntent

        | Msg.Close ->
            model
            , Intent.DefaultedAndClose


module Bindings =

    open Elmish.WPF

    let bindings () =
        [
            // "RememberChoice" |> Binding.twoWay (_.RememberChoice, Msg.SetRememberChoice)
            // "SubstractWorkAddBreakCommand" |> Binding.cmd Msg.SubstractWorkAddBreak
            // "CloseCommand" |> Binding.cmd Msg.Close
        ]

