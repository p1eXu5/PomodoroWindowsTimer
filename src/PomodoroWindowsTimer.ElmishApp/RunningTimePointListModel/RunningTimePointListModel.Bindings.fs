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
                    (fun tp -> tp.Id),
                    (fun () -> [
                        "Id" |> Binding.oneWay (fun (_, e) -> e.Id)
                        "Name" |> Binding.oneWay (fun (_, e) -> e.Name)
                        // TODO: move conversion to presentation
                        "TimeSpan" |> Binding.oneWay (fun (_, e) -> e.TimeSpan.ToString("h':'mm"))
                        "Kind" |> Binding.oneWay (fun (_, e) -> e.Kind)
                        "KindAlias" |> Binding.oneWay (fun (_, e) -> e.KindAlias |> Alias.value)
                        "IsSelected" |> Binding.oneWay (fun (m, e) -> m.ActiveTimePointId |> Option.map (fun atpId -> atpId = e.Id) |> Option.defaultValue false)
                    ])
                )

            nameof __.DisableSkipBreak
                |> Binding.twoWay (_.DisableSkipBreak, Msg.SetDisableSkipBreak)

            nameof __.DisableMinimizeMaximizeWindows
                |> Binding.twoWay (_.DisableMinimizeMaximizeWindows, Msg.SetDisableMinimizeMaximizeWindows)

            nameof __.RequestTimePointGeneratorCommand
                |> Binding.cmd Msg.RequestTimePointGenerator
        ]


