namespace PomodoroWindowsTimer.ElmishApp.Models

open Elmish.Extensions
open PomodoroWindowsTimer.Types

type WorkEventListModel =
    {
        WorkEvents: AsyncDeferred<WorkEventModel list>
    }


module WorkEventListModel =

    type Msg =
        | LoadEventList of AsyncOperation<uint64 * DateOnlyPeriod, Result<WorkEventOffsetTime list, string>>
        | NoMsg

    module MsgWith =

        let (|``Start of LoadEventList``|_|) model msg =
            match msg with
            | Msg.LoadEventList (AsyncOperation.Start (workId, period)) ->
                model.WorkEvents |> AsyncDeferred.forceInProgressWithCancellation |> fun (deff, cts) -> (workId, period, deff, cts) |> Some
            | _ -> None

        let (|``Finish of LoadEventList``|_|) model msg =
            match msg with
            | Msg.LoadEventList (AsyncOperation.Finish (res, cts)) ->
                model.WorkEvents
                |> AsyncDeferred.chooseRetrievedResultWithin res cts
                |> Option.map (
                    Result.map (fun (_, res) ->
                        match res with
                        | [] -> []
                        | head :: tail ->
                            let sm =
                                WorkEventModel.init head.WorkEvent head.OffsetTime false None

                            let isPrevWork =
                                match sm with
                                | WorkEventModel.Work wm ->
                                    match wm.TimePoitName with
                                    | Some _ -> true
                                    | _ -> false
                                | _ -> false

                            tail
                            |> List.fold (fun (acc, isPrevWork, lastTimePointName) offsetTime ->
                                let sm = 
                                    WorkEventModel.init offsetTime.WorkEvent offsetTime.OffsetTime isPrevWork lastTimePointName
                                match sm with
                                | WorkEventModel.Work wm ->
                                    match wm.TimePoitName with
                                    | Some _ ->
                                        (sm :: acc, true, wm.TimePoitName)
                                    | None ->
                                        (sm :: acc, isPrevWork, lastTimePointName)
                                | WorkEventModel.Break wm ->
                                    match wm.TimePoitName with
                                    | Some _ ->
                                        (sm :: acc, false, wm.TimePoitName)
                                    | None ->
                                        (sm :: acc, isPrevWork, lastTimePointName)

                            ) ([sm], isPrevWork, sm |> WorkEventModel.timePointName )
                            |> fun (acc, _, _) -> acc |> List.rev
                        |> AsyncDeferred.Retrieved
                    )
                )
            | _ -> None


    open Elmish

    let init (workId: uint64) (period: DateOnlyPeriod) =
        { WorkEvents = AsyncDeferred.NotRequested }
        , Cmd.ofMsg (AsyncOperation.start Msg.LoadEventList (workId, period))

    let withWorkEvents deff (model: WorkEventListModel) =
        { model with WorkEvents = deff }

    let workEventModels (model: WorkEventListModel) : WorkEventModel list =
        match model.WorkEvents with
        | AsyncDeferred.Retrieved models -> models
        | _ -> List.empty



namespace PomodoroWindowsTimer.ElmishApp.WorkEventListModel

open Microsoft.Extensions.Logging

open Elmish
open Elmish.Extensions

open PomodoroWindowsTimer
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions

open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Logging
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.WorkEventListModel
open System

module Program =

    let update (workEventRepo: IWorkEventRepository) (errorMessageQueue: IErrorMessageQueue) (logger: ILogger<WorkEventListModel>) msg model =
        match msg with
        | MsgWith.``Start of LoadEventList`` model (workId, period, deff, cts) ->
            model |> withWorkEvents deff
            , Cmd.OfTask.perform (WorkEventOffsetTimeProjector.projectByWorkIdByPeriod workEventRepo workId period) cts.Token (AsyncOperation.finishWithin Msg.LoadEventList cts)

        | MsgWith.``Finish of LoadEventList`` model res ->
            match res with
            | Error err ->
                do errorMessageQueue.EnqueueError err
                logger.LogError(err)
                model |> withWorkEvents AsyncDeferred.NotRequested |> withCmdNone
            | Ok deff ->
                model |> withWorkEvents deff |> withCmdNone

        | _ ->
            logger.LogUnprocessedMessage(msg, model)
            model |> withCmdNone


open Elmish.WPF

type private Binding = Binding<WorkEventListModel, WorkEventListModel.Msg>

[<Sealed>]
type Bindings() =
    static let props = Utils.bindingProperties typeof<Bindings>
    static let mutable __ = Unchecked.defaultof<Bindings>
    static member Instance() =
        if System.Object.ReferenceEquals(__, null) then
            __ <- Bindings()
            __
        else __

    static member ToList () =
        Utils.bindings<Binding>
            (Bindings.Instance())
            props

    member val WorkEvents : Binding =
        nameof __.WorkEvents
            |> Binding.subModelSeq (WorkEventModel.Bindings.ToList, WorkEventModel.createdAt)
            |> Binding.mapModel (workEventModels >> List.toSeq)
            |> Binding.mapMsg (fun _ -> Msg.NoMsg)
