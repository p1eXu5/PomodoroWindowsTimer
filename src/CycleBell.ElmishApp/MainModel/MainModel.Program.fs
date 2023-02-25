module CycleBell.ElmishApp.MainModel.Program

    open Elmish
    open CycleBell.ElmishApp.Models
    open CycleBell.ElmishApp.Models.MainModel
    open CycleBell.Looper


    let update (looper: Looper) (msg: Msg) (model: MainModel) =
        match msg with
        | Msg.Play ->
            looper.Resume()
            { model with LooperState = Playing }, Cmd.none

        | Msg.Stop ->
            looper.Stop()
            { model with LooperState = Stopped }, Cmd.none

        | Msg.LooperMsg evt ->
            let activeTimePoint =
                match evt with
                | LooperEvent.TimePointTimeReduced tp
                | LooperEvent.TimePointStarted (tp, _) -> tp |> Some
                | _ -> model.ActiveTimePoint

            { model with ActiveTimePoint = activeTimePoint }, Cmd.none

        | Msg.OnError ex ->
            model.ErrorQueue.EnqueuError(ex.Message)
            model, Cmd.none
