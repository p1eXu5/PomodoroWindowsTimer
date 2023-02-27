module CycleBell.ElmishApp.BotSettingsModel.Bindings

open CycleBell.ElmishApp
open CycleBell.ElmishApp.Models
open CycleBell.ElmishApp.Models.BotSettingsModel

open Elmish.WPF

let bindings () : Binding<BotSettingsModel, Msg> list =
    [
        "BotToken" |> Binding.twoWayOpt ((fun m -> m.BotToken), SetBotToken)
        "ChatId" |> Binding.twoWayOpt ((fun m -> m.ChatId), SetChatId)
    ]