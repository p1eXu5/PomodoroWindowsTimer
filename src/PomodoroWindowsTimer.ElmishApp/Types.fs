namespace PomodoroWindowsTimer.ElmishApp

/// Used in theme switcher on WPF side
type TimePointKind =
    | Undefined = 0
    | Work = 1
    | Break = 2

type Message = string

(* TODO
[<Struct>]
type RollbackWorkStrategy = 
    | UserChoiceIsRequired
    | SubstractWorkAddBreak
    | Default
*)

[<Struct>]
type LocalRollbackStrategy = 
    | DoNotCorrect
    | SubstractSpentTime
    | InvertSpentTime
