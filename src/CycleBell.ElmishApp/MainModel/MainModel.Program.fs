module CycleBell.ElmishApp.MainModel.Program

open Elmish
open CycleBell.ElmishApp.Models
open CycleBell.ElmishApp.Models.MainModel
open CycleBell.Looper
open CycleBell.ElmishApp
open CycleBell.Types
open System.Threading.Tasks
open Elmish.Extensions
open CycleBell.ElmishApp.Abstractions


let update (botConfiguration: IBotConfiguration) (sendToBot: IBotConfiguration -> string -> Task<unit>) (looper: Looper) (msg: Msg) (model: MainModel) =
    match msg with
    | Msg.PickFirstTimePoint ->
        let atp = looper.PickFirst()
        { model with ActiveTimePoint = atp }, Cmd.none
    | Msg.Play ->
        looper.Resume()
        { model with LooperState = Playing }, Cmd.none

    | Msg.Stop ->
        looper.Stop()
        { model with LooperState = Stopped }, Cmd.none

    | Msg.LooperMsg evt ->
        let (activeTimePoint, cmd) =
            match evt with
            | LooperEvent.TimePointTimeReduced tp -> (tp |> Some, Cmd.none)
            | LooperEvent.TimePointStarted (tp, None) -> (tp |> Some, Cmd.none) // initial start
            | LooperEvent.TimePointStarted (tp, Some _) ->
                match tp.Kind with
#if DEBUG
                | Break -> (tp |> Some, Cmd.none)
                | Work -> (tp |> Some, Cmd.ofMsg SendToChatBot)
#else
                | Break -> (tp |> Some, Cmd.OfAsync.attempt Infrastructure.minimize () Msg.OnError)
                | Work -> (tp |> Some, Cmd.batch [ Cmd.OfAsync.attempt Infrastructure.restore () Msg.OnError; Cmd.ofMsg SendToChatBot ])
#endif
            | _ -> (model.ActiveTimePoint, Cmd.none)

        { model with ActiveTimePoint = activeTimePoint }, cmd

    | Minimize ->
        model, Cmd.OfAsync.attempt Infrastructure.minimize () Msg.OnError

    | SendToChatBot ->
        model, Cmd.OfTask.attempt (sendToBot botConfiguration) "It's time!!" Msg.OnError

    | StartTimePoint (Operation.Start id) ->
        model, Cmd.batch [ Cmd.ofMsg Stop; Cmd.OfAsync.either looper.TimePointQueue.Scroll id (Operation.Finish >> StartTimePoint) OnError ]

    | StartTimePoint (Operation.Finish _) ->
        model, Cmd.ofMsg Play

    | BotSettingsModelMsg bmsg ->
        let botSettingsModel = BotSettingsModel.Program.update botConfiguration bmsg model.BotSettingsModel
        { model with BotSettingsModel = botSettingsModel }, Cmd.none

    | Msg.OnError ex ->
        model.ErrorQueue.EnqueuError(ex.Message)
        model, Cmd.none
