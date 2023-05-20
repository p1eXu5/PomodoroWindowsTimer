namespace PomodoroWindowsTimer.ElmishApp.Models

type SettingsModel =
    {
        SelectedSettingsIndex: int
        BotSettingsModel: BotSettingsModel option
        TimePointsGenerator: TimePointsGenerator option
    }

module SettingsModel =

    type Msg =
        | SetSelectedSettingsIndex of int
        | BotSettingsModelMsg of BotSettingsModel.Msg
        | TimePointsSettingsModelMsg of TimePointsGenerator.Msg

    open Elmish

    //let init () =
    //    {
    //        SelectedSettingsIndex = -1
    //        BotSettingsModel = None
    //        TimePointsGenerator = None
    //    }


    let getSelectedSettingsIndex m = m.SelectedSettingsIndex

    let setSelectedSettingsIndex ind m =
        { m with SelectedSettingsIndex = ind }

    let init botConfiguration timePointPrototypeStore patternSettings =
        let botSettings = BotSettingsModel.init botConfiguration
        let (timePointsSettings, cmd) = TimePointsGenerator.init timePointPrototypeStore patternSettings
        {
            SelectedSettingsIndex = -1
            BotSettingsModel = botSettings |> Some
            TimePointsGenerator = timePointsSettings |> Some
        }
        , Cmd.map TimePointsSettingsModelMsg cmd
