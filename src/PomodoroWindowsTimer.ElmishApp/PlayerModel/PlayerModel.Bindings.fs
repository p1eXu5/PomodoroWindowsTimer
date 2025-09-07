namespace PomodoroWindowsTimer.ElmishApp.PlayerModel

open System
open Elmish.WPF
open Elmish.Extensions

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.PlayerModel
open System.Windows.Input


type private Binding = Binding<PlayerModel, PlayerModel.Msg>

/// Design time bindings
type IBindings =
    interface
        abstract LooperIsRunning: bool
        abstract IsPlaying : bool
        abstract IsBreak : bool
        abstract IsActiveTimePointSet : bool
        abstract CanShiftActiveTime : bool
        abstract DisableSkipBreak : bool with get, set
        abstract DisableMinimizeMaximizeWindows : bool with get, set

        abstract PlayPauseButtonText : string
        abstract ActiveTimePointName : string
        abstract ActiveTime : TimeSpan
        abstract ActiveTimeDuration : float
        abstract ActiveTimeKind : Kind
        abstract ActiveTimeSeconds : float with get, set
        
        abstract PlayStopCommand : ICommand
        abstract NextCommand : ICommand
        abstract ReplayCommand : ICommand
        abstract PreChangeActiveTimeSpanCommand : ICommand
        abstract PostChangeActiveTimeSpanCommand : ICommand
    end


module Bindings =
    let private __ = Unchecked.defaultof<IBindings>

    let bindings () =
        [
            nameof __.LooperIsRunning
                |> Binding.oneWay isLooperStateIsNotInitialized

            nameof __.IsPlaying
                |> Binding.oneWay isPlaying

            nameof __.PlayPauseButtonText
                |> Binding.oneWay (fun m ->
                    match m.LooperState with
                    | TimeShifting _
                    | Initialized -> "Play"
                    | Playing -> "Stop"
                    | Stopped -> "Resume"
                )

            nameof __.PlayStopCommand
                |> Binding.cmd Msg.playStopResume

            nameof __.NextCommand
                |> Binding.cmdIf Msg.tryNext

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
            nameof __.ActiveTimePointName
                |> Binding.oneWayOpt (fun m -> m.ActiveTimePoint |> Option.map (fun tp -> tp.Name))

            nameof __.ActiveTime
                |> Binding.oneWay getRemainingTimeSpan

            nameof __.ActiveTimeDuration
                |> Binding.oneWay (getActiveTimeDuration)

            nameof __.ActiveTimeKind
                |> Binding.oneWay (getActiveTimeKind)

            nameof __.IsActiveTimePointSet
                |> Binding.oneWay (fun m -> m.ActiveTimePoint |> Option.isSome)

            nameof __.CanShiftActiveTime
                |> Binding.oneWay (canShiftActiveTime)

            nameof __.ActiveTimeSeconds
                |> Binding.twoWay (getActiveSpentTime, Msg.ChangeActiveTimeSpan)

            nameof __.PreChangeActiveTimeSpanCommand
                |> Binding.cmd (Msg.PreChangeActiveTimeSpan)

            nameof __.PostChangeActiveTimeSpanCommand
                |> Binding.cmd (Msg.PostChangeActiveTimeSpan (AsyncOperation.Start ()))

            nameof __.IsBreak
                |> Binding.oneWay (fun m ->
                    m.ActiveTimePoint
                    |> Option.map (fun tp -> tp.Kind |> function Kind.Break | Kind.LongBreak -> true | _ -> false)
                    |> Option.defaultValue false
                )

            nameof __.DisableSkipBreak
                |> Binding.twoWay (_.DisableSkipBreak, Msg.SetDisableSkipBreak)

            nameof __.DisableMinimizeMaximizeWindows
                |> Binding.twoWay (_.DisableMinimizeMaximizeWindows, Msg.SetDisableMinimizeMaximizeWindows)
        ]
