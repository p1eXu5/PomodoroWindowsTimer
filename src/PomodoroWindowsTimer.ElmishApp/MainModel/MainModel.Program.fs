module PomodoroWindowsTimer.ElmishApp.MainModel.Program

open Elmish
open Elmish.Extensions
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Looper
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Infrastructure
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.MainModel
open Elmish.WPF
open Elmish.WPF.Extensions
open Elmish.WPF.Extensions

let update
    (cfg: MainModeConfig)
    initBotSettingsModel
    updateBotSettingsModel
    updateTimePointGeneratorModel
    initTimePointGeneratorModel
    (errorMessageQueue: IErrorMessageQueue)
    (msg: Msg)
    (model: MainModel)
    =
    match msg with
    | Msg.LoadTimePointsFromSettings ->
        let timePoints = cfg.TimePointStore.Read()
        cfg.TimePointQueue.Reload(timePoints)
        cfg.Looper.PreloadTimePoint()
        { model with TimePoints = timePoints }, Cmd.none

    | Msg.LooperMsg evt ->
        let (activeTimePoint, cmd, switchTheme) =
            match evt with
            | LooperEvent.TimePointTimeReduced tp -> (tp |> Some, Cmd.none, false)
            | LooperEvent.TimePointStarted (newTp, None) -> (newTp |> Some, Cmd.none, true)
            | LooperEvent.TimePointStarted (nextTp, Some oldTp) ->
                match nextTp.Kind with
                | LongBreak -> (nextTp |> Some, Cmd.ofMsg MinimizeWindows, true)
                | Break -> (nextTp |> Some, Cmd.ofMsg MinimizeWindows, true)
                | Work when model |> isUIInitiator oldTp -> (nextTp |> Some, Cmd.batch [ Cmd.ofMsg RestoreWindows ], true)
                | Work -> (nextTp |> Some, Cmd.batch [ Cmd.ofMsg RestoreWindows; Cmd.ofMsg SendToChatBot ], true)

        let model = model |> setActiveTimePoint activeTimePoint
        model
        , Cmd.batch [
            cmd
            if switchTheme then
                Cmd.OfFunc.attempt cfg.ThemeSwitcher.SwitchTheme (model |> timePointKindEnum) Msg.OnError
        ]

    | Msg.Next ->
        cfg.Looper.Next()
        model |> setLooperState Playing |> setUIInitiator, Cmd.none

    | Msg.Play ->
        cfg.Looper.Next()
        model |> setLooperState Playing |> setUIInitiator, Cmd.none

    // TODO: logic can be moved into Msg.LooperMsg handler
    | Msg.Resume when model.ActiveTimePoint |> Option.isSome ->
        cfg.Looper.Resume()
        let model' = model |> setLooperState Playing

        match model.ActiveTimePoint with
        | Some ({ Kind = Kind.Break }) when not model.IsMinimized ->
            model', Cmd.ofMsg MinimizeWindows

        | Some ({ Kind = Kind.Work }) when model.IsMinimized ->
            model', Cmd.ofMsg RestoreWindows

        | _ ->
            model', Cmd.none

    | Msg.Stop when model.LooperState = LooperState.Playing ->
        cfg.Looper.Stop()
        model |> setLooperState Stopped
        , Cmd.batch [
            Cmd.ofMsg Msg.RestoreWindows
            Cmd.ofMsg Msg.RestoreMainWindow
        ]

    | Msg.Replay when model.ActiveTimePoint |> Option.isSome ->
        let cmd =
            Cmd.batch [Cmd.ofMsg Msg.PreChangeActiveTimeSpan; Cmd.ofMsg (ChangeActiveTimeSpan 0.0); Cmd.ofMsg Msg.Resume]
        model, cmd
    | MinimizeWindows when not model.IsMinimized ->
        { model with IsMinimized = true }, Cmd.OfAsync.either cfg.WindowsMinimizer.MinimizeOther () (fun _ -> Msg.SetIsMinimized true) Msg.OnError

    | RestoreWindows when model.IsMinimized ->
        { model with IsMinimized = false }, Cmd.OfAsync.either cfg.WindowsMinimizer.Restore () (fun _ -> Msg.SetIsMinimized false) Msg.OnError

    | RestoreMainWindow ->
        model, Cmd.OfAsync.attempt cfg.WindowsMinimizer.RestoreMainWindow () Msg.OnError

    | SetIsMinimized v ->
        { model with IsMinimized = v }, Cmd.none

    | SendToChatBot ->
        let messageText =
            model.ActiveTimePoint |> Option.map (fun tp -> $"It's time to {tp.Name}!!") |> Option.defaultValue "It's time!!"
        model, Cmd.OfTask.attempt cfg.SendToBot messageText Msg.OnError

    | StartTimePoint (Operation.Start id) ->
        model, Cmd.batch [ Cmd.ofMsg Stop; Cmd.OfFunc.either cfg.Looper.TimePointQueue.Scroll id (Operation.Finish >> StartTimePoint) OnError ]

    | StartTimePoint (Operation.Finish _) ->
        model, Cmd.ofMsg Play

    | Msg.InitializeTimePointsGeneratorModel ->
        let (genModel, genCmd) = initTimePointGeneratorModel ()
        { model with TimePointsGeneratorModel = genModel |> Some }
        , Cmd.map TimePointsGeneratorMsg genCmd

    | Msg.EraseTimePointsGeneratorModel isDrawerOpen when not isDrawerOpen ->
         {model with TimePointsGeneratorModel = None }, Cmd.none

    | MsgWith.TimePointsGeneratorMsg model (genMsg, genModel) ->
        let (genModel, genCmd, intent) = updateTimePointGeneratorModel errorMessageQueue genMsg genModel
        let model' = { model with TimePointsGeneratorModel = genModel |> Some }
        let cmd' = Cmd.map TimePointsGeneratorMsg genCmd
        match intent with
        | Intent.Request (TimePointsGeneratorModel.Request.ApplyGeneratedTimePoints) ->
            model', Cmd.batch [cmd'; Cmd.ofMsg (Msg.LoadTimePoints (genModel.TimePoints |> List.map _.TimePoint))]
        | Intent.None ->
            model', cmd'

    | Msg.SetDisableSkipBreak v ->
        cfg.DisableSkipBreakSettings.DisableSkipBreak <- v
        { model with DisableSkipBreak = v }, Cmd.none

    // --------------------
    // Active time changing
    // --------------------
    | Msg.PreChangeActiveTimeSpan ->
        match model.LooperState with
        | Playing ->
            cfg.Looper.Stop()
            model, Cmd.none
        | _ ->
            { model with LooperState = TimeShiftOnStopped model.LooperState }, Cmd.none

    | Msg.ChangeActiveTimeSpan v ->
        let duration = model |> getActiveTimeDuration
        cfg.Looper.Shift((duration - v) * 1.0<sec>)
        model, Cmd.none

    | Msg.PostChangeActiveTimeSpan ->
        match model.LooperState with
        | TimeShiftOnStopped s ->
            { model with LooperState = s }, Cmd.none
        | _ ->
            cfg.Looper.Resume()
            model, Cmd.none

    // --------------------
    | Msg.LoadTimePoints timePoints ->
        cfg.Looper.Stop()
        cfg.TimePointQueue.Reload(timePoints)
        cfg.TimePointStore.Write(timePoints)
        cfg.Looper.PreloadTimePoint()

        { model with TimePoints = timePoints }
        , Cmd.none

    | Msg.LoadBotSettingsModel ->
        { model with BotSettingsModel = initBotSettingsModel () }
        , Cmd.none

    | MsgWith.BotSettingsMsg model (bmsg, bModel) ->
        let bModel, intent = updateBotSettingsModel bmsg bModel
        match intent with
        | BotSettingsModel.Intent.None ->
            { model with BotSettingsModel = bModel |> Some }, Cmd.none
        | BotSettingsModel.Intent.CloseDialogRequested ->
            { model with BotSettingsModel = None }, Cmd.none

    | Msg.OnError ex ->
        errorMessageQueue.EnqueueError ex.Message
        model, Cmd.none

    | _ -> model, Cmd.none
