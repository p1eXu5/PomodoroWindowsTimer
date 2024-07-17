namespace PomodoroWindowsTimer.ElmishApp.Infrastructure

open System
open System.Threading.Tasks
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer



type TimePointPrototypeStore =
    {
        Read: unit -> TimePointPrototype list
        Write: TimePointPrototype list -> unit
    }

module TimePointPrototypeStore =
    let read (timePointPrototypeSettings : ITimePointPrototypesSettings) =
        timePointPrototypeSettings.TimePointPrototypesSettings
        |> Option.map (fun s ->
            JsonHelpers.Deserialize<TimePointPrototype list>(s)
        )
        |> Option.defaultValue TimePointPrototype.defaults


    let write (timePointPrototypeSettings : ITimePointPrototypesSettings) (timePointPrototypes: TimePointPrototype list) =
        match timePointPrototypes with
        | [] ->
            timePointPrototypeSettings.TimePointPrototypesSettings <- None
        | _ ->
            let s = JsonHelpers.Serialize(timePointPrototypes)
            timePointPrototypeSettings.TimePointPrototypesSettings <- s |> Some


    let initialize (timePointPrototypeSettings : ITimePointPrototypesSettings) : TimePointPrototypeStore =
        {
            Read = fun () -> read timePointPrototypeSettings
            Write = write timePointPrototypeSettings
        }


type TimePointStore =
    {
        Read: unit -> TimePoint list
        Write: TimePoint list -> unit
    }

module TimePointStore =

    let read (timePointSettings : ITimePointSettings) =
        timePointSettings.TimePointSettings
        |> Option.map (fun s ->
            JsonHelpers.Deserialize<TimePoint list>(s)
        )
        |> Option.defaultValue TimePoint.defaults


    let write (timePointSettings : ITimePointSettings) (timePoints: TimePoint list) =
        match timePoints with
        | [] ->
            timePointSettings.TimePointSettings <- None
        | _ ->
            let s = JsonHelpers.Serialize(timePoints)
            timePointSettings.TimePointSettings <- s |> Some


    let initialize (timePointSettings : ITimePointSettings) : TimePointStore =
        {
            Read = fun () -> read timePointSettings
            Write = write timePointSettings
        }


type PatternStore =
    {
        Read: unit -> Pattern list
        Write: Pattern list -> unit
    }

module PatternStore =

    let read (patternSettings : IPatternSettings) =
        match patternSettings.Patterns with
        | [] -> Pattern.defaults
        | l -> l


    let write (patternSettings : IPatternSettings) (patterns: Pattern list) =
        patternSettings.Patterns <- patterns


    let init (patternSettings : IPatternSettings) : PatternStore =
        {
            Read = fun () -> read patternSettings
            Write = write patternSettings
        }


module TelegramBot =

    open Telegram.Bot

    let sendMessage (botClient: ITelegramBotClient) chatId text =
        task {
            let! _ =
                botClient.SendTextMessageAsync(
                    chatId,
                    text)

            return ()
        }

    let init (userSettings: IBotSettings) =
        { new ITelegramBot with
            member _.SendMessage message =
                match userSettings.BotToken, userSettings.MyChatId with
                | Some botToken, Some myChatId ->
                    let botClient = TelegramBotClient(botToken)
                    sendMessage botClient (Types.ChatId(myChatId)) message
                | _ ->
                    task { return () }
        }


module ExcelReport =

    open Microsoft.Win32

    open PomodoroWindowsTimer
    open PomodoroWindowsTimer.Types
    open PomodoroWindowsTimer.Abstractions

    let exportToExcelTask (workEventRepo: IWorkEventRepository) (excelBook: IExcelBook) =
        let fd = SaveFileDialog()
        fd.Filter <- "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*"
        let result = fd.ShowDialog()
        if result.HasValue && result.Value then
            fun period ct ->
                task {
                    let! events = workEventRepo.FindAllByPeriodAsync period ct
                    return
                        events
                        |> Result.bind (ExcelExporter.export excelBook (TimeSpan.FromMinutes(5)) fd.FileName)
                }
        else
            fun _ _ ->
                task {
                    return Ok ()
                }
