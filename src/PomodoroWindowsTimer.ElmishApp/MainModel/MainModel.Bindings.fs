namespace PomodoroWindowsTimer.ElmishApp.MainModel

open Elmish.WPF
open Elmish.Extensions

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions

open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.MainModel

type private Binding = Binding<MainModel, MainModel.Msg>

/// Design time bindings
type IBindings =
    interface
        abstract Title: string
        abstract AssemblyVersion: string
        abstract ErrorMessageQueue: IErrorMessageQueue
        abstract TimePointList: Binding
        abstract StartTimePointCommand: Binding
        abstract PlayStopCommand: Binding
        abstract IsPlaying: Binding
        abstract ActiveTime: Binding
        abstract SendToChatBotCommand: Binding
        abstract IsTimePointsShown: Binding
        abstract CurrentWork: Binding
        abstract IsCurrentWorkSet: Binding
        abstract AppDialog: Binding
        abstract WorkSelector: Binding
        abstract IsWorkSelectorLoaded: Binding
        abstract IsWorkStatisticShown: Binding
        abstract WorkStatisticWindow: Binding
        abstract Player: Binding
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
                |> Binding.addLazy (=)

            nameof __.AssemblyVersion
                |> Binding.oneWay (fun _ -> assemblyVersion)
                |> Binding.addLazy (=)

            nameof __.ErrorMessageQueue 
                |> Binding.oneWay (fun _ -> mainErrorMessageQueue)
                |> Binding.addLazy LanguagePrimitives.PhysicalEquality

            // -------------------------------------------------------------
    
            nameof __.TimePointList
                |> Binding.SubModel.required (TimePointListModel.Bindings.bindings)
                |> Binding.mapModel _.TimePointList
                |> Binding.mapMsg Msg.TimePointListModelMsg

            nameof __.StartTimePointCommand
                |> Binding.cmdParam (fun id -> (id :?> System.Guid) |> Msg.StartTimePoint)

            nameof __.PlayStopCommand
                |> Binding.cmdParam (fun id -> (id :?> System.Guid) |> Msg.PlayStopCommand)

            nameof __.IsPlaying
                |> Binding.oneWay (fun m -> m.Player |> PlayerModel.isPlaying)
                |> Binding.addLazy (=)

            // Binding for selected running time point.
            nameof __.ActiveTime
                |> Binding.oneWay (fun m -> m.Player |> PlayerModel.getRemainingTimeSpan)
                |> Binding.addLazy (=)

            // ----------------------------------------------------
            // For the test purpose
            //    nameof __.MinimizeCommand |> Binding.cmd (Msg.MinimizeAllRestoreApp)

            // For the test purpose
            nameof __.SendToChatBotCommand
                |> Binding.cmd (fun m ->
                    match m.Player.ActiveTimePoint, m.CurrentWork with
                    | Some tp, None ->
                        Msg.SendToChatBot $"It's time to {tp.Name}!!"
                    | Some tp, Some wm ->
                        Msg.SendToChatBot (
                        $"""It's time to {tp.Name}!!

Current work is [{wm.Work.Number}] {wm.Work.Title}."""
                        )
                    | _ ->
                        Msg.SendToChatBot $"It's time!!"
                )

            // ----------------------------------------------------

            nameof __.IsTimePointsShown
                |> Binding.twoWay (_.IsTimePointsShown, Msg.SetIsTimePointsShown)

            // ------------------------------------------------------

            nameof __.CurrentWork
                |> Binding.SubModel.opt (WorkModel.Bindings.ToList)
                |> Binding.mapModel _.CurrentWork
                |> Binding.mapMsg Msg.WorkModelMsg

            nameof __.IsCurrentWorkSet
                |> Binding.oneWay (_.CurrentWork >> Option.isSome)

            // ------------------------------------------------------

            nameof __.AppDialog
                |> Binding.SubModel.required (fun () -> AppDialogModel.Bindings.ToList(dialogErrorMessageQueue))
                |> Binding.mapModel _.AppDialog
                |> Binding.mapMsg Msg.AppDialogModelMsg

            nameof __.WorkSelector
                |> Binding.SubModel.opt (WorkSelectorModel.Bindings.ToList)
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
