namespace PomodoroWindowsTimer.ElmishApp.Models

open PomodoroWindowsTimer.ElmishApp.Abstractions

type BotSettingsModel =
    {
        BotToken: string option
        ChatId: string option
    }


module BotSettingsModel =

    type Msg =
        | SetBotToken of string option
        | SetChatId of string option


    let init (botConfiguration: IBotSettings) =
        {
            BotToken = botConfiguration.BotToken
            ChatId = botConfiguration.MyChatId
        }
