namespace PomodoroWindowsTimer.ElmishApp.Models

open PomodoroWindowsTimer.Types
open System

type TimePointsSettingsModel =
    {
        TimePointPrototypes: TimePointPrototype list
        Patterns: string list
        SelectedPattern: string option
    }


module TimePointsSettingsModel =

    type Msg =
        | ParsePattern of string
        | TimePointPrototypeMsg of Kind * TimePointPrototypeMsg
    and
        TimePointPrototypeMsg =
            | SetTimeSpan of TimeSpan


    open PomodoroWindowsTimer.ElmishApp.Infrastructure

    let init (kindAliasesStore: TimePointPrototypeStore) =
        {
            TimePointPrototypes = kindAliasesStore.Read ()
        }