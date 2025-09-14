namespace PomodoroWindowsTimer.ElmishApp.Models

open System
open System.Threading
open PomodoroWindowsTimer
open Elmish.Extensions
open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.Abstractions

/// Responsible for work event projection to database
type CurrentWorkModel =
    {
        Work: Work option
        IsPlaying: bool
    }
    member this.Id = this.Work |> Option.map _.Id

module CurrentWorkModel =

    type Msg =
        | SetCurrentWorkIfNone of Result<Work, string>
        | SetWork of Work
        | UnsetWork
        | LooperMsg of LooperEvent
        | OnError of string
        | OnExn of exn

    open Elmish

    let init (currentWorkSettings: ICurrentWorkItemSettings) (workRepo: IWorkRepository) =
        let currentWork = currentWorkSettings.CurrentWork
        let cmd =
            match currentWork with
            | None -> Cmd.none
            | Some work ->
                Cmd.OfTask.perform
                    (workRepo.FindByIdOrCreateAsync work)
                    CancellationToken.None
                    Msg.SetCurrentWorkIfNone
        { Work = None; IsPlaying = false }, cmd

    // =========
    // accessors
    // =========
    let inline withWork work (model: CurrentWorkModel) =
        { model with Work = work |> Some }

    let inline withoutWork (model: CurrentWorkModel) =
        { model with Work = None }

    let inline withIsPlaying isPlaying (model: CurrentWorkModel) =
        { model with IsPlaying = isPlaying }
