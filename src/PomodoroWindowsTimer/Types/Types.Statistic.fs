namespace PomodoroWindowsTimer.Types

open System

type Statistic =
    {
        Period: DateTimePeriod
        WorkTime: TimeSpan
        BreakTime: TimeSpan
        TimePointNameStack: string list
    }

type WorkStatistic =
    {
        Work: Work
        Statistic: Statistic option
    }

type DailyStatistic =
    {
        Date: DateOnly
        WorkStatistic: WorkStatistic list
    }

// ------------------------------- modules

module Statistic =
    /// 14 * 25 min pomodoro + 1 * 15 min pomodoro + 11 * 5 min break + 3 * 20 min long break
    [<Literal>]
    let OVERALL_MINUTES_PER_DAY_MAX = 8.0 * 60.0

    [<Literal>]
    let WORK_MINUTES_PER_DAY_MAX = 25. * 4. * 3. + 25. + 25. + 15.

    [<Literal>]
    let BREAK_MINUTES_PER_DAY_MAX =
        OVERALL_MINUTES_PER_DAY_MAX - WORK_MINUTES_PER_DAY_MAX

    let breakMinutesPerDayMax = TimeSpan.FromMinutes(BREAK_MINUTES_PER_DAY_MAX)
    let workMinutesPerDayMax = TimeSpan.FromMinutes(WORK_MINUTES_PER_DAY_MAX)
    let overallMinutesPerDayMax = TimeSpan.FromMinutes(OVERALL_MINUTES_PER_DAY_MAX)

    let total (statistic: Statistic) =
        statistic.BreakTime + statistic.WorkTime

    /// Returns difference
    let chooseNotCompleted (statistic: Statistic) =
        let total = statistic.BreakTime + statistic.WorkTime
        if total < overallMinutesPerDayMax then
            overallMinutesPerDayMax - total |> Some
        else
            None
