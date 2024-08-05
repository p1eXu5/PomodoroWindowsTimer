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

[<Sealed>]
type Bindings(
    title: string,
    assemblyVersion: string,
    workStatisticWindowFactory: System.Func<System.Windows.Window>,
    mainErrorMessageQueue: IErrorMessageQueue,
    dialogErrorMessageQueue: IErrorMessageQueue,
    timePointQueue: ITimePointQueue,
    looper: ILooper
) =
    static let props = Utils.bindingProperties typeof<Bindings>
    static let mutable __ = Unchecked.defaultof<Bindings>
    static member Instance
        title
        assemblyVersion
        (workStatisticWindowFactory: System.Func<System.Windows.Window>)
        mainErrorMessageQueue
        dialogErrorMessageQueue
        timePointQueue
        looper
        =
        if System.Object.ReferenceEquals(__, null) then
            __ <- Bindings(title, assemblyVersion, workStatisticWindowFactory, mainErrorMessageQueue, dialogErrorMessageQueue, timePointQueue, looper)
            __
        else __

    static member ToList title assemblyVersion (workStatisticWindowFactory: System.Func<System.Windows.Window>) mainErrorMessageQueue dialogErrorMessageQueue timePointQueue looper =
        Utils.bindings<Binding>
            (Bindings.Instance title assemblyVersion workStatisticWindowFactory mainErrorMessageQueue dialogErrorMessageQueue timePointQueue looper)
            props

    member val Title : Binding =
        nameof __.Title |> Binding.oneWay (fun _ -> title)

    member val AssemblyVersion : Binding =
        nameof __.AssemblyVersion |> Binding.oneWay (fun _ -> assemblyVersion)

    member val ErrorMessageQueue : Binding =
        nameof __.ErrorMessageQueue |> Binding.oneWay (fun _ -> mainErrorMessageQueue)

    // -------------------------------------------------------------
    
    member val TimePointList : Binding =
        nameof __.TimePointList
            |> Binding.SubModel.required (fun () -> TimePointListModel.Bindings.ToList())
            |> Binding.mapModel _.TimePointList
            |> Binding.mapMsg Msg.TimePointListModelMsg

    member val StartTimePointCommand : Binding =
        nameof __.StartTimePointCommand |> Binding.cmdParam (fun id -> (id :?> System.Guid) |> Msg.StartTimePoint)

    member val PlayStopCommand : Binding =
        nameof __.PlayStopCommand |> Binding.cmdParam (fun id -> (id :?> System.Guid) |> Msg.PlayStopCommand)

    member val IsPlaying : Binding =
        nameof __.IsPlaying |> Binding.oneWay (fun m -> m.Player |> PlayerModel.isPlaying)

    member val ActiveTime : Binding =
        nameof __.ActiveTime |> Binding.oneWay (fun m -> m.Player |> PlayerModel.getRemainingTimeSpan)

    // ----------------------------------------------------
    /// For the test purpose
    //member val MinimizeCommand : Binding =
    //    nameof __.MinimizeCommand |> Binding.cmd (Msg.MinimizeAllRestoreApp)

    /// For the test purpose
    member val SendToChatBotCommand : Binding =
        nameof __.SendToChatBotCommand
        |> Binding.cmd (fun m ->
            match m.Player.ActiveTimePoint with
            | Some tp ->
                Msg.SendToChatBot $"It's time to {tp.Name}!!"
            | None ->
                Msg.SendToChatBot $"It's time!!"
        )

    // ----------------------------------------------------

    member val IsTimePointsShown : Binding =
        nameof __.IsTimePointsShown |> Binding.twoWay (_.IsTimePointsShown, Msg.SetIsTimePointsShown)

    // ------------------------------------------------------

    member val CurrentWork : Binding =
        nameof __.CurrentWork
            |> Binding.SubModel.opt (WorkModel.Bindings.ToList)
            |> Binding.mapModel _.CurrentWork
            |> Binding.mapMsg Msg.WorkModelMsg

    member val IsCurrentWorkSet : Binding =
        nameof __.IsCurrentWorkSet |> Binding.oneWay (_.CurrentWork >> Option.isSome)

    // ------------------------------------------------------
    member val AppDialog : Binding =
        nameof __.AppDialog
            |> Binding.SubModel.required (fun () -> AppDialogModel.Bindings.ToList(dialogErrorMessageQueue))
            |> Binding.mapModel _.AppDialog
            |> Binding.mapMsg Msg.AppDialogModelMsg

    member val WorkSelector : Binding =
        nameof __.WorkSelector
            |> Binding.SubModel.opt (WorkSelectorModel.Bindings.ToList)
            |> Binding.mapModel _.WorkSelector
            |> Binding.mapMsg Msg.WorkSelectorModelMsg

    member val IsWorkSelectorLoaded : Binding =
        nameof __.IsWorkSelectorLoaded |> Binding.twoWay (_.WorkSelector >> Option.isSome, Msg.SetIsWorkSelectorLoaded)

    member val IsWorkStatisticShown : Binding =
        nameof __.IsWorkStatisticShown |> Binding.twoWay (_.DailyStatisticList >> Option.isSome, Msg.SetIsWorkStatisticShown)

    member val WorkStatisticWindow : Binding =
        nameof __.WorkStatisticWindow
            |> Binding.subModelWin (
                (_.DailyStatisticList >> WindowState.ofOption),
                snd,
                Msg.DailyStatisticListModelMsg,
                (fun () -> DailyStatisticListModel.Bindings.ToList(dialogErrorMessageQueue)),
                (fun _ _ -> workStatisticWindowFactory.Invoke()),
                isModal = false
            )

    member val Player : Binding =
        nameof __.Player
            |> Binding.SubModel.required (PlayerModel.Bindings.ToList)
            |> Binding.mapModel _.Player
            |> Binding.mapMsg Msg.PlayerModelMsg

(*
let bindings title assemblyVersion errorMessageQueue : Binding<MainModel, Msg> list =
    [
        // "BotSettingsModel"
        //     |> Binding.SubModel.opt BotSettingsModel.Bindings.bindings
        //     |> Binding.mapModel _.BotSettingsModel
        //     |> Binding.mapMsg MainModel.Msg.BotSettingsMsg
        // 
        // "TimePointsGeneratorModel"
        //     |> Binding.SubModel.opt TimePointsGeneratorModel.Bindings.bindings
        //     |> Binding.mapModel _.TimePointsGeneratorModel
        //     |> Binding.mapMsg MainModel.Msg.TimePointsGeneratorMsg
        // 
        // "HasTimePointsGenerator" |> Binding.twoWay (_.TimePointsGeneratorModel >> Option.isSome, Msg.EraseTimePointsGeneratorModel)

        // "LoadTimePointsGeneratorCommand"
        //     |> Binding.cmdIf (fun model ->
        //         match model.TimePointsGeneratorModel with
        //         | None -> Msg.InitializeTimePointsGeneratorModel |> Some
        //         | Some _ -> None
        //     )

        // "TryStoreAndSetTimePointsCommand"
        //     |> Binding.cmdIf (fun m ->
        //         m.TimePointsGeneratorModel
        //         |> Option.bind (fun tpModel ->
        //             if not tpModel.IsPatternWrong then
        //                 Msg.LoadTimePoints (tpModel.TimePoints |> List.map _.TimePoint) |> Some
        //             else
        //                 None
        //         )
        //     )

        // "IsDialogOpened" |> Binding.oneWay (_.BotSettingsModel >> Option.isSome)

        // "LoadBotSettingsModelCommand" |> Binding.cmd Msg.LoadBotSettingsModel
    ]
*)


