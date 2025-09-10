namespace PomodoroWindowsTimer.ElmishApp.CurrentWorkModel

open System
open Elmish.WPF
open Elmish.Extensions

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.CurrentWorkModel
open System.Windows.Input

type IBindings =
    interface
        abstract Id: WorkId
        abstract Number: string
        abstract Title: string
        abstract LastEventCreatedAt: DateTimeOffset option
        abstract LastEventCreatedAtOrUpdatedAt: DateTimeOffset
        abstract UpdatedAt: DateTimeOffset
    end

module Bindings =
    let private __ = Unchecked.defaultof<IBindings>

    let bindings () : Binding<CurrentWorkModel, CurrentWorkModel.Msg> list =
        [
            nameof __.Id |> Binding.oneWay (_.Work >> _.Id)
            nameof __.Number |> Binding.oneWay (_.Work >> _.Number)
            nameof __.Title |> Binding.oneWay (_.Work >> _.Title)
            nameof __.LastEventCreatedAt |> Binding.oneWayOpt (_.Work >> _.LastEventCreatedAt)

            nameof __.LastEventCreatedAtOrUpdatedAt
                |> Binding.oneWay (fun m ->
                    m.Work |> _.LastEventCreatedAt |> Option.defaultValue m.Work.UpdatedAt
                )
            nameof __.UpdatedAt |> Binding.oneWay (_.Work >> _.UpdatedAt)
        ]
