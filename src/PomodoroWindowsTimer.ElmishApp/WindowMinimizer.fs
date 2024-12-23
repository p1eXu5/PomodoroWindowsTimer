namespace PomodoroWindowsTimer.ElmishApp

open System
open System.Threading.Tasks
open System.Runtime.InteropServices
open PomodoroWindowsTimer.ElmishApp.Abstractions

module private Interop =

    [<DllImport("user32.dll", EntryPoint="FindWindow", SetLastError=true, CharSet=CharSet.Auto)>]
    extern IntPtr findWindow (string lpClassName, string lpWindowName)

    [<DllImport("user32.dll", EntryPoint = "SendMessage", SetLastError = true)>]
    extern IntPtr sendMessage(IntPtr hWnd, Int32 Msg, IntPtr wParam, IntPtr lParam)

    [<DllImport("user32.dll", EntryPoint = "ShowWindow")>]
    extern bool showWindow(IntPtr hWnd, int nCmdShow)

[<RequireQualifiedAccess>]
type public WindowsState =
    | Minimized
    | Maximized

type private State =
    {
        WindowsState: WindowsState
        LastUpdated: DateTimeOffset
    }

module private State =

    let withLastUpdated time (state: State) =
        { state with LastUpdated = time }

    let withWindowsState winState (state: State) =
        { state with WindowsState = winState }

type private Msg =
    | GetIsMinimized of AsyncReplyChannel<bool>
    | MinimizeAllRestoreAppWindowAsync
    | RestoreAllMinimized
    | RestoreAppWindow


type WindowsMinimizer(timeProvider: System.TimeProvider) as this =
    let [<Literal>] WM_COMMAND : Int32 = 0x111;

    let [<Literal>] MIN_ALL : Int32 = 419;
    let [<Literal>] MIN_ALL_UNDO : Int32 = 416;

    let [<Literal>] SW_SHOWNORMAL = 1;
    let [<Literal>] SW_MAXIMIZE = 3;
    let [<Literal>] SW_SHOW = 5;
    let [<Literal>] SW_MINIMIZE = 6;
    let [<Literal>] SW_RESTORE = 9;

    let shellTrayWnd = Interop.findWindow("Shell_TrayWnd", null)

    let mutable _isDisposed = false

    let agent = 
        MailboxProcessor<Msg>.Start(
            fun inbox ->
                let rec loop state =
                    async {
                        let! msg = inbox.Receive()

                        let diff = (timeProvider.GetLocalNow()) - state.LastUpdated
                        if diff < TimeSpan.FromMilliseconds(500) then
                            do! Async.Sleep 500

                        let now = timeProvider.GetLocalNow()

                        match msg with
                        | Msg.GetIsMinimized reply ->
                            match state.WindowsState with
                            | WindowsState.Minimized ->
                                reply.Reply(true)

                            | WindowsState.Maximized ->
                                reply.Reply(false)
                            
                            return! loop (state |> State.withLastUpdated now)

                        | Msg.MinimizeAllRestoreAppWindowAsync when this.AppWindowPtr <> IntPtr.Zero ->
                            let shellTrayWnd = Interop.findWindow("Shell_TrayWnd", null)
                            Interop.sendMessage(shellTrayWnd, WM_COMMAND, IntPtr(MIN_ALL), IntPtr.Zero) |> ignore
                            do! Async.Sleep(500)
                            Interop.showWindow(this.AppWindowPtr, SW_RESTORE) |> ignore
                            let now = timeProvider.GetLocalNow()

                            return! loop (state |> State.withLastUpdated now |> State.withWindowsState WindowsState.Minimized)

                        | Msg.RestoreAllMinimized ->
                            Interop.sendMessage(shellTrayWnd, WM_COMMAND, IntPtr(MIN_ALL_UNDO), IntPtr.Zero) |> ignore
                            return! loop (state |> State.withLastUpdated now |> State.withWindowsState WindowsState.Maximized)

                        | Msg.RestoreAppWindow when this.AppWindowPtr <> IntPtr.Zero ->
                             Interop.showWindow(this.AppWindowPtr, SW_RESTORE) |> ignore
                             return! loop (state |> State.withLastUpdated now)

                        | _ -> return! loop state
                    }

                loop { WindowsState = WindowsState.Maximized; LastUpdated = timeProvider.GetLocalNow() }
        )

    /// Initialized onetime in the WpfClient.Bootstrap.ShowMainWindow
    member val internal AppWindowPtr: IntPtr = IntPtr.Zero with get, set

    member _.GetIsMinimized () =
        agent.PostAndReply(Msg.GetIsMinimized)

    member _.RestoreAllMinimized () =
        agent.Post(Msg.RestoreAllMinimized)

    member _.MinimizeAllRestoreAppWindowAsync () =
        agent.Post(Msg.MinimizeAllRestoreAppWindowAsync)

    member this.RestoreAppWindow () =
        agent.Post(Msg.RestoreAppWindow)

    member private _.Dispose(isDisposing: bool) =
        if _isDisposed then ()
        else
            if isDisposing then
                (agent :> IDisposable).Dispose()

            _isDisposed <- true

    interface IWindowsMinimizer with
        member this.GetIsMinimized () =
            this.GetIsMinimized ()
        member this.MinimizeAllRestoreAppWindowAsync () =
            this.MinimizeAllRestoreAppWindowAsync()
        member this.RestoreAllMinimized() =
            this.RestoreAllMinimized()
        member this.RestoreAppWindow () =
            this.RestoreAppWindow ()
        member this.AppWindowPtr with set ptr = this.AppWindowPtr <- ptr

    interface IDisposable with
        member this.Dispose() = this.Dispose(true)


