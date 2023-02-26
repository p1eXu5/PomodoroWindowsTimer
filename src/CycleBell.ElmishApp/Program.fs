module CycleBell.ElmishApp.Program

open Microsoft.Extensions.Logging
open System
open Serilog
open Serilog.Extensions.Logging
open Elmish.WPF
open CycleBell.Types
open CycleBell.Abstrractions
open CycleBell.TimePointQueue
open CycleBell.Looper
open CycleBell.ElmishApp
open CycleBell.ElmishApp.Models

let mainModel = ViewModel.designInstance (MainModel.initForDesign ()) (MainModel.Bindings.bindings ())

let internal main (window, errorQueue, settingsManager) =
    let logger =
        LoggerConfiguration()
#if DEBUG
            .MinimumLevel.Override("Elmish.WPF.Update", Events.LogEventLevel.Error)
            .MinimumLevel.Override("Elmish.WPF.Bindings", Events.LogEventLevel.Error)
            .MinimumLevel.Override("Elmish.WPF.Performance", Events.LogEventLevel.Error)
            .MinimumLevel.Override(nameof (CycleBell.ElmishApp), Events.LogEventLevel.Error)
#else
            .MinimumLevel.Override("Elmish.WPF.Update", Events.LogEventLevel.Information)
            .MinimumLevel.Override("Elmish.WPF.Bindings", Events.LogEventLevel.Information)
            .MinimumLevel.Override("Elmish.WPF.Performance", Events.LogEventLevel.Information)
            .MinimumLevel.Override(nameof (CycleBell.ElmishApp), Events.LogEventLevel.Information)
#endif
            //.WriteTo.Console(outputTemplate="[{Timestamp:HH:mm:ss:fff} {Level:u3}] [{SourceContext:l}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.Console(outputTemplate="[{Timestamp:HH:mm:ss:fff} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.Debug(outputTemplate="[{Timestamp:HH:mm:ss:fff} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            //.WriteTo.Seq("http://localhost:5341")
            .CreateLogger()

    let loggerFactory = new SerilogLoggerFactory(logger)

    // let mainModelLogger : ILogger = loggerFactory.CreateLogger(nameof (CycleBell.ElmishApp.Models.MainModel))

    let timePoints =
        [
            { Name = "1"; TimeSpan = TimeSpan.FromMinutes(25); Kind = Work }
            { Name = "2"; TimeSpan = TimeSpan.FromMinutes(5); Kind = Break }
            { Name = "3"; TimeSpan = TimeSpan.FromMinutes(25); Kind = Work }
            { Name = "4"; TimeSpan = TimeSpan.FromMinutes(5); Kind = Break }
            { Name = "5"; TimeSpan = TimeSpan.FromMinutes(25); Kind = Work }
            { Name = "6"; TimeSpan = TimeSpan.FromMinutes(10); Kind = Break }
        ]

    let testTimePoints =
        [
            { Name = "1"; TimeSpan = TimeSpan.FromSeconds(5); Kind = Work }
            { Name = "2"; TimeSpan = TimeSpan.FromSeconds(5); Kind = Break }
        ]

    let timePointQueue = new TimePointQueue(testTimePoints)
    let looper = new Looper((timePointQueue :> ITimePointQueue), 200)

    let subscribe _ =
        let effect dispatch =
            let onLooperEvt =
                fun evt ->
                    async {
                        do dispatch (MainModel.Msg.LooperMsg evt)
                    }

            looper.AddSubscriber(onLooperEvt)

        [ effect ]

    looper.Start()


    WpfProgram.mkProgram 
        (fun () -> MainModel.init settingsManager errorQueue testTimePoints) 
        (MainModel.Program.update looper)
        MainModel.Bindings.bindings
    |> WpfProgram.withLogger loggerFactory
    |> WpfProgram.withSubscription subscribe
    |> WpfProgram.startElmishLoop window
