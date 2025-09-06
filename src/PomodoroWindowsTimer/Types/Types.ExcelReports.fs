namespace PomodoroWindowsTimer.Types

open System
open System.Diagnostics

type StartRow = int

[<DebuggerDisplay("Num = {Num}, End = {End}")>]
type IdleExcelRow =
    {
        Num: int
        End: TimeOnly
    }

[<DebuggerDisplay("Num = {Num}, End = {End}, WorkId = {Work.Id}")>]
type WorkExcelRow =
    {
        Num: int
        Work: Work
        End: TimeOnly
    }

type ExcelRow =
    | WorkExcelRow of WorkExcelRow
    | IdleExcelRow of IdleExcelRow


// ------------------------------- modules

module ExcelRow =

    let createWorkExcelRow num (work: Work) (endTimeOnly: TimeOnly) =
        {
            Num = num
            Work = work
            End = endTimeOnly
        }
        |> ExcelRow.WorkExcelRow

    let createIdleExcelRow num (endTimeOnly: TimeOnly) =
        {
            Num = num
            End = endTimeOnly
        }
        |> ExcelRow.IdleExcelRow

    let addTime (time: TimeSpan) = function
        | ExcelRow.WorkExcelRow w ->
            { w with End = w.End.Add(time) } |> ExcelRow.WorkExcelRow
        | ExcelRow.IdleExcelRow idle ->
            { idle with End = idle.End.Add(time) } |> ExcelRow.IdleExcelRow

    let subTime (time: TimeSpan) = function
        | ExcelRow.WorkExcelRow w ->
            { w with End = w.End.Add(-time) } |> ExcelRow.WorkExcelRow
        | ExcelRow.IdleExcelRow idle ->
            { idle with End = idle.End.Add(-time) } |> ExcelRow.IdleExcelRow

    let num = function
        | ExcelRow.WorkExcelRow w ->
            w.Num
        | ExcelRow.IdleExcelRow idle ->
            idle.Num

    let endAddTime (time: TimeSpan) = function
        | ExcelRow.WorkExcelRow w ->
            w.End.Add(time)
        | ExcelRow.IdleExcelRow idle ->
            idle.End.Add(time)

    let endTimeOnly = function
        | ExcelRow.WorkExcelRow w ->
            w.End
        | ExcelRow.IdleExcelRow idle ->
            idle.End

module WorkExcelRow =

    let subTime (time: TimeSpan) (row: WorkExcelRow) =
        { row with End = row.End.Add(-time) }

    let addTime (time: TimeSpan) (row: WorkExcelRow) =
        { row with End = row.End.Add(time) }

