﻿namespace CycleBell.ElmishApp.Abstractions

type ISettingsManager =
    interface
        abstract Load : key: string -> obj
        abstract Save : key: string -> value: obj -> unit
    end


type IErrorMessageQueue =
    interface
        abstract EnqueuError : string -> unit
    end


type IBotConfiguration =
    interface
        abstract BotToken : string
        abstract MyChatId : string
    end