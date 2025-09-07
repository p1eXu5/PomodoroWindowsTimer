namespace PomodoroWindowsTimer.ElmishApp.TimePointListModel

open Elmish.WPF

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models

type private Binding = Binding<TimePointListModel, TimePointListModel.Msg>

///// Design time bindings
//type IBindings =
//    interface
//        abstract Id: TimePointId
//        abstract Name: string
//        abstract TimeSpan: TimeSpan
//        abstract Kind: Kind
//        // TODO: move to presentation
//        abstract KindAlias: string
//        abstract IsSelected: bool
//    end

/// Design time bindings
type IBindings =
    interface
        abstract TimePoints: TimePoint seq
    end


module Bindings =

    let private __ = Unchecked.defaultof<IBindings>

    let bindings () : Binding list =
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
        ]


