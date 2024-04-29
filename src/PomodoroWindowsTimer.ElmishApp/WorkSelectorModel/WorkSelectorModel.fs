namespace PomodoroWindowsTimer.ElmishApp.Models

type WorkSelectorModel =
    | WorkList of WorkListModel
    | CreatingWork of WorkModel
    | UpdatingWork of WorkModel

module WorkSelectorModel =

    type Msg =
        | WorkListModelMsg of WorkListModel.Msg
        | CreatingWorkModelMsg of WorkModel.Msg
        | UpdatingWorkModelMsg of WorkModel.Msg

    [<Struct; RequireQualifiedAccess>]
    type SubmodelId =
        | WorkListId
        | CreatingWorkId
        | UpdatingWorkId

    open Elmish

    let init () =
        let (m, cmd) = WorkListModel.init ()
        m |> WorkSelectorModel.WorkList
        , Cmd.map Msg.WorkListModelMsg cmd

    let submodelId = function
        | WorkList _ -> SubmodelId.WorkListId
        | CreatingWork _ -> SubmodelId.CreatingWorkId
        | UpdatingWork _ -> SubmodelId.UpdatingWorkId

    let workListModel = function
        | WorkList m -> m |> Some
        | _ -> None

    let withWorkListModel workListModel (_: WorkSelectorModel) =
        workListModel |> WorkSelectorModel.WorkList

    let creatingWorkModel = function
        | CreatingWork m -> m |> Some
        | _ -> None

    let withCreatingWorkModel creatingWorkModel (_: WorkSelectorModel) =
        creatingWorkModel |> WorkSelectorModel.CreatingWork

    let updatingWorkModel = function
        | UpdatingWork m -> m |> Some
        | _ -> None

    let withUpdatingWorkModel updatingWorkModel (_: WorkSelectorModel) =
        updatingWorkModel |> WorkSelectorModel.UpdatingWork
