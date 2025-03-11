namespace PomodoroWindowsTimer.ElmishApp.RollbackWorkModel

open Elmish.WPF
open Elmish.Extensions

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.RollbackWorkModel
open PomodoroWindowsTimer.ElmishApp.Abstractions


type private Binding = Binding<RollbackWorkModel, RollbackWorkModel.Msg>

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

    member val InterpretAsBreakAndCloseCommand : Binding =
        nameof __.InterpretAsBreakAndCloseCommand
            |> Binding.cmdIf (chooseIfWorkKind (Msg.SetLocalRollbackStrategyAndClose LocalRollbackStrategy.InvertSpentTime))

    member val InterpretAsWorkAndCloseCommand : Binding =
        nameof __.InterpretAsWorkAndCloseCommand
            |> Binding.cmdIf (chooseIfBreakKind (Msg.SetLocalRollbackStrategyAndClose LocalRollbackStrategy.InvertSpentTime))

    member val ApplyAsWorkAndCloseCommand : Binding =
        nameof __.ApplyAsWorkAndCloseCommand
            |> Binding.cmd (Msg.SetLocalRollbackStrategyAndClose LocalRollbackStrategy.ApplyAsWorkTime)

    member val ApplyAsBreakAndCloseCommand : Binding =
        nameof __.ApplyAsBreakAndCloseCommand
            |> Binding.cmd (Msg.SetLocalRollbackStrategyAndClose LocalRollbackStrategy.ApplyAsBreakTime)

    member val EraseCommand : Binding =
        nameof __.EraseCommand |> Binding.cmd (Msg.SetLocalRollbackStrategyAndClose LocalRollbackStrategy.SubstractSpentTime)

    member val CloseCommand : Binding =
        nameof __.CloseCommand |> Binding.cmd (Msg.SetLocalRollbackStrategyAndClose LocalRollbackStrategy.DoNotCorrect)

    member val IsWorkKind : Binding =
        nameof __.IsWorkKind |> Binding.oneWay (_.Kind >> Kind.isWork)
    
    member val IsBreakKind : Binding =
        nameof __.IsBreakKind |> Binding.oneWay (_.Kind >> Kind.isBreak)

    member val Difference : Binding =
        nameof __.Difference |> Binding.oneWay _.Difference

    member val Number : Binding =
        nameof __.Number |> Binding.oneWay (_.Work >> _.Number)

    member val Title : Binding =
        nameof __.Title |> Binding.oneWay (_.Work >> _.Title)
