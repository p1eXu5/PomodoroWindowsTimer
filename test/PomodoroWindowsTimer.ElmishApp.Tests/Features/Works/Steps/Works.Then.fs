﻿module rec PomodoroWindowsTimer.ElmishApp.Tests.Features.Works.Steps.Then

open System
open Microsoft.Extensions.DependencyInjection

open FsUnit
open p1eXu5.FSharp.Testing.ShouldExtensions
open NSubstitute

open PomodoroWindowsTimer.Types
open PomodoroWindowsTimer.Abstractions
open PomodoroWindowsTimer.ElmishApp
open PomodoroWindowsTimer.ElmishApp.Abstractions
open PomodoroWindowsTimer.ElmishApp.Models
open PomodoroWindowsTimer.ElmishApp.Models.MainModel

open PomodoroWindowsTimer.ElmishApp.Tests
open PomodoroWindowsTimer.ElmishApp.Tests.ScenarioCE

open PomodoroWindowsTimer.ElmishApp.Tests
open PomodoroWindowsTimer.ElmishApp.Tests.Features.Helpers
open NUnit.Framework
open Elmish.Extensions


let ``Current Work has been set to`` (expectedCurrentWork: Work) =
    scenario {
        let! (sut: ISut) = Scenario.getState
        
        match sut.MainModel.CurrentWork with
        | Some actualCurrentWork ->
            actualCurrentWork.Work.Number |> should equal expectedCurrentWork.Number
            actualCurrentWork.Work.Title |> should equal expectedCurrentWork.Title
            // Created/UpdatedAt dates is different cause work is created in database
        | None ->
            assertionExn "CurrentWork has not been set."
    }
    |> Scenario.log $"Then.``{nameof ``Current Work has been set to``}``"


let ``Current Work has not been set`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState
        
        match sut.MainModel.CurrentWork with
        | Some actualCurrentWork ->
            assertionExn $"CurrentWork has been set to {actualCurrentWork}."
        | None ->
            writeLine "CurrentWork has not been set"
    }
    |> Scenario.log $"Then.``{nameof ``Current Work has not been set``}``"


let ``Have no work events in db within`` (workId: uint64) =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let workEventRepository = sut.ServiceProvider.GetRequiredService<IWorkEventRepository>()

        match workEventRepository.ReadAll workId with
        | Ok workEvents ->
            workEvents |> shouldL be Empty $"Work {workId} have db events"
        | Error err ->
            assertionExn err
    }
    |> Scenario.log $"Then.``{nameof ``Have no work events in db within``}`` {workId} work Id"


let ``Work events in db exist`` (workId: uint64) (eventExpr: #Quotations.Expr seq) =
    scenario {
        let! (sut: ISut) = Scenario.getState
        let workEventRepository = sut.ServiceProvider.GetRequiredService<IWorkEventRepository>()

        match workEventRepository.ReadAll workId with
        | Ok workEvents ->
            workEvents |> Seq.length |> shouldL equal (eventExpr |> Seq.length) $"Actual work event length is {workEvents |> Seq.length}"
            Seq.zip workEvents eventExpr
            |> Seq.iter (fun (ev, expr) -> ev |> should be (ofCase expr))
        | Error err ->
            assertionExn err
    }
    |> Scenario.log $"Then.``{nameof ``Work events in db exist``}`` {workId} work Id"


let ``CreatingWork sub model has been shown`` () =
    Common.``CreatingWork sub model has been shown`` ()
    |> Scenario.log $"Then.``{nameof Common.``CreatingWork sub model has been shown``}``"

let ``WorkList sub model has been shown`` times =
    Common.``WorkList sub model has been shown`` times
    |> Scenario.log $"Then.``{nameof Common.``WorkList sub model has been shown``}``"


let ``WorkList sub model selected work is set on`` (workId: uint64) =
    scenario {
        let! (sut: ISut) = Scenario.getState
        match sut.MainModel.WorkSelector with
        | Some ws ->
            match ws.SubModel with
            | WorkSelectorSubModel.WorkList workList ->
                match workList.SelectedWorkId with
                | Some id -> id |> shouldL equal workId $"SelectedWorkId is {id}"
                | _ -> assertionExn $"SelectedWorkId is {workList.SelectedWorkId}"
            | _ -> assertionExn "Is not WorkList submodel"
        | _ -> assertionExn "WorkSelector is None"
    }
    |> Scenario.log $"Then.``{nameof ``WorkList sub model selected work is set on``}``"


let ``WorkList sub model selected work is not set`` () =
    scenario {
        let! (sut: ISut) = Scenario.getState
        match sut.MainModel.WorkSelector with
        | Some ws ->
            match ws.SubModel with
            | WorkSelectorSubModel.WorkList workList ->
                workList.SelectedWorkId |> shouldL equal None $"SelectedWorkId is {workList.SelectedWorkId}"
            | _ -> assertionExn "Is not WorkList submodel"
        | _ -> assertionExn "WorkSelector is None"
    }
    |> Scenario.log $"Then.``{nameof ``WorkList sub model selected work is set on``}``"


let ``WorkList sub model work list contains`` (work: Work) =
    scenario {
        let! (sut: ISut) = Scenario.getState
        match sut.MainModel.WorkSelector with
        | Some ws ->
            match ws.SubModel with
            | WorkSelectorSubModel.WorkList workListModel ->
                match workListModel.Works with
                | AsyncDeferred.Retrieved workList ->
                    workList |> List.map _.Title |> should contain work.Title
                    workList |> List.map _.Number |> should contain work.Number
                | _ -> assertionExn "WorkListModel Works are not retrieved."
            | _ -> assertionExn "Is not WorkList submodel."
        | _ -> assertionExn "WorkSelector is None."
    }
    |> Scenario.log $"Then.``{nameof ``WorkList sub model work list contains``}``"

let ``MainModel WorkSelector becomes None`` () =
    scenario {
        do! Scenario.modelSatisfiesWithin2Sec "MainModel.WorkSelector becomes None" (fun mainModel ->
            match mainModel.WorkSelector with
            | None ->
                writeLine "MainModel.WorkSelector is None"
                true
            | Some workSelector ->
                writeLine $"MainModel.WorkSelector is {workSelector}"
                false
        )
    }

