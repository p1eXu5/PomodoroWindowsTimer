namespace CycleBell.ElmishApp.Models

open CycleBell.ElmishApp.Abstractions

type BotSettingsModel =
    {
        BotToken: string option
        ChatId: string option
    }


module BotSettingsModel =

    type Msg =
        | SetBotToken of string option
        | SetChatId of string option


    let init (botConfiguration: IBotConfiguration) =
        {
            BotToken = 
                match botConfiguration.BotToken with
                | null -> None
                | s -> s |> Some

            ChatId =
                match botConfiguration.MyChatId with
                | null -> None
                | s -> s |> Some
        }
