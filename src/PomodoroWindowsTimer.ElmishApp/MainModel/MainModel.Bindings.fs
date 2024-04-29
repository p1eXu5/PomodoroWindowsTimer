namespace PomodoroWindowsTimer.ElmishApp.MainModel

open Elmish.WPF
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.MainModel
open System
open Elmish.Extensions

open Elmish.WPF
open Elmish.Extensions
open PomodoroWindowsTimer.ElmishApp.Abstractions

type private Binding = Binding<MainModel, MainModel.Msg>

[<Sealed>]
type Bindings(title: string, assemblyVersion: string, mainErrorMessageQueue: IErrorMessageQueue, dialogErrorMessageQueue: IErrorMessageQueue) =
    static let props = Utils.bindingProperties typeof<Bindings>
    static let mutable __ = Unchecked.defaultof<Bindings>
    static member Instance title assemblyVersion mainErrorMessageQueue dialogErrorMessageQueue =
        if System.Object.ReferenceEquals(__, null) then
            __ <- Bindings(title, assemblyVersion, mainErrorMessageQueue, dialogErrorMessageQueue)
            __
        else __

    static member ToList title assemblyVersion mainErrorMessageQueue dialogErrorMessageQueue =
        Utils.bindings<Binding>
            (Bindings.Instance title assemblyVersion mainErrorMessageQueue dialogErrorMessageQueue)
            props

    member val Title : Binding =
        nameof __.Title |> Binding.oneWay (fun _ -> title)

    member val AssemblyVersion : Binding =
        nameof __.AssemblyVersion |> Binding.oneWay (fun _ -> assemblyVersion)

    member val ErrorMessageQueue : Binding =
        nameof __.ErrorMessageQueue |> Binding.oneWay (fun _ -> mainErrorMessageQueue)

    // -------------------------------------------------------------
    member val LooperIsRunning : Binding =
        nameof __.LooperIsRunning |> Binding.oneWay isLooperRunning

    member val IsPlaying : Binding =
        nameof __.IsPlaying |> Binding.oneWay isPlaying

    member val PlayPauseButtonText : Binding =
        nameof __.PlayPauseButtonText
            |> Binding.oneWay (fun m ->
                match m.LooperState with
                | TimeShiftOnStopped _
                | Initialized -> "Play"
                | Playing -> "Stop"
                | Stopped -> "Resume"
            )

    member val PlayStopCommand : Binding =
        nameof __.PlayStopCommand
            |> Binding.cmd (fun m ->
                match m.LooperState with
                | TimeShiftOnStopped _
                | Initialized -> PlayerMsg.Play |> Msg.PlayerMsg
                | Playing -> PlayerMsg.Stop |> Msg.PlayerMsg
                | Stopped -> PlayerMsg.Resume |> Msg.PlayerMsg
            )

    member val NextCommand : Binding =
        nameof __.NextCommand |> Binding.cmdIf Msg.tryNext

    member val ReplayCommand : Binding =
        nameof __.ReplayCommand
            |> Binding.cmdIf (fun m ->
                match m.LooperState with
                | Playing
                | Stopped ->
                    m.ActiveTimePoint
                    |> Option.map (fun _ -> PlayerMsg.Replay |> Msg.PlayerMsg)
                | _ -> None
            )

    // ----------------------------------------------------- ActiveTimePoint
    member val ActiveTimePointName : Binding =
        nameof __.ActiveTimePointName |> Binding.oneWayOpt (fun m -> m.ActiveTimePoint |> Option.map (fun tp -> tp.Name))

    member val ActiveTime : Binding =
        nameof __.ActiveTime |> Binding.oneWay getActiveTimeSpan

    member val ActiveTimeDuration : Binding =
        nameof __.ActiveTimeDuration |> Binding.oneWay getActiveTimeDuration

    member val IsActiveTimePointSet : Binding =
        nameof __.IsActiveTimePointSet |> Binding.oneWay (fun m -> m.ActiveTimePoint |> Option.isSome)

    member val ActiveTimeSeconds : Binding =
        nameof __.ActiveTimeSeconds |> Binding.twoWay (getActiveSpentTime, Msg.ChangeActiveTimeSpan)

    member val PreChangeActiveTimeSpanCommand : Binding =
        nameof __.PreChangeActiveTimeSpanCommand |> Binding.cmd Msg.PreChangeActiveTimeSpan

    member val PostChangeActiveTimeSpanCommand : Binding =
        nameof __.PostChangeActiveTimeSpanCommand |> Binding.cmd Msg.PostChangeActiveTimeSpan

    member val IsBreak : Binding =
        nameof __.IsBreak
            |> Binding.oneWay (fun m ->
                m.ActiveTimePoint
                |> Option.map (fun tp -> tp.Kind |> function Kind.Break | Kind.LongBreak -> true | _ -> false)
                |> Option.defaultValue false
            )

    // ----------------------------------------------------
    member val TimePoints : Binding =
        nameof __.TimePoints
            |> Binding.subModelSeq (
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

    member val StartTimePointCommand : Binding =
        nameof __.StartTimePointCommand |> Binding.cmdParam (fun id -> (id :?> Guid) |> Operation.Start |> StartTimePoint)

    member val MinimizeCommand : Binding =
        nameof __.MinimizeCommand |> Binding.cmd (WindowsMsg.MinimizeWindows |> Msg.WindowsMsg)

    member val SendToChatBotCommand : Binding =
        nameof __.SendToChatBotCommand |> Binding.cmd SendToChatBot

    member val DisableSkipBreak : Binding =
        nameof __.DisableSkipBreak |> Binding.twoWay (_.DisableSkipBreak, Msg.SetDisableSkipBreak)

    member val DisableMinimizeMaximizeWindows : Binding =
        nameof __.DisableMinimizeMaximizeWindows |> Binding.twoWay (_.DisableMinimizeMaximizeWindows, Msg.SetDisableMinimizeMaximizeWindows)

    member val IsTimePointsShown : Binding =
        nameof __.IsTimePointsShown |> Binding.twoWay (_.IsTimePointsShown, Msg.SetIsTimePointsShown)

    // ------------------------------------------------------

    member val CurrentWork : Binding =
        nameof __.CurrentWork
            |> Binding.SubModel.opt (WorkModel.Bindings.ToList)
            |> Binding.mapModel _.Work
            |> Binding.mapMsg Msg.WorkModelMsg

    member val IsCurrentWorkSet : Binding =
        nameof __.IsCurrentWorkSet |> Binding.oneWay (_.Work >> Option.isSome)

    // ------------------------------------------------------
    member val AppDialog : Binding =
        nameof __.AppDialog
            |> Binding.SubModel.required (fun () -> AppDialogModel.Bindings.ToList(dialogErrorMessageQueue))
            |> Binding.mapModel _.AppDialog
            |> Binding.mapMsg Msg.AppDialogModelMsg

    member val WorkSelector : Binding =
        nameof __.WorkSelector
            |> Binding.SubModel.opt (WorkSelectorModel.Bindings.ToList)
            |> Binding.mapModel _.WorkSelector
            |> Binding.mapMsg Msg.WorkSelectorModelMsg

    member val IsWorkSelectorLoaded : Binding =
        nameof __.IsWorkSelectorLoaded |> Binding.twoWay (_.WorkSelector >> Option.isSome, Msg.SetIsWorkSelectorLoaded)


(*
let bindings title assemblyVersion errorMessageQueue : Binding<MainModel, Msg> list =
    [
        // "BotSettingsModel"
        //     |> Binding.SubModel.opt BotSettingsModel.Bindings.bindings
        //     |> Binding.mapModel _.BotSettingsModel
        //     |> Binding.mapMsg MainModel.Msg.BotSettingsMsg
        // 
        // "TimePointsGeneratorModel"
        //     |> Binding.SubModel.opt TimePointsGeneratorModel.Bindings.bindings
        //     |> Binding.mapModel _.TimePointsGeneratorModel
        //     |> Binding.mapMsg MainModel.Msg.TimePointsGeneratorMsg
        // 
        // "HasTimePointsGenerator" |> Binding.twoWay (_.TimePointsGeneratorModel >> Option.isSome, Msg.EraseTimePointsGeneratorModel)

        // "LoadTimePointsGeneratorCommand"
        //     |> Binding.cmdIf (fun model ->
        //         match model.TimePointsGeneratorModel with
        //         | None -> Msg.InitializeTimePointsGeneratorModel |> Some
        //         | Some _ -> None
        //     )

        // "TryStoreAndSetTimePointsCommand"
        //     |> Binding.cmdIf (fun m ->
        //         m.TimePointsGeneratorModel
        //         |> Option.bind (fun tpModel ->
        //             if not tpModel.IsPatternWrong then
        //                 Msg.LoadTimePoints (tpModel.TimePoints |> List.map _.TimePoint) |> Some
        //             else
        //                 None
        //         )
        //     )

        // "IsDialogOpened" |> Binding.oneWay (_.BotSettingsModel >> Option.isSome)

        // "LoadBotSettingsModelCommand" |> Binding.cmd Msg.LoadBotSettingsModel
    ]
*)


