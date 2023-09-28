using DCL.Controllers;

namespace DCL
{
    public class DataStore_ContentModeration
    {
        public enum AdultContentAgeConfirmationResult
        {
            Accepted,
            Rejected,
        }

        public readonly BaseVariable<bool> adultContentSettingEnabled = new (false);
        public readonly BaseVariable<bool> adultContentAgeConfirmationVisible = new (false);
        public readonly BaseVariable<AdultContentAgeConfirmationResult> adultContentAgeConfirmationResult = new (AdultContentAgeConfirmationResult.Accepted);
        public readonly BaseVariable<(bool isVisible, SceneContentCategory rating)> reportingScenePanelVisible = new ((false, SceneContentCategory.TEEN));
    }
}
