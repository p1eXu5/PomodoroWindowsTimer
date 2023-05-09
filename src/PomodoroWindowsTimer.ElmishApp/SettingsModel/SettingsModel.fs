namespace PomodoroWindowsTimer.ElmishApp.Models

type SettingsModel =
    {
        SelectedSettingsIndex: int
        BotSettingsModel: BotSettingsModel option
        TimePointsSettingsModel: TimePointsSettingsModel option
    }

module SettingsModel =

    type Msg =
        | SetSelectedSettingsIndex of int
        | BotSettingsModelMsg of BotSettingsModel.Msg
        | TimePointsSettingsModelMsg of TimePointsSettingsModel.Msg

    open Elmish

    let init () =
        {
            SelectedSettingsIndex = -1
            BotSettingsModel = None
            TimePointsSettingsModel = None
        }


    let getSelectedSettingsIndex m = m.SelectedSettingsIndex

    let setSelectedSettingsIndex ind m =
        { m with SelectedSettingsIndex = ind }

    // let init botConfiguration timePointPrototypeStore patternSettings =
    //     let botSettings = BotSettingsModel.init botConfiguration
    //     let (timePointsSettings, cmd) = TimePointsSettingsModel.init timePointPrototypeStore patternSettings
    //     {
    //         BotSettingsModel = botSettings
    //         TimePointsSettingsModel = timePointsSettings
    //     }
    //     , Cmd.map TimePointsSettingsModelMsg cmd
