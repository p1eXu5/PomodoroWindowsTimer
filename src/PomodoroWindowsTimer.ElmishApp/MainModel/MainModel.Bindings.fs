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

        "PlayPauseButtonText"
        |> Binding.oneWay (fun m ->
            match m.LooperState with
            | Playing -> "Stop"
            | _ -> "Play"
        )

        "PlayStopCommand"
        |> Binding.cmd (fun m ->
            match m.LooperState with
            | Playing -> Msg.Stop
            | Stopped -> Msg.Play
        )

        "ActiveTime"
        |> Binding.oneWay (fun m -> m.ActiveTimePoint |> Option.map (fun tp -> tp.TimeSpan) |> Option.defaultValue TimeSpan.Zero )

        "TimePoints" |> Binding.oneWaySeq (
            (fun m -> m.TimePoints),
            (=),
            (fun tp -> tp.Id)
        )

        "MinimizeCommand" |> Binding.cmd Minimize
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


