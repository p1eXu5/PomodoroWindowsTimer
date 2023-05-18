module PomodoroWindowsTimer.ElmishApp.BotSettingsModel.Program

open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.BotSettingsModel


let update (botConfiguration: IBotSettings) msg model =
    match msg with
    | SetBotToken token ->
        botConfiguration.BotToken <- token
        { model with BotToken = token }

    | SetChatId chatId ->
        botConfiguration.MyChatId <- chatId
        { model with ChatId = chatId }
