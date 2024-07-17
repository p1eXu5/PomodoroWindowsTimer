namespace PomodoroWindowsTimer.ElmishApp.TimePointListModel

open System
open Elmish.WPF
open Elmish.Extensions

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions

open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.TimePointListModel

type private Binding = Binding<TimePointListModel, TimePointListModel.Msg>

[<Sealed>]
type Bindings() =
    static let props = Utils.bindingProperties typeof<Bindings>
    static let mutable __ = Unchecked.defaultof<Bindings>
 
    static member Instance() =
        if System.Object.ReferenceEquals(__, null) then
            __ <- Bindings()
            __
        else __

    static member ToList () =
        Utils.bindings<Binding>
            (Bindings.Instance())
            props


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
                    "IsSelected" |> Binding.oneWay (fun (m, e) -> m.ActiveTimePointId |> Option.map (fun atpId -> atpId = e.Id) |> Option.defaultValue false)
                ])
            )


