namespace DCL
{
    public class DataStore_ContentModeration
    {
        public readonly BaseVariable<bool> adultContentSettingEnabled = new (false);
        public readonly BaseVariable<bool> adultContentAgeConfirmationVisible = new (false);
        public readonly BaseVariable<bool> resetAdultContentSetting = new (false);
    }
}
