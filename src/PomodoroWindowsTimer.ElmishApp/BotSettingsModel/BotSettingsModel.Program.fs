﻿module PomodoroWindowsTimer.ElmishApp.BotSettingsModel.Program

open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.BotSettingsModel


let update (botConfiguration: IBotConfiguration) msg model =
    match msg with
    | SetBotToken token ->
        botConfiguration.BotToken <- (token |> Option.defaultValue "")
        { model with BotToken = token }

    | SetChatId chatId ->
        botConfiguration.MyChatId <- (chatId |> Option.defaultValue "")
        { model with ChatId = chatId }
