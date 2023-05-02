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


    let init botConfiguration kindAliasesStore =
        let botSettings = BotSettingsModel.init botConfiguration
        let timePointsSettings = TimePointsSettingsModel.init kindAliasesStore
        {
            BotSettingsModel = botSettings
            TimePointsSettingsModel = timePointsSettings
        }