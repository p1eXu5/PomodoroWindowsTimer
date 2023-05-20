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

let update
    (cfg: MainModeConfig)
    (msg: Msg) (model: MainModel) =

    match msg with
    | Msg.LoadTimePointsFromSettings ->
        let timePoints = cfg.TimePointStore.Read()
        cfg.TimePointQueue.Reload(timePoints)
        { model with TimePoints = timePoints }, Cmd.ofMsg Msg.PickFirstTimePoint

    | Msg.PickFirstTimePoint ->
        cfg.Looper.PreloadTimePoint()
        model, Cmd.none
    
    | Msg.SetActiveTimePoint atp ->
        model |> setActiveTimePoint atp, Cmd.OfFunc.attempt cfg.ThemeSwitcher.SwitchTheme (model |> timePointKindEnum) Msg.OnError

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

    | Msg.LooperMsg evt ->
        let (activeTimePoint, cmd) =
            match evt with
            | LooperEvent.TimePointTimeReduced tp -> (tp |> Some, Cmd.none)
            | LooperEvent.TimePointStarted (newTp, None) -> (newTp |> Some, Cmd.none)
            | LooperEvent.TimePointStarted (nextTp, Some oldTp) ->
                //let breakCmd =
                //    Cmd.batch [
                //        Cmd.ofMsg MinimizeWindows
                //        Cmd.OfAsync.start (
                //            task {
                //                let vm = Binding.SubModelT.
                //                let! _ = cfg.DialogHost.
                //            }
                //        )
                //    ]
                match nextTp.Kind with
                | LongBreak -> (nextTp |> Some, Cmd.ofMsg MinimizeWindows)
                | Break -> (nextTp |> Some, Cmd.ofMsg MinimizeWindows)
                | Work when model |> isUIInitiator oldTp -> (nextTp |> Some, Cmd.batch [ Cmd.ofMsg RestoreWindows ])
                | Work -> (nextTp |> Some, Cmd.batch [ Cmd.ofMsg RestoreWindows; Cmd.ofMsg SendToChatBot ])

        model, Cmd.batch [ Cmd.ofMsg (Msg.SetActiveTimePoint activeTimePoint); cmd ]

    | MinimizeWindows when not model.IsMinimized ->
        { model with IsMinimized = true }, Cmd.OfAsync.either cfg.WindowsMinimizer.Minimize model.Title (fun _ -> Msg.SetIsMinimized true) Msg.OnError

    | RestoreWindows when model.IsMinimized ->
        { model with IsMinimized = false }, Cmd.OfAsync.either cfg.WindowsMinimizer.Restore () (fun _ -> Msg.SetIsMinimized false) Msg.OnError

    | RestoreMainWindow ->
        model, Cmd.OfAsync.attempt cfg.WindowsMinimizer.RestoreMainWindow model.Title Msg.OnError

    | SetIsMinimized v ->
        { model with IsMinimized = v }, Cmd.none

    | SendToChatBot ->
        let messageText =
            model.ActiveTimePoint |> Option.map (fun tp -> $"It's time to {tp.Name}!!") |> Option.defaultValue "It's time!!"
        model, Cmd.OfTask.attempt (cfg.SendToBot cfg.BotSettings) messageText Msg.OnError

    | StartTimePoint (Operation.Start id) ->
        model, Cmd.batch [ Cmd.ofMsg Stop; Cmd.OfAsync.either cfg.Looper.TimePointQueue.Scroll id (Operation.Finish >> StartTimePoint) OnError ]

    | StartTimePoint (Operation.Finish _) ->
        model, Cmd.ofMsg Play

    | BotSettingsMsg bmsg ->
        let settingsModel = BotSettingsModel.Program.update cfg.BotSettings bmsg model.BotSettingsModel
        { model with BotSettingsModel = settingsModel }, Cmd.none

    | TimePointsSettingsMsg tpmsg ->
        let (tpSettingsModel, tpSettingsModelCmd) = TimePointsGenerator.Program.update tpmsg model.TimePointsGeneratorModel
        { model with TimePointsGeneratorModel = tpSettingsModel}, Cmd.map TimePointsSettingsMsg tpSettingsModelCmd

    | SetDisableSkipBreak v ->
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
        cfg.Looper.Shift(duration - v)
        model, Cmd.none

    | Msg.PostChangeActiveTimeSpan ->
        match model.LooperState with
        | TimeShiftOnStopped s ->
            { model with LooperState = s }, Cmd.none
        | _ ->
            cfg.Looper.Resume()
            model, Cmd.none

    // --------------------
    | Msg.TryStoreAndSetTimePoints ->
        cfg.Looper.Stop()
        let timePointsSettingsModel = model.TimePointsGeneratorModel
        cfg.TimePointQueue.Reload(timePointsSettingsModel.TimePoints)
        let patterns = timePointsSettingsModel.SelectedPattern |> Option.get |> (fun p -> p :: timePointsSettingsModel.Patterns) |> List.distinct
        cfg.PatternSettings.Write(patterns)
        cfg.TimePointPrototypeStore.Write(timePointsSettingsModel.TimePointPrototypes)
        { model with TimePoints = timePointsSettingsModel.TimePoints }, Cmd.batch [
            Cmd.ofMsg Msg.PickFirstTimePoint

            TimePointsGenerator.Msg.SetPatterns patterns
            |> Msg.TimePointsSettingsMsg
            |> Cmd.ofMsg
        ]

    | Msg.OnError ex ->
        model.ErrorQueue.EnqueuError(ex.Message)
        model, Cmd.none

    | _ -> model, Cmd.none
