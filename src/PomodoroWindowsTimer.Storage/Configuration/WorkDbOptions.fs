namespace PomodoroWindowsTimer.Storage.Configuration

open System.ComponentModel.DataAnnotations

[<CLIMutable>]
type WorkDbOptions =
    {
        [<Required>]
        [<MinLength(16)>]
        ConnectionString: string
    }
    with
        static member SectionName = "WorkDb"
        member this.DbFilePath =
            let eqInd = this.ConnectionString.IndexOf('=')
            if eqInd >= 0 then
                this.ConnectionString.Substring(eqInd + 1, this.ConnectionString.Length - eqInd - 2)
            else
                this.ConnectionString

