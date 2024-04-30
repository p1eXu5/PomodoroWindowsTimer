namespace PomodoroWindowsTimer.ElmishApp.Models

open Elmish.Extensions

type WorkListModel =
    {
        Works: AsyncDeferred<WorkModel list>
        SelectedWorkId: int option
    }


module WorkListModel =

    type Msg =
        | LoadWorkList of AsyncOperation<unit, Result<WorkModel list, string>>
        | SetSelectedWorkId of int option
        | WorkModelMsg of int * WorkModel.Msg

    module MsgWith =

        let (|``Start of LoadWorkList``|_|) (model: WorkListModel) (msg: Msg) =
            match msg with
            | Msg.LoadWorkList (AsyncOperation.Start _) ->
                model.Works |> AsyncDeferred.tryInProgressWithCancellation
            | _ -> None

        let (|``Finish of LoadWorkList``|_|) (model: WorkListModel) (msg: Msg) =
            match msg with
            | Msg.LoadWorkList (AsyncOperation.Finish (res, cts)) ->
                model.Works |> AsyncDeferred.chooseRetrievedResultWithin res cts
            | _ -> None
 
    [<RequireQualifiedAccess; Struct>]
    type Intent =
        | None
        | SwitchToCreateWork
        | Select
        | Edit of workModel: WorkModel * selectedWorkId: int option

    [<AutoOpen>]
    module Intent =

        let withNoIntent (model, cmd) =
            (model, cmd, Intent.None)

        let withSwitchToCreateWorkIntent(model, cmd) =
            (model, cmd, Intent.SwitchToCreateWork)

        let withSelectIntent (model, cmd) =
            (model, cmd, Intent.Select)

    open Elmish

    let init selectedWorkId =
        {
            Works = AsyncDeferred.NotRequested
            SelectedWorkId = selectedWorkId
        }
        , Cmd.ofMsg (AsyncOperation.startUnit Msg.LoadWorkList)


    let withSelectedWorkId id (model: WorkListModel) =
        { model with SelectedWorkId = id }

    let withWorks deff (model: WorkListModel) =
        { model with Works = deff }

    let selectedWorkModel (model: WorkListModel) =
        model.SelectedWorkId
        |> Option.bind (fun id ->
            model.Works
            |> AsyncDeferred.chooseRetrieved
            |> Option.bind (List.tryFind (_.Work >> _.Id >> (=) id))
        )
