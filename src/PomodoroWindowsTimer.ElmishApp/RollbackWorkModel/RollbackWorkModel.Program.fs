module PomodoroWindowsTimer.ElmishApp.RollbackWorkModel.Program

open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.RollbackWorkModel
open PomodoroWindowsTimer.ElmishApp.Abstractions


let update (userSettings: IUserSettings) msg model =
    match msg with
    //| Msg.SetRememberChoice v ->
    //    if not v then
    //        userSettings.RollbackWorkStrategy <- RollbackWorkStrategy.UserChoiceIsRequired
    //    { model with RememberChoice = v }, Intent.None

    | Msg.SubstractWorkAddBreak ->
        model
        , Intent.SubstractWorkAddBreakAndClose

    | Msg.Close ->
        model
        , Intent.DefaultedAndClose


