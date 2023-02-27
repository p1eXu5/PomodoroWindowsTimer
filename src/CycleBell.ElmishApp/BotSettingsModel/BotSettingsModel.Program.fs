module CycleBell.ElmishApp.BotSettingsModel.Program

open CycleBell.ElmishApp.Abstractions
open CycleBell.ElmishApp.Models
open CycleBell.ElmishApp.Models.BotSettingsModel


let update (botConfiguration: IBotConfiguration) msg model =
    match msg with
    | SetBotToken token ->
        botConfiguration.BotToken <- (token |> Option.defaultValue "")
        { model with BotToken = token }

    | SetChatId chatId ->
        botConfiguration.MyChatId <- (chatId |> Option.defaultValue "")
        { model with ChatId = chatId }
