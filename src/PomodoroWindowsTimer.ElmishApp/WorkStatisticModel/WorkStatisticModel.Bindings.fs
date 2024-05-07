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
    let time (timeSpan: TimeSpan) =
        // let hours = MathF.Floor(float32 timeSpan.TotalHours)
        // let minutes = timeSpan.Minutes
        $"{timeSpan.Hours} h  {timeSpan.Minutes} m"


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
        nameof __.WorkNumber |> Binding.oneWay (_.Work >> _.Number)

    member val WorkTitle : Binding =
        nameof __.WorkTitle |> Binding.oneWay (_.Work >> _.Title)

    member val StartPeriod : Binding =
        nameof __.StartPeriod |> Binding.oneWayOpt (_.Statistic >> Option.map (_.Period >> _.Start >> fun d -> d.ToShortDateString()))

    member val EndPeriod : Binding =
        nameof __.EndPeriod |> Binding.oneWayOpt (_.Statistic >> Option.map (_.Period >> _.EndInclusive >> fun d -> d.ToShortDateString()))

    member val WorkTime : Binding =
        nameof __.WorkTime
        |> Binding.oneWayOpt (_.Statistic >> Option.map (_.WorkTime >> time))

    member val BreakTime : Binding =
        nameof __.BreakTime |> Binding.oneWayOpt (_.Statistic >> Option.map (_.BreakTime >> time))

    member val OverallTime : Binding =
        nameof __.OverallTime |> Binding.oneWayOpt (_.Statistic >> Option.map (fun s -> (s.BreakTime + s.WorkTime) |> time))

