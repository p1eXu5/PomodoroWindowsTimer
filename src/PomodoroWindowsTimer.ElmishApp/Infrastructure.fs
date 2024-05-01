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

module WindowsMinimizer =

    open System.Runtime.InteropServices

    [<DllImport("user32.dll", EntryPoint="FindWindow", SetLastError=true, CharSet=CharSet.Auto)>]
    extern IntPtr findWindow (string lpClassName, string lpWindowName)

    [<DllImport("user32.dll", EntryPoint = "SendMessage", SetLastError = true)>]
    extern IntPtr sendMessage(IntPtr hWnd, Int32 Msg, IntPtr wParam, IntPtr lParam)

    [<DllImport("user32.dll", EntryPoint = "ShowWindow")>]
    extern bool showWindow(IntPtr hWnd, int nCmdShow)

    let [<Literal>] WM_COMMAND : Int32 = 0x111;

    let [<Literal>] MIN_ALL : Int32 = 419;
    let [<Literal>] MIN_ALL_UNDO : Int32 = 416;

    let [<Literal>] SW_SHOWNORMAL = 1;
    let [<Literal>] SW_MAXIMIZE = 3;
    let [<Literal>] SW_SHOW = 5;
    let [<Literal>] SW_MINIMIZE = 6;
    let [<Literal>] SW_RESTORE = 9;

    let minimize ipWindowName =
        task {
            let appWindow = findWindow(null, ipWindowName)
            let shellTrayWnd = findWindow("Shell_TrayWnd", null)
            sendMessage(shellTrayWnd, WM_COMMAND, IntPtr(MIN_ALL), IntPtr.Zero) |> ignore
            do! Task.Delay(500)
            showWindow(appWindow, SW_RESTORE) |> ignore
        }

    let restore () =
        let shellTrayWnd = findWindow("Shell_TrayWnd", null)
        sendMessage(shellTrayWnd, WM_COMMAND, IntPtr(MIN_ALL_UNDO), IntPtr.Zero) |> ignore
       

    let restoreMainWindow ipWindowName =
        let appWindow = findWindow(null, ipWindowName)
        showWindow(appWindow, SW_RESTORE) |> ignore

    let init mainWindowTitle =
        { new IWindowsMinimizer with
            member _.MinimizeOtherAsync () =
                minimize mainWindowTitle
            member _.Restore () =
                restore ()
            member _.RestoreMainWindow () =
                restoreMainWindow mainWindowTitle
        }

    /// for debug purpose
    let initStub _ =
        { new IWindowsMinimizer with
            member _.MinimizeOtherAsync () =
                task { return () }
            member _.Restore () =
                ()
            member _.RestoreMainWindow () =
                ()
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