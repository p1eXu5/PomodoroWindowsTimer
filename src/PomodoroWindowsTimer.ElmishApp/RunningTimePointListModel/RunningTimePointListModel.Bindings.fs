namespace PomodoroWindowsTimer.ElmishApp.RunningTimePointListModel

open Elmish.WPF

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.RunningTimePointListModel
open System.Windows.Input

/// Design time bindings
type IBindings =
    interface
        abstract TimePoints: TimePoint seq
        abstract SelectedTimePointId: TimePointId option
        abstract DisableSkipBreak : bool with get, set
        abstract DisableMinimizeMaximizeWindows : bool with get, set
        abstract RequestTimePointGeneratorCommand: ICommand
    end


module Bindings =

    let private __ = Unchecked.defaultof<IBindings>

    let bindings () : Binding<RunningTimePointListModel, RunningTimePointListModel.Msg> list =
        [
            nameof __.TimePoints
                |> Binding.subModelSeq (
                    (fun m -> m.TimePoints),
                    snd,
                    (fun tp -> tp.Id),
                    Msg.TimePointModelMsg,
                    TimePointModel.Bindings.bindings
                )

            // Do not use in running time point cause it's not controlled by ItemsControl but Looper
            nameof __.SelectedTimePointId |> Binding.subModelSelectedItem ((nameof __.TimePoints), _.ActiveTimePointId, Msg.SetActiveTimePointId)

            nameof __.DisableSkipBreak
                |> Binding.twoWay (_.DisableSkipBreak, Msg.SetDisableSkipBreak)

            nameof __.DisableMinimizeMaximizeWindows
                |> Binding.twoWay (_.DisableMinimizeMaximizeWindows, Msg.SetDisableMinimizeMaximizeWindows)

            nameof __.RequestTimePointGeneratorCommand
                |> Binding.cmd Msg.RequestTimePointGenerator
        ]