type StabWindowsMinimizer() as this =
    let mutable _isDisposed = false

    let agent = 
        MailboxProcessor<Msg>.Start(
            fun inbox ->
                let rec loop state =
                    async {
                        let! msg = inbox.Receive()

                        match msg with
                        | Msg.GetIsMinimized reply ->
                            match state.WindowsState with
                            | WindowsState.Minimized ->
                                reply.Reply(true)

                            | WindowsState.Maximized ->
                                reply.Reply(false)
                            
                            return! loop state

                        | Msg.MinimizeAllRestoreAppWindowAsync when this.AppWindowPtr <> IntPtr.Zero ->
                             return! loop (state |> State.withWindowsState WindowsState.Minimized)

                        | Msg.RestoreAllMinimized ->
                            return! loop (state |> State.withWindowsState WindowsState.Maximized)

                        | Msg.RestoreAppWindow when this.AppWindowPtr <> IntPtr.Zero ->
                             return! loop (state)

                        | _ -> return! loop state
                    }

                loop { WindowsState = WindowsState.Maximized; LastUpdated = DateTimeOffset.Now }
        )

    /// Initialized onetime in the WpfClient.Bootstrap.ShowMainWindow
    member val internal AppWindowPtr: IntPtr = IntPtr.Zero with get, set

    member _.GetIsMinimized () =
        agent.PostAndReply(Msg.GetIsMinimized)

    member _.RestoreAllMinimized () =
        agent.Post(Msg.RestoreAllMinimized)

    member _.MinimizeAllRestoreAppWindowAsync () =
        agent.Post(Msg.MinimizeAllRestoreAppWindowAsync)

    member this.RestoreAppWindow () =
        agent.Post(Msg.RestoreAppWindow)

    member private _.Dispose(isDisposing: bool) =
        if _isDisposed then ()
        else
            if isDisposing then
                (agent :> IDisposable).Dispose()

            _isDisposed <- true

    interface IWindowsMinimizer with
        member this.GetIsMinimized () =
            this.GetIsMinimized ()
        member this.MinimizeAllRestoreAppWindowAsync () =
            this.MinimizeAllRestoreAppWindowAsync()
        member this.RestoreAllMinimized() =
            this.RestoreAllMinimized()
        member this.RestoreAppWindow () =
            this.RestoreAppWindow ()
        member this.AppWindowPtr
            with set ptr = this.AppWindowPtr <- ptr

    interface IDisposable with
        member this.Dispose() = this.Dispose(true)

