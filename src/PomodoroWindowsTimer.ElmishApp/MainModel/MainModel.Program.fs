module PomodoroWindowsTimer.ElmishApp.MainModel.Program

open Elmish
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.MainModel
open PomodoroWindowsTimer.Looper
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.Types
open System.Threading.Tasks
open Elmish.Extensions
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Types


let update
    (botConfiguration: IBotConfiguration)
    (sendToBot: BotSender)
    (looper: Looper)
    (windowsMinimizer: WindowsMinimizer)
    (msg: Msg) (model: MainModel) =

    match msg with
    | Msg.PickFirstTimePoint ->
        let atp = looper.PickFirst()
        model |> setActiveTimePoint atp, Cmd.none
    
    | Msg.Next ->
        looper.Next()
        model |> setLooperState Playing |> setUIInitiator, Cmd.none

    | Msg.Play ->
        looper.Next()
        model |> setLooperState Playing |> setUIInitiator, Cmd.none

    // TODO: logic can be moved into Msg.LooperMsg handler
    | Msg.Resume when model.ActiveTimePoint |> Option.isSome ->
        looper.Resume()
        let model' = model |> setLooperState Playing

        match model.ActiveTimePoint with
        | Some ({ Kind = Kind.Break }) when not model.IsMinimized ->
            model', Cmd.ofMsg MinimizeWindows

        | Some ({ Kind = Kind.Work }) when model.IsMinimized ->
            model', Cmd.ofMsg RestoreWindows

        | _ ->
            model', Cmd.none

    | Msg.Stop when model.LooperState = LooperState.Playing ->
        looper.Stop()
        model |> setLooperState Stopped
        , Cmd.batch [
            Cmd.ofMsg Msg.RestoreWindows
            Cmd.ofMsg Msg.RestoreMainWindow
        ]

    | Msg.Replay when model.ActiveTimePoint |> Option.isSome ->
        let cmd =
            Cmd.ofMsg (Msg.StartTimePoint (Operation.Start (model.ActiveTimePoint |> Option.get).Id))
        model, cmd

    | Msg.LooperMsg evt ->
        let (activeTimePoint, cmd) =
            match evt with
            | LooperEvent.TimePointTimeReduced tp -> (tp |> Some, Cmd.none)
            | LooperEvent.TimePointStarted (newTp, None) -> (newTp |> Some, Cmd.none)
            | LooperEvent.TimePointStarted (nextTp, Some oldTp) ->
                match nextTp.Kind with
                | Break -> (nextTp |> Some, Cmd.ofMsg MinimizeWindows)
                | Work when model |> isUIInitiator oldTp -> (nextTp |> Some, Cmd.batch [ Cmd.ofMsg RestoreWindows ])
                | Work -> (nextTp |> Some, Cmd.batch [ Cmd.ofMsg RestoreWindows; Cmd.ofMsg SendToChatBot ])

        model |> setActiveTimePoint activeTimePoint, cmd

    | MinimizeWindows when not model.IsMinimized ->
        { model with IsMinimized = true }, Cmd.OfAsync.either windowsMinimizer.Minimize () (fun _ -> Msg.SetIsMinimized true) Msg.OnError

    | RestoreWindows when model.IsMinimized ->
        { model with IsMinimized = false }, Cmd.OfAsync.either windowsMinimizer.Restore () (fun _ -> Msg.SetIsMinimized false) Msg.OnError

    | RestoreMainWindow ->
        model, Cmd.OfAsync.attempt windowsMinimizer.RestoreMainWindow () Msg.OnError

    | SetIsMinimized v ->
        { model with IsMinimized = v }, Cmd.none

    | SendToChatBot ->
        let messageText =
            model.ActiveTimePoint |> Option.map (fun tp -> $"It's time to {tp.Name}!!") |> Option.defaultValue "It's time!!"
        model, Cmd.OfTask.attempt (sendToBot botConfiguration) messageText Msg.OnError

    | StartTimePoint (Operation.Start id) ->
        model, Cmd.batch [ Cmd.ofMsg Stop; Cmd.OfAsync.either looper.TimePointQueue.Scroll id (Operation.Finish >> StartTimePoint) OnError ]

    | StartTimePoint (Operation.Finish _) ->
        model, Cmd.ofMsg Play

    | BotSettingsModelMsg bmsg ->
        let botSettingsModel = BotSettingsModel.Program.update botConfiguration bmsg model.BotSettingsModel
        { model with BotSettingsModel = botSettingsModel }, Cmd.none

    | Msg.OnError ex ->
        model.ErrorQueue.EnqueuError(ex.Message)
        model, Cmd.none

    | _ -> model, Cmd.none
