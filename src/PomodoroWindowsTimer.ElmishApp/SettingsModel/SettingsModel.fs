namespace PomodoroWindowsTimer.ElmishApp.Models

type SettingsModel =
    {
        BotSettingsModel: BotSettingsModel
        TimePointsSettingsModel: TimePointsSettingsModel
    }

module SettingsModel =

    type Msg =
        | BotSettingsModelMsg of BotSettingsModel.Msg
        | TimePointsSettingsModelMsg of TimePointsSettingsModel.Msg

    open Elmish

    let init botConfiguration timePointPrototypeStore patternSettings =
        let botSettings = BotSettingsModel.init botConfiguration
        let (timePointsSettings, cmd) = TimePointsSettingsModel.init timePointPrototypeStore patternSettings
        {
            BotSettingsModel = botSettings
            TimePointsSettingsModel = timePointsSettings
        }
        , Cmd.map TimePointsSettingsModelMsg cmd
