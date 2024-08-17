namespace PomodoroWindowsTimer.ElmishApp.Models

type RollbackWorkListModel =
    {
        RollbackList: RollbackWorkModel list
    }

module RollbackWorkListModel =

    open PomodoroWindowsTimer.Types

    type Msg =
        | RollbackWorkModelMsg of WorkId * RollbackWorkModel.Msg
        | ApplyAndClose
        | Close

    //module MsgWith =

    //    let (|RollbackWorkModelMsg|_|) model msg =
    //        match msg, model.RollbackList |> 


    [<RequireQualifiedAccess>]
    type Intent =
        | None
        | ProcessRollbackAndClose
        | Close

    let init workSpenTimeList timePointKind timePointId time =
        {
            RollbackList = workSpenTimeList |> List.map (fun wst -> RollbackWorkModel.init wst timePointKind timePointId time)
        }

    let withRollbackList rollbackList (model: RollbackWorkListModel) =
        { model with RollbackList = rollbackList }


