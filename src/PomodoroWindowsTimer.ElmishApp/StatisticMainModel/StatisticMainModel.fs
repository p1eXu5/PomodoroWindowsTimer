namespace PomodoroWindowsTimer.ElmishApp.Models

open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.Abstractions

type StatisticMainModel =
    {
        DailyStatisticListModel: DailyStatisticListModel option
        WorkListModel: WorkListModel option
    }

module StatisticMainModel =

    type Msg =
        | DailyStatisticListModelMsg of DailyStatisticListModel.Msg
        | WorkListModelMsg of WorkListModel.Msg
        | SetSelectedTabInd of int
        | CloseWindow

    module MsgWith =

        let (|DailyStatisticListModelMsg|_|) (model: StatisticMainModel) (msg: Msg) =
            match model.DailyStatisticListModel with
            | Some dailyStatisticListModel ->
                match msg with
                | DailyStatisticListModelMsg msg -> Some (msg, dailyStatisticListModel)
                | _ -> None
            | None -> None

        let (|WorkListModelMsg|_|) (model: StatisticMainModel) (msg: Msg) =
            match model.WorkListModel with
            | Some workListModel ->
                match msg with
                | WorkListModelMsg msg -> Some (msg, workListModel)
                | _ -> None
            | None -> None

        let (|SelectDailyStatisticListTab|_|) (model: StatisticMainModel) (msg: Msg) =
            match model.DailyStatisticListModel with
            | Some _ -> None
            | None ->
                match msg with
                | SetSelectedTabInd ind when ind = 0 -> Some ()
                | _ -> None

        let (|SelectWorkListTab|_|) (model: StatisticMainModel) (msg: Msg) =
            match model.WorkListModel with
            | Some _ -> None
            | None ->
                match msg with
                | SetSelectedTabInd ind when ind = 1 -> Some ()
                | _ -> None


    // --------------------

    [<RequireQualifiedAccess; Struct>]
    type Intent =
        | None
        | CloseWindow

    module Intent =

        let fromDailyStatisticListModelIntent (intent: DailyStatisticListModel.Intent) =
            match intent with
            | DailyStatisticListModel.Intent.None -> Intent.None
            | DailyStatisticListModel.Intent.CloseDialogRequested -> Intent.CloseWindow

        let fromWorkListModelIntent (intent: WorkListModel.Intent) =
            match intent with
            | WorkListModel.Intent.CloseDialogRequested -> Intent.CloseWindow
            | _ -> Intent.None

    // --------------------

    open Elmish

    let init (userSettings: IUserSettings) timeProvider =
        let (dailyStatisticListModel, dailyStatisticListModelCmd) = DailyStatisticListModel.init userSettings timeProvider
        {
            DailyStatisticListModel = dailyStatisticListModel |> Some
            WorkListModel = None
        }
        , Cmd.map DailyStatisticListModelMsg dailyStatisticListModelCmd


    // accessors:

    let withDailyStatisticListModel (dailyStatisticListModel: DailyStatisticListModel) (model: StatisticMainModel) =
        { model with DailyStatisticListModel = Some dailyStatisticListModel }

    let withWorkListModel (workListModel: WorkListModel) (model: StatisticMainModel) =
        { model with WorkListModel = Some workListModel }

