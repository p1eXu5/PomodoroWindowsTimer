namespace PomodoroWindowsTimer.ElmishApp.Models

open System
open PomodoroWindowsTimer
open Elmish.Extensions

type WorkModel =
    {
        Work: PomodoroWindowsTimer.Types.Work
        Number: string option
        Title: string
        UpdateState: AsyncDeferred<DateTimeOffset>
        CreateNewState: AsyncDeferred<int * DateTimeOffset>
    }

module WorkModel =

    type Msg =
        | SetNumber of string option
        | SetTitle of string
        | Update of AsyncOperation<unit, Result<DateTimeOffset, string>>
        | CreateNew of AsyncOperation<unit, Result<(int * DateTimeOffset), string>>

    module MsgWith =

        let (|``Start of Update``|_|) (model: WorkModel) (msg: Msg) =
            match msg, model.CreateNewState with
            | Msg.Update (AsyncOperation.Start _), AsyncDeferred.NotRequested ->
                model.UpdateState |> AsyncDeferred.tryInProgressWithCancellation
            | _ -> None

        let (|``Finish of Update``|_|) (model: WorkModel) (msg: Msg) =
            match msg with
            | Msg.Update (AsyncOperation.Finish (res, cts)) ->
                model.UpdateState |> AsyncDeferred.chooseRetrievedResultWithin res cts
            | _ -> None

        let (|``Start of CreateNew``|_|) (model: WorkModel) (msg: Msg) =
            match msg, model.UpdateState with
            | Msg.CreateNew (AsyncOperation.Start _), AsyncDeferred.NotRequested ->
                model.CreateNewState |> AsyncDeferred.tryInProgressWithCancellation
            | _ -> None

        let (|``Finish of CreateNew``|_|) (model: WorkModel) (msg: Msg) =
            match msg with
            | Msg.CreateNew (AsyncOperation.Finish (res, cts)) ->
                model.CreateNewState |> AsyncDeferred.chooseRetrievedResultWithin res cts
            | _ -> None

    let init work =
        {
            Work = work
            Number = work.Number
            Title = work.Title
            UpdateState = AsyncDeferred.NotRequested
            CreateNewState = AsyncDeferred.NotRequested
        }

    let withNumber number (model: WorkModel) =
        { model with Number = number }

    let withTitle title (model: WorkModel) =
        { model with Title = title }

    let withUpdateState deff (model: WorkModel) =
        { model with UpdateState = deff }

    let withUpdatedWork updatedAt (model: WorkModel) =
        {
            model with
                Work.Number = model.Number
                Work.Title = model.Title
                Work.UpdatedAt = updatedAt
        }

    let withCreateNewState deff (model: WorkModel) =
        { model with CreateNewState = deff }

    let withCreatedWork id createdAt (model: WorkModel) =
        {
            model with
                Work.Id = id
                Work.Number = model.Number
                Work.Title = model.Title
                Work.CreatedAt = createdAt
                Work.UpdatedAt = createdAt
        }

    let isModified (model: WorkModel) =
        not (
            model.Work.Number |> Option.equalOrigin model.Number
            && model.Work.Title |> String.equalOrigin model.Title
        )

    let ifModifiedThen v (model: WorkModel) =
        if model |> isModified then
            v |> Some
        else
            None

