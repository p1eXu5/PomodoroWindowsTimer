namespace PomodoroWindowsTimer.ElmishApp.MainModel

open Elmish.WPF
open Elmish.Extensions

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions

open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.MainModel
open System.Windows.Input

type private Binding = Binding<MainModel, MainModel.Msg>

/// Design time bindings
type IBindings =
    interface
        abstract Title: string
        abstract AssemblyVersion: string
        abstract ErrorMessageQueue: IErrorMessageQueue

        abstract StartTimePointCommand: ICommand
        abstract PlayStopCommand: Binding
        abstract IsPlaying: Binding
        abstract ActiveTime: Binding
        abstract SendToChatBotCommand: Binding
        
        abstract Player: Binding

        abstract CurrentWork: Binding
        abstract IsCurrentWorkSet: Binding

        abstract IsTimePointsDrawerShown: bool with get, set
        abstract TimePointsDrawer: TimePointsDrawerModel.IBindings

        abstract IsWorkSelectorLoaded: bool
        abstract WorkSelector: Binding

        abstract AppDialog: Binding
        
        abstract IsWorkStatisticShown: Binding
        abstract WorkStatisticWindow: Binding
    end

module Bindings =

    let private __ = Unchecked.defaultof<IBindings>

    let bindings
        (title: string)
        (assemblyVersion: string)
        (workStatisticWindowFactory: System.Func<System.Windows.Window>)
        (mainErrorMessageQueue: IErrorMessageQueue)
        (dialogErrorMessageQueue: IErrorMessageQueue)
        : Binding list
        =
        [
            nameof __.Title
                |> Binding.oneWay (fun _ -> title)
                |> Binding.addLazy (fun _ _ -> true)

            nameof __.AssemblyVersion
                |> Binding.oneWay (fun _ -> assemblyVersion)
                |> Binding.addLazy (fun _ _ -> true)

            nameof __.ErrorMessageQueue 
                |> Binding.oneWay (fun _ -> mainErrorMessageQueue)
                |> Binding.addLazy (fun _ _ -> true)

            // -------------------------------------------------------------
    

            nameof __.StartTimePointCommand
                |> Binding.cmdParam (fun id -> (id :?> System.Guid) |> Msg.StartTimePoint)

            nameof __.PlayStopCommand
                |> Binding.cmdParam (fun id -> (id :?> System.Guid) |> Msg.PlayStopCommand)

            nameof __.IsPlaying
                |> Binding.oneWay (fun m -> m.Player |> PlayerModel.isPlaying)

            // Binding for selected running time point.
            nameof __.ActiveTime
                |> Binding.oneWay (fun m -> m.Player |> PlayerModel.getRemainingTimeSpan)

            // ----------------------------------------------------
            // For the test purpose
            //    nameof __.MinimizeCommand |> Binding.cmd (Msg.MinimizeAllRestoreApp)

            // For the test purpose
            nameof __.SendToChatBotCommand
                |> Binding.cmd (fun m ->
                    match m.Player.ActiveTimePoint, m.CurrentWork.Work with
                    | Some tp, None ->
                        Msg.SendToChatBot $"It's time to {tp.Name}!!"
                    | Some tp, Some wm ->
                        Msg.SendToChatBot (
                        $"""It's time to {tp.Name}!!

Current work is [{wm.Number}] {wm.Title}."""
                        )
                    | _ ->
                        Msg.SendToChatBot $"It's time!!"
                )

            // ----------------------------------------------------

            nameof __.IsTimePointsDrawerShown
                |> Binding.twoWay (_.IsTimePointsDrawerShown, Msg.SetIsTimePointsDrawerShown)

            nameof __.TimePointsDrawer
                |> Binding.SubModel.required (TimePointsDrawerModel.Bindings.bindings)
                |> Binding.mapModel _.TimePointsDrawer
                |> Binding.mapMsg Msg.TimePointsDrawerMsg

            // ------------------------------------------------------

            nameof __.CurrentWork
                |> Binding.SubModel.required (CurrentWorkModel.Bindings.bindings)
                |> Binding.mapModel _.CurrentWork
                |> Binding.mapMsg Msg.CurrentWorkModelMsg

            nameof __.IsCurrentWorkSet
                |> Binding.oneWay (_.CurrentWork.Work >> Option.isSome)

            // ------------------------------------------------------

            nameof __.AppDialog
                |> Binding.SubModel.required (fun () -> AppDialogModel.Bindings.ToList(dialogErrorMessageQueue))
                |> Binding.mapModel _.AppDialog
                |> Binding.mapMsg Msg.AppDialogModelMsg

            nameof __.WorkSelector
                |> Binding.SubModel.opt (WorkSelectorModel.Bindings.bindings)
                |> Binding.mapModel _.WorkSelector
                |> Binding.mapMsg Msg.WorkSelectorModelMsg

            nameof __.IsWorkSelectorLoaded
                |> Binding.twoWay (_.WorkSelector >> Option.isSome, Msg.SetIsWorkSelectorLoaded)

            nameof __.IsWorkStatisticShown
                |> Binding.twoWay (_.StatisticMainModel >> Option.isSome, Msg.SetIsWorkStatisticShown)

            nameof __.WorkStatisticWindow
                |> Binding.subModelWin (
                    (_.StatisticMainModel >> WindowState.ofOption),
                    snd,
                    Msg.StatisticMainModelMsg,
                    (fun () -> StatisticMainModel.Bindings.ToList(dialogErrorMessageQueue)),
                    (fun _ _ -> workStatisticWindowFactory.Invoke()),
                    isModal = false
                )

            nameof __.Player
                |> Binding.SubModel.required (PlayerModel.Bindings.bindings)
                |> Binding.mapModel _.Player
                |> Binding.mapMsg Msg.PlayerModelMsg
        ]
