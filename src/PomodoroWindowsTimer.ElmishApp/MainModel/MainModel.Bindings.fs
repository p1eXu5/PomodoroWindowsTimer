module PomodoroWindowsTimer.ElmishApp.MainModel.Bindings

open Elmish.WPF
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.MainModel
open System
open Elmish.Extensions

let bindings () : Binding<MainModel, Msg> list =
    [
        "AssemblyVersion" |> Binding.oneWay (fun m -> m.AssemblyVersion)
        "ErrorMessageQueue" |> Binding.oneWay (fun m -> m.ErrorQueue)

        "LooperIsRunning" |> Binding.oneWay (isLooperRunning)

        "PlayPauseButtonText"
        |> Binding.oneWay (fun m ->
            match m.LooperState with
            | Initialized -> "Play"
            | Playing -> "Stop"
            | Stopped -> "Resume"
        )

        "PlayStopCommand"
        |> Binding.cmd (fun m ->
            match m.LooperState with
            | Initialized -> Msg.Play
            | Playing -> Msg.Stop
            | Stopped -> Msg.Resume
        )

        "NextCommand" |> Binding.cmd Msg.Next

        "ReplayCommand"
        |> Binding.cmdIf (fun m ->
            match m.LooperState with
            | Playing
            | Stopped ->
                m.ActiveTimePoint
                |> Option.map (fun _ -> Msg.Replay)
            | _ -> None
        )

        "ActiveTime"
        |> Binding.oneWay (fun m -> m.ActiveTimePoint |> Option.map (fun tp -> tp.TimeSpan) |> Option.defaultValue TimeSpan.Zero )

        "ActiveTimePointName" |> Binding.oneWayOpt (fun m -> m.ActiveTimePoint |> Option.map (fun tp -> tp.Name))

        "TimePoints" |> Binding.oneWaySeq (
            (fun m -> m.TimePoints),
            (=),
            (fun tp -> tp.Id)
        )

        "MinimizeCommand" |> Binding.cmd MinimizeWindows
        "SendToChatBotCommand" |> Binding.cmd SendToChatBot
        "StartTimePointCommand" |> Binding.cmdParam (fun id -> (id :?> Guid) |> Operation.Start |> StartTimePoint)

        "IsBreak"
        |> Binding.oneWay (fun m ->
            m.ActiveTimePoint
            |> Option.map (fun tp -> tp.Kind |> function Kind.Break -> true | _ -> false)
            |> Option.defaultValue false
        )

        "BotSettingsModel"
            |> Binding.SubModel.required BotSettingsModel.Bindings.bindings
            |> Binding.mapModel (fun m ->
                m.BotSettingsModel
            )
            |> Binding.mapMsg MainModel.Msg.BotSettingsModelMsg
    ]


