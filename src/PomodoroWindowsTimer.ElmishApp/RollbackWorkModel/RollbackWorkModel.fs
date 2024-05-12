namespace PomodoroWindowsTimer.ElmishApp.Models

open System
open PomodoroWindowsTimer.Abstractions
open PomodoroWindowsTimer.ElmishApp.Abstractions

type RollbackWorkModel =
    {
        WorkId: uint64
        Time: DateTimeOffset
        Difference: TimeSpan
        RememberChoice: bool
    }

module RollbackWorkModel =

    type Cfg =
        {
            WorkEventRepository: IWorkEventRepository
            UserSettings: IUserSettings
        }

    type Msg =
        | SetRememberChoice of bool
        | SubstractWorkAddBreak
        | Close

    type Intent =
        | None
        | SubstractWorkAddBreakAndClose
        | DefaultedAndClose

    let init workId time timeSpan =
        {
            WorkId = workId
            Time = time
            Difference = timeSpan
            RememberChoice = false
        }


namespace PomodoroWindowsTimer.ElmishApp.RollbackWorkModel

open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.RollbackWorkModel
open PomodoroWindowsTimer.ElmishApp.Abstractions

module Program =

    let update (userSettings: IUserSettings) msg model =

        match msg with
        | Msg.SetRememberChoice v ->
            if not v then
                userSettings.RollbackWorkStrategy <- RollbackWorkStrategy.UserChoiceIsRequired
            { model with RememberChoice = v }, Intent.None

        | Msg.SubstractWorkAddBreak ->
            model
            , Intent.SubstractWorkAddBreakAndClose

        | Msg.Close ->
            model
            , Intent.DefaultedAndClose


module Bindings =

    open Elmish.WPF

    let bindings () =
        [
            "RememberChoice" |> Binding.twoWay (_.RememberChoice, Msg.SetRememberChoice)
            "SubstractWorkAddBreakCommand" |> Binding.cmd Msg.SubstractWorkAddBreak
            "CloseCommand" |> Binding.cmd Msg.Close
        ]

