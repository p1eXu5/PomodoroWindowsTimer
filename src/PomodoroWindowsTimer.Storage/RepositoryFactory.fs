namespace PomodoroWindowsTimer.Storage

open Microsoft.Extensions.Logging
open PomodoroWindowsTimer.Abstractions

type RepositoryFactory(
    options: IDatabaseSettings,
    timeProvider: System.TimeProvider,
    loggerFactory: ILoggerFactory
) =
    interface IRepositoryFactory with
        member _.GetWorkRepository () : IWorkRepository = 
            WorkRepository(options, timeProvider, loggerFactory.CreateLogger<WorkRepository>()) :> IWorkRepository

        member _.GetWorkEventRepository () : IWorkEventRepository = 
            WorkEventRepository(options, timeProvider, loggerFactory.CreateLogger<WorkEventRepository>()) :> IWorkEventRepository

        member _.GetActiveTimePointRepository () : IActiveTimePointRepository = 
            ActiveTimePointRepository(options, timeProvider, loggerFactory.CreateLogger<ActiveTimePointRepository>()) :> IActiveTimePointRepository
