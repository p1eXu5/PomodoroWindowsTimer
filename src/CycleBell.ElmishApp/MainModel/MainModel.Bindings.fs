﻿module CycleBell.ElmishApp.MainModel.Bindings

open Elmish.WPF
open CycleBell.Types
open CycleBell.ElmishApp.Models
open CycleBell.ElmishApp.Models.MainModel
open System

let bindings () : Binding<MainModel, Msg> list =
    [
        "AssemblyVersion" |> Binding.oneWay (fun m -> m.AssemblyVersion)

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
            (fun tp -> tp.Name)
        )

        "MinimizeCommand" |> Binding.cmd Minimize
        "SendToChatBotCommand" |> Binding.cmd SendToChatBot

        "IsBreak"
        |> Binding.oneWay (fun m ->
            m.ActiveTimePoint
            |> Option.map (fun tp -> tp.Kind |> function Kind.Break -> true | _ -> false)
            |> Option.defaultValue false
        )
    ]


