namespace PomodoroWindowsTimer.ElmishApp.WorkEventListModel

open Microsoft.Extensions.Logging

open Elmish
open Elmish.Extensions

open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Logging
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.WorkEventListModel

module Program =

    let update (repositoryFactory: WorkEventStore) (errorMessageQueue: IErrorMessageQueue) (logger: ILogger<WorkEventListModel>) msg model =
        match msg with
        | MsgWith.``Start of LoadEventList`` model (workId, period, deff, cts) ->
            model |> withWorkEvents deff
            , Cmd.OfTask.perform
                repositoryFactory.ProjectByWorkIdByPeriod 
                (workId, period, cts.Token)
                (AsyncOperation.finishWithin Msg.LoadEventList cts)

        | MsgWith.``Finish of LoadEventList`` model res ->
            match res with
            | Error err ->
                do errorMessageQueue.EnqueueError err
                logger.LogError(err)
                model |> withWorkEvents AsyncDeferred.NotRequested |> withCmdNone
            | Ok deff ->
                model |> withWorkEvents deff |> withCmdNone

        | _ ->
            logger.LogNonProcessedMessage(msg, model)
            model |> withCmdNone

