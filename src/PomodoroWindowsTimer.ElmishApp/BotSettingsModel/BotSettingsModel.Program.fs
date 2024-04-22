module PomodoroWindowsTimer.ElmishApp.BotSettingsModel.Program

open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.BotSettingsModel


let update (botConfiguration: IBotSettings) msg model =
    match msg with
    | SetBotToken token ->
        { model with BotToken = token }, Intent.None

    | SetChatId chatId ->
        { model with ChatId = chatId }, Intent.None

    | Apply ->
        botConfiguration.MyChatId <- model.ChatId
        botConfiguration.BotToken <- model.BotToken
        model, Intent.CloseDialogRequested

    | Cancel ->
        model, Intent.CloseDialogRequested

