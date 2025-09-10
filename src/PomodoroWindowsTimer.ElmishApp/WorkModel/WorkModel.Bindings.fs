namespace PomodoroWindowsTimer.ElmishApp.WorkModel

open System
open Elmish.WPF
open Elmish.Extensions

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.WorkModel
open System.Windows.Input

type IBindings =
    interface
        abstract Id: WorkId
        abstract Number: string
        abstract Title: string
        abstract LastEventCreatedAt: DateTimeOffset option
        abstract LastEventCreatedAtOrUpdatedAt: DateTimeOffset
        abstract EditNumber: string with get, set
        abstract EditTitle: string with get, set
        abstract UpdatedAt: DateTimeOffset
        abstract UpdateCommand: ICommand
        abstract CreateCommand: ICommand
        abstract SelectCommand: ICommand
        abstract EditCommand: ICommand
        abstract CancelEditCommand: ICommand
    end

module Bindings =
    let private __ = Unchecked.defaultof<IBindings>

    let bindings () : Binding<WorkModel, WorkModel.Msg> list =
        [
            nameof __.Id |> Binding.oneWay (_.Work >> _.Id)
            nameof __.Number |> Binding.oneWay (_.Work >> _.Number)
            nameof __.Title |> Binding.oneWay (_.Work >> _.Title)
            nameof __.LastEventCreatedAt |> Binding.oneWayOpt (_.Work >> _.LastEventCreatedAt)

            nameof __.LastEventCreatedAtOrUpdatedAt
                |> Binding.oneWay (fun m ->
                    m.Work |> _.LastEventCreatedAt |> Option.defaultValue m.Work.UpdatedAt
                )

            nameof __.EditNumber
                |> Binding.twoWay (_.EditableNumber, (fun (v: string) -> v.Trim() |> Msg.SetNumber))

            nameof __.EditTitle
                |> Binding.twoWay (_.EditableTitle, (fun (v: string) -> v.Trim() |> Msg.SetTitle))

            nameof __.UpdatedAt |> Binding.oneWay (_.Work >> _.UpdatedAt)

            nameof __.UpdateCommand
                |> Binding.cmdIf (fun m ->
                    match m |> isModified, m.CreateNewState with
                    | true, AsyncDeferred.NotRequested ->
                        AsyncOperation.startUnit Msg.Update |> Some
                    | _ -> None
                )

            nameof __.CreateCommand
                |> Binding.cmdIf (fun m ->
                    match m |> isModified, m.UpdateState with
                    | true, AsyncDeferred.NotRequested ->
                        AsyncOperation.startUnit Msg.Update |> Some
                    | _ -> None
                )

            nameof __.SelectCommand |> Binding.cmd Msg.Select
            nameof __.EditCommand |> Binding.cmd Msg.Edit
            nameof __.CancelEditCommand |> Binding.cmd Msg.CancelEdit
        ]
