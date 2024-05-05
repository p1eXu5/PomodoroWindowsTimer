namespace PomodoroWindowsTimer.ElmishApp.Models

type WorkSelectorModel =
    {
        SubModel: WorkSelectorSubModel
    }
and
    WorkSelectorSubModel =
        | WorkList of WorkListModel
        | CreatingWork of CreatingWorkModel
        | UpdatingWork of WorkModel * selectedWorkId: uint64 option

module WorkSelectorModel =

    type Msg =
        | WorkListModelMsg of WorkListModel.Msg
        | CreatingWorkModelMsg of CreatingWorkModel.Msg
        | UpdatingWorkModelMsg of WorkModel.Msg


    [<Struct; RequireQualifiedAccess>]
    type SubModelId =
        | WorkListId
        | CreatingWorkId
        | UpdatingWorkId

    [<RequireQualifiedAccess; Struct>]
    type Intent =
        | None
        | Close
        | SelectCurrentWork of WorkModel option
        | UnselectCurrentWork

    [<AutoOpen>]
    module Intent =

        let withNoIntent (model, cmd) =
            (model, cmd, Intent.None)

        let withCloseIntent (model, cmd) =
            (model, cmd, Intent.Close)

        let withSelectIntent workModel (model, cmd) =
            (model, cmd, Intent.SelectCurrentWork workModel)

    open Elmish

    let init selectedWorkId =
        let (m, cmd) = WorkListModel.init selectedWorkId
        {
            SubModel = m |> WorkSelectorSubModel.WorkList
        }
        , Cmd.map Msg.WorkListModelMsg cmd

    let subModelId (model: WorkSelectorModel) = 
        match model.SubModel with
        | WorkList _ -> SubModelId.WorkListId
        | CreatingWork _ -> SubModelId.CreatingWorkId
        | UpdatingWork _ -> SubModelId.UpdatingWorkId

    let workListModel =
        _.SubModel
        >> function
            | WorkList m -> m |> Some
            | _ -> None

    let withWorkListModel workListModel (model: WorkSelectorModel) =
        { model with SubModel = workListModel |> WorkSelectorSubModel.WorkList }

    let creatingWorkModel =
         _.SubModel
        >> function
            | CreatingWork m -> m |> Some
            | _ -> None

    let withCreatingWorkModel creatingWorkModel (model: WorkSelectorModel) =
        { model with SubModel = creatingWorkModel |> WorkSelectorSubModel.CreatingWork }

    let updatingWorkModel  =
         _.SubModel
        >> function
            | UpdatingWork (m, _) -> m |> Some
            | _ -> None

    let withUpdatingWorkModel updatingWorkModel selectedWorkId (model: WorkSelectorModel) =
        { model with SubModel = (updatingWorkModel, selectedWorkId) |> WorkSelectorSubModel.UpdatingWork }
