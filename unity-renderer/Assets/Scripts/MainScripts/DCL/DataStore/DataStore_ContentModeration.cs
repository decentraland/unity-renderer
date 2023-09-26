namespace DCL
{
    public class DataStore_ContentModeration
    {
        public enum AdultContentAgeConfirmationResult
        {
            ACCEPTED,
            REJECTED,
        }

        public readonly BaseVariable<bool> adultContentSettingEnabled = new (false);
        public readonly BaseVariable<bool> adultContentAgeConfirmationVisible = new (false);
        public readonly BaseVariable<AdultContentAgeConfirmationResult> adultContentAgeConfirmationResult = new (AdultContentAgeConfirmationResult.ACCEPTED);
    }
}
