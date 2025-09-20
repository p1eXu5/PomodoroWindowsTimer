namespace PomodoroWindowsTimer.ElmishApp.TimePointsGeneratorModel

open System.Windows.Input
open Elmish.WPF
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.TimePointsGeneratorModel

type IBindings =
    interface
        abstract TimePointPrototypes: TimePointPrototypeModel.IBindings list
        abstract TimePoints: TimePointModel.IBindings list
        abstract Patterns: string list
        abstract SelectedPattern: string option with get, set
        abstract SelectedPatternIndex: int with get, set
        abstract IsPatternCorrect: bool
        abstract TimePointsTime: obj
        abstract ApplyCommand: ICommand
        abstract CancelCommand: ICommand
    end

module Bindings =

    let private __ = Unchecked.defaultof<IBindings>

    let bindings ()  : Binding<TimePointsGeneratorModel, TimePointsGeneratorModel.Msg> list =
        [
            nameof __.TimePointPrototypes
                |> Binding.subModelSeq (TimePointPrototypeModel.Bindings.bindings, _.Prototype >> _.Kind)
                |> Binding.mapModel _.TimePointPrototypes
                |> Binding.mapMsg TimePointPrototypeMsg

            nameof __.TimePoints
                |> Binding.subModelSeq (TimePointModel.Bindings.bindings, _.TimePoint >> _.Id)
                |> Binding.mapModel _.TimePoints
                |> Binding.mapMsg TimePointMsg

            // TODO: copy from LogParser
            nameof __.Patterns |> Binding.oneWaySeq (getPatterns, (=), id)
            nameof __.SelectedPattern
                |> Binding.twoWayOpt ((fun m -> m.SelectedPattern), SetSelectedPattern)
                |> Binding.addValidation (fun m -> if m.IsPatternWrong then ["Wrong pattern"] else [])

            nameof __.SelectedPatternIndex |> Binding.twoWay (getSelectedPatternIndex, SetSelectedPatternIndex)
            nameof __.IsPatternCorrect |> Binding.oneWay (fun m -> m.IsPatternWrong |> not)

            nameof __.TimePointsTime
                |> Binding.SubModel.vopt (fun () -> [
                    "WorkTime" |> Binding.oneWay _.WorkTime
                    "BreakTime" |> Binding.oneWay _.BreakTime
                    "TotalTime" |> Binding.oneWay (fun (m: TimePointsTime) -> m.WorkTime + m.BreakTime)
                ])
                |> Binding.mapModel _.TimePointsTime

            nameof __.ApplyCommand
                |> Binding.cmdIf (fun m ->
                    if not m.IsPatternWrong then
                        match m.TimePoints with
                        | [] -> None
                        | _ -> Some Msg.ApplyTimePoints
                    else None
                )

            nameof __.CancelCommand
                |> Binding.cmd Msg.RequestCancelling
        ]

