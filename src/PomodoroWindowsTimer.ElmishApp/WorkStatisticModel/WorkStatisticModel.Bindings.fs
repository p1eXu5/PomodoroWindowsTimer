namespace PomodoroWindowsTimer.ElmishApp.WorkStatisticModel

open Elmish.WPF
open Elmish.Extensions
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.WorkStatisticModel
open System

type private Binding = Binding<WorkStatisticModel, WorkStatisticModel.Msg>

[<Sealed>]
type Bindings() =
    let date (dateTime: DateTime) =
        let d = dateTime.ToShortDateString()
        let t = dateTime.ToShortTimeString()
        $"{d} {t}"

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

    member val WorkId : Binding =
        nameof __.WorkId |> Binding.oneWay _.WorkId

    member val WorkNumber : Binding =
        nameof __.WorkNumber |> Binding.oneWay (_.Work >> _.EditableNumber)

    member val WorkTitle : Binding =
        nameof __.WorkTitle |> Binding.oneWay (_.Work >> _.EditableTitle)

    member val StartPeriod : Binding =
        nameof __.StartPeriod |> Binding.oneWayOpt (_.Statistic >> Option.map (_.Period >> _.Start >> date))

    member val EndPeriod : Binding =
        nameof __.EndPeriod |> Binding.oneWayOpt (_.Statistic >> Option.map (_.Period >> _.EndInclusive >> date))

    member val WorkTime : Binding =
        nameof __.WorkTime
        |> Binding.oneWayOpt (_.Statistic >> Option.map (_.WorkTime))

    member val BreakTime : Binding =
        nameof __.BreakTime |> Binding.oneWayOpt (_.Statistic >> Option.map (_.BreakTime))

    member val OverallTime : Binding =
        nameof __.OverallTime |> Binding.oneWayOpt (_.Statistic >> Option.map (fun s -> (s.BreakTime + s.WorkTime)))

