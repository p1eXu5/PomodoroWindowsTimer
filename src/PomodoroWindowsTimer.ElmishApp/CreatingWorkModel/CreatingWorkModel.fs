namespace PomodoroWindowsTimer.ElmishApp.Models

open Elmish.Extensions

type CreatingWorkModel =
    {
        Number: string option
        Title: string
        CreatingState: AsyncDeferred<int>
    }

module CreatingWorkModel =

    type Msg =
        | SetNumber of string option
        | SetTitle of string
        | CreateWork of AsyncOperation<unit, Result<int, string>>
        | Cancel

    [<RequireQualifiedAccess; Struct>]
    type Intent =
        | None
        | SwitchToWorkList of id: int
        | Cancel

    module MsgWith =

        let (|``Start of CreateWork``|_|) (model: CreatingWorkModel) (msg: Msg) =
            match msg, model.CreatingState with
            | Msg.CreateWork (AsyncOperation.Start _), AsyncDeferred.NotRequested ->
                model.CreatingState |> AsyncDeferred.tryInProgressWithCancellation
            | _ -> None

        let (|``Finish of CreateWork``|_|) (model: CreatingWorkModel) (msg: Msg) =
            match msg with
            | Msg.CreateWork (AsyncOperation.Finish (res, cts)) ->
                model.CreatingState |> AsyncDeferred.chooseRetrievedResultWithin res cts
            | _ -> None

    [<AutoOpen>]
    module Intent =

        let withNoIntent (model, cmd) =
            (model, cmd, Intent.None)

        let withSwitchToWorkListIntent id (model, cmd) =
            (model, cmd, Intent.SwitchToWorkList id)

        let withCancelIntent (model, cmd) =
            (model, cmd, Intent.Cancel)


    let init () =
        {
            Number = None
            Title = "New Work"
            CreatingState = AsyncDeferred.NotRequested
        }

    let withCreatingState deff (model: CreatingWorkModel) =
        { model with CreatingState = deff }

    let withNumber number (model: CreatingWorkModel) =
        { model with Number = number }

    let withTitle title (model: CreatingWorkModel) =
        { model with Title = title }

