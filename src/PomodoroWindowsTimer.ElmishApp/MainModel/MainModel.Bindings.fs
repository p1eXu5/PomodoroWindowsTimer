module PomodoroWindowsTimer.ElmishApp.MainModel.Bindings

open Elmish.WPF
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.MainModel
open System
open Elmish.Extensions

let bindings title assemblyVersion errorMessageQueue : Binding<MainModel, Msg> list =
    [
        "Title" |> Binding.oneWay (fun _ -> title)
        "AssemblyVersion" |> Binding.oneWay (fun _ -> assemblyVersion)
        "ErrorMessageQueue" |> Binding.oneWay (fun _ -> errorMessageQueue)

        "LooperIsRunning" |> Binding.oneWay (isLooperRunning)

        "IsPlaying" |> Binding.oneWay (isPlaying)

        "PlayPauseButtonText"
        |> Binding.oneWay (fun m ->
            match m.LooperState with
            | TimeShiftOnStopped _
            | Initialized -> "Play"
            | Playing -> "Stop"
            | Stopped -> "Resume"
        )

        "PlayStopCommand"
        |> Binding.cmd (fun m ->
            match m.LooperState with
            | TimeShiftOnStopped _
            | Initialized -> Msg.Play
            | Playing -> Msg.Stop
            | Stopped -> Msg.Resume
        )

        "NextCommand" |> Binding.cmdIf nextMsg

        "ReplayCommand"
        |> Binding.cmdIf (fun m ->
            match m.LooperState with
            | Playing
            | Stopped ->
                m.ActiveTimePoint
                |> Option.map (fun _ -> Msg.Replay)
            | _ -> None
        )

        "ActiveTime" |> Binding.oneWay getActiveTimeSpan

        "ActiveTimeDuration" |> Binding.oneWay getActiveTimeDuration

        "PreChangeActiveTimeSpanCommand" |> Binding.cmd Msg.PreChangeActiveTimeSpan
        "ActiveTimeSeconds" |> Binding.twoWay (getActiveSpentTime, Msg.ChangeActiveTimeSpan)
        "PostChangeActiveTimeSpanCommand" |> Binding.cmd Msg.PostChangeActiveTimeSpan
        "IsActiveTimePointSet" |> Binding.oneWay (fun m -> m.ActiveTimePoint |> Option.isSome)

        "ActiveTimePointName" |> Binding.oneWayOpt (fun m -> m.ActiveTimePoint |> Option.map (fun tp -> tp.Name))

        "TimePoints" |> Binding.subModelSeq (
            (fun m -> m.TimePoints),
            (fun tp -> tp.Id),
            (fun () -> [
                "Name" |> Binding.oneWay (fun (_, e) -> e.Name)
                "TimeSpan" |> Binding.oneWay (fun (_, e) -> e.TimeSpan.ToString("h':'mm"))
                "Kind" |> Binding.oneWay (fun (_, e) -> e.Kind)
                "KindAlias" |> Binding.oneWay (fun (_, e) -> e.KindAlias |> Alias.value)
                "Id" |> Binding.oneWay (fun (_, e) -> e.Id)
                "IsSelected" |> Binding.oneWay (fun (m, e) -> m.ActiveTimePoint |> Option.map (fun atp -> atp.Id = e.Id) |> Option.defaultValue false)
            ])
        )

        "MinimizeCommand" |> Binding.cmd MinimizeWindows
        "SendToChatBotCommand" |> Binding.cmd SendToChatBot
        "StartTimePointCommand" |> Binding.cmdParam (fun id -> (id :?> Guid) |> Operation.Start |> StartTimePoint)

        "IsBreak"
        |> Binding.oneWay (fun m ->
            m.ActiveTimePoint
            |> Option.map (fun tp -> tp.Kind |> function Kind.Break | Kind.LongBreak -> true | _ -> false)
            |> Option.defaultValue false
        )

        "BotSettingsModel"
            |> Binding.SubModel.opt BotSettingsModel.Bindings.bindings
            |> Binding.mapModel (fun m ->
                m.BotSettingsModel
            )
            |> Binding.mapMsg MainModel.Msg.BotSettingsMsg

        "TimePointsGeneratorModel"
            |> Binding.SubModel.opt TimePointsGeneratorModel.Bindings.bindings
            |> Binding.mapModel _.TimePointsGeneratorModel
            |> Binding.mapMsg MainModel.Msg.TimePointsGeneratorMsg

        "HasTimePointsGenerator" |> Binding.twoWay (_.TimePointsGeneratorModel >> Option.isSome, Msg.EraseTimePointsGeneratorModel)

        "LoadTimePointsGeneratorCommand"
            |> Binding.cmdIf (fun model ->
                match model.TimePointsGeneratorModel with
                | None -> Msg.InitializeTimePointsGeneratorModel |> Some
                | Some _ -> None
            )

        "DisableSkipBreak"
            |> Binding.twoWay (getDisableSkipBreak, Msg.SetDisableSkipBreak)

        "TryStoreAndSetTimePointsCommand"
            |> Binding.cmdIf (fun m ->
                m.TimePointsGeneratorModel
                |> Option.bind (fun tpModel ->
                    if not tpModel.IsPatternWrong then
                        Msg.LoadTimePoints (tpModel.TimePoints |> List.map _.TimePoint) |> Some
                    else
                        None
                )
            )
    ]


