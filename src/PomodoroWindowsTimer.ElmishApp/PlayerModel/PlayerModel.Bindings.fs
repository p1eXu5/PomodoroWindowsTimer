namespace PomodoroWindowsTimer.ElmishApp.PlayerModel

open Elmish.WPF
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.PlayerModel
open System
open Elmish.Extensions

open Elmish.WPF
open Elmish.Extensions
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.Abstractions

type private Binding = Binding<PlayerModel, PlayerModel.Msg>

[<Sealed>]
type Bindings() =
    static let props = Utils.bindingProperties typeof<Bindings>
    static let mutable __ = Unchecked.defaultof<Bindings>
    static member Instance () =
        if System.Object.ReferenceEquals(__, null) then
            __ <- Bindings()
            __
        else __

    static member ToList () =
        Utils.bindings<Binding>
            (Bindings.Instance ())
            props

    member val LooperIsRunning : Binding =
        nameof __.LooperIsRunning |> Binding.oneWay isLooperStateIsNotInitialized

    member val IsPlaying : Binding =
        nameof __.IsPlaying |> Binding.oneWay isPlaying

    member val PlayPauseButtonText : Binding =
        nameof __.PlayPauseButtonText
            |> Binding.oneWay (fun m ->
                match m.LooperState with
                | TimeShifting _
                | Initialized -> "Play"
                | Playing -> "Stop"
                | Stopped -> "Resume"
            )

    member val PlayStopCommand : Binding =
        nameof __.PlayStopCommand |> Binding.cmd Msg.playStopResume

    member val NextCommand : Binding =
        nameof __.NextCommand |> Binding.cmdIf Msg.tryNext

    member val ReplayCommand : Binding =
        nameof __.ReplayCommand
            |> Binding.cmdIf (fun m ->
                match m.LooperState with
                | Playing
                | Stopped ->
                    m.ActiveTimePoint
                    |> Option.map (fun _ -> Msg.Replay)
                | _ -> None
            )

    // ----------------------------------------------------- ActiveTimePoint
    member val ActiveTimePointName : Binding =
        nameof __.ActiveTimePointName |> Binding.oneWayOpt (fun m -> m.ActiveTimePoint |> Option.map (fun tp -> tp.Name))

    member val ActiveTime : Binding =
        nameof __.ActiveTime |> Binding.oneWay getActiveTimeSpan

    member val ActiveTimeDuration : Binding =
        nameof __.ActiveTimeDuration |> Binding.oneWay (getActiveTimeDuration)

    member val ActiveTimeKind : Binding =
        nameof __.ActiveTimeKind |> Binding.oneWay (getActiveTimeKind)

    member val IsActiveTimePointSet : Binding =
        nameof __.IsActiveTimePointSet |> Binding.oneWay (fun m -> m.ActiveTimePoint |> Option.isSome)

    member val CanShiftActiveTime : Binding =
        nameof __.CanShiftActiveTime |> Binding.oneWay (canShiftActiveTime)

    member val ActiveTimeSeconds : Binding =
        nameof __.ActiveTimeSeconds |> Binding.twoWay (getActiveSpentTime, Msg.ChangeActiveTimeSpan)

    member val PreChangeActiveTimeSpanCommand : Binding =
        nameof __.PreChangeActiveTimeSpanCommand |> Binding.cmd (Msg.PreChangeActiveTimeSpan)

    member val PostChangeActiveTimeSpanCommand : Binding =
        nameof __.PostChangeActiveTimeSpanCommand |> Binding.cmd (Msg.PostChangeActiveTimeSpan (AsyncOperation.Start ()))

    member val IsBreak : Binding =
        nameof __.IsBreak
            |> Binding.oneWay (fun m ->
                m.ActiveTimePoint
                |> Option.map (fun tp -> tp.Kind |> function Kind.Break | Kind.LongBreak -> true | _ -> false)
                |> Option.defaultValue false
            )

    member val DisableSkipBreak : Binding =
        nameof __.DisableSkipBreak |> Binding.twoWay (_.DisableSkipBreak, Msg.SetDisableSkipBreak)

    member val DisableMinimizeMaximizeWindows : Binding =
        nameof __.DisableMinimizeMaximizeWindows |> Binding.twoWay (_.DisableMinimizeMaximizeWindows, Msg.SetDisableMinimizeMaximizeWindows)
