namespace PomodoroWindowsTimer.Abstractions

open System
open PomodoroWindowsTimer.Types


type IExcelSheet =
    interface
        abstract AddHeaders: unit -> Result<StartRow, string>
        abstract AddRows: date: DateOnly -> startTime: TimeOnly -> rows: ExcelRow seq -> StartRow -> Result<StartRow, string>
    end

type IExcelBook =
    interface
        abstract Create: filePath: string -> Result<IExcelSheet, string>
        abstract Save: excelSheet: IExcelSheet -> Result<unit, string>
    end
