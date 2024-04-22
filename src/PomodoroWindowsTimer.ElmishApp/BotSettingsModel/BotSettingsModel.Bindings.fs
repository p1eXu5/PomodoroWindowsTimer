module PomodoroWindowsTimer.ElmishApp.BotSettingsModel.Bindings

open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.BotSettingsModel

open Elmish.WPF

let bindings () : Binding<BotSettingsModel, Msg> list =
    [
        "BotToken" |> Binding.twoWayOpt ((fun m -> m.BotToken), SetBotToken)
        "ChatId" |> Binding.twoWayOpt ((fun m -> m.ChatId), SetChatId)
        "ApplyCommand"
            |> Binding.cmdIf (fun m ->
                match m.ChatId, m.BotToken with
                | Some _, Some _ -> Msg.Apply |> Some
                | _ -> None
            )

        "CancelCommand" |> Binding.cmd Msg.Cancel
    ]