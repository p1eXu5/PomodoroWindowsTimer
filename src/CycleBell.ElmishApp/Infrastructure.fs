module CycleBell.ElmishApp.Infrastructure

open System.Runtime.InteropServices
open System



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

let minimize () =
    async {
        let appWindow = findWindow(null, "MainWindow")
        let shellTrayWnd = findWindow("Shell_TrayWnd", null)
        sendMessage(shellTrayWnd, WM_COMMAND, IntPtr(MIN_ALL), IntPtr.Zero) |> ignore
        do! Async.Sleep(500)
        showWindow(appWindow, SW_RESTORE) |> ignore
    }
