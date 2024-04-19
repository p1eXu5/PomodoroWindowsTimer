namespace PomodoroWindowsTimer.ElmishApp.Infrastructure

open System
open System.Threading.Tasks
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp.Abstractions


type TimePointPrototypeStore =
    {
        Read: unit -> TimePointPrototype list
        Write: TimePointPrototype list -> unit
    }

module TimePointPrototypeStore =
    open System.Text.Json
    open System.Text.Json.Serialization

    let options =
        JsonFSharpOptions.Default()
            .WithUnionExternalTag()
            .WithUnionNamedFields()
            .WithUnionUnwrapSingleCaseUnions(false)
            .ToJsonSerializerOptions()

    let read (timePointPrototypeSettings : ITimePointPrototypesSettings) =
        timePointPrototypeSettings.TimePointPrototypesSettings
        |> Option.map (fun s ->
            JsonSerializer.Deserialize<TimePointPrototype list>(s, options)
        )
        |> Option.defaultValue TimePointPrototype.defaults


    let write (timePointPrototypeSettings : ITimePointPrototypesSettings) (timePointPrototypes: TimePointPrototype list) =
        match timePointPrototypes with
        | [] ->
            timePointPrototypeSettings.TimePointPrototypesSettings <- None
        | _ ->
            let s = JsonSerializer.Serialize(timePointPrototypes, options)
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
    open System.Text.Json
    open System.Text.Json.Serialization

    let options =
        JsonFSharpOptions.Default()
            .WithUnionExternalTag()
            .WithUnionNamedFields()
            .WithUnionUnwrapSingleCaseUnions(false)
            .ToJsonSerializerOptions()

    let read (timePointSettings : ITimePointSettings) =
        timePointSettings.TimePointSettings
        |> Option.map (fun s ->
            JsonSerializer.Deserialize<TimePoint list>(s, options)
        )
        |> Option.defaultValue TimePoint.defaults


    let write (timePointSettings : ITimePointSettings) (timePoints: TimePoint list) =
        match timePoints with
        | [] ->
            timePointSettings.TimePointSettings <- None
        | _ ->
            let s = JsonSerializer.Serialize(timePoints, options)
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


    let initialize (patternSettings : IPatternSettings) : PatternStore =
        {
            Read = fun () -> read patternSettings
            Write = write patternSettings
        }


type WindowsMinimizer =
    {
        MinimizeOther: unit -> Async<unit>
        Restore: unit -> Async<unit>
        RestoreMainWindow: unit -> Async<unit>
    }

module Windows =

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
        async {
            let appWindow = findWindow(null, ipWindowName)
            let shellTrayWnd = findWindow("Shell_TrayWnd", null)
            sendMessage(shellTrayWnd, WM_COMMAND, IntPtr(MIN_ALL), IntPtr.Zero) |> ignore
            do! Async.Sleep(500)
            showWindow(appWindow, SW_RESTORE) |> ignore
        }

    let restore () =
        async {
            let shellTrayWnd = findWindow("Shell_TrayWnd", null)
            sendMessage(shellTrayWnd, WM_COMMAND, IntPtr(MIN_ALL_UNDO), IntPtr.Zero) |> ignore
        }

    let restoreMainWindow ipWindowName =
        async {
            let appWindow = findWindow(null, ipWindowName)
            showWindow(appWindow, SW_RESTORE) |> ignore
        }

    let prodWindowsMinimizer mainWindowTitle =
        {
            MinimizeOther = fun () -> minimize mainWindowTitle
            Restore = restore
            RestoreMainWindow = fun () -> restoreMainWindow mainWindowTitle
        }

    /// for debug purpose
    let simWindowsMinimizer =
        {
            MinimizeOther = fun _ -> async.Return ()
            Restore = async.Return
            RestoreMainWindow = fun _ -> async.Return ()
        }


type Message = string

type BotSender = Message -> Task<unit>

module Telegram =

    open Telegram.Bot

    let sendToBot (botClient: ITelegramBotClient) chatId text =
        task {
            let! _ =
                botClient.SendTextMessageAsync(
                    chatId,
                    text)

            return ()
        }