namespace PomodoroWindowsTimer.ElmishApp.BotSettingsModel

open Elmish.WPF
open Elmish.Extensions

open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.BotSettingsModel

type private Binding = Binding<BotSettingsModel, BotSettingsModel.Msg>

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

    member val BotToken : Binding =
        nameof __.BotToken |> Binding.twoWayOpt ((fun m -> m.BotToken), SetBotToken)

    member val ChatId : Binding =
        nameof __.ChatId |> Binding.twoWayOpt ((fun m -> m.ChatId), SetChatId)

    member val ApplyCommand : Binding =
        nameof __.ApplyCommand
            |> Binding.cmdIf (fun m ->
                match m.ChatId, m.BotToken with
                | Some _, Some _ -> Msg.Apply |> Some
                | _ -> None
            )

    member val CancelCommand : Binding =
        nameof __.CancelCommand |> Binding.cmd Msg.Cancel
