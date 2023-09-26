namespace DCL
{
    public class DataStore_Settings
    {
        public readonly BaseVariable<bool> isInitialized = new BaseVariable<bool>(false);
        public readonly BaseVariable<bool> settingsPanelVisible = new BaseVariable<bool>(false);
        public readonly BaseVariable<bool> profanityChatFilteringEnabled = new BaseVariable<bool>();
    }
}
