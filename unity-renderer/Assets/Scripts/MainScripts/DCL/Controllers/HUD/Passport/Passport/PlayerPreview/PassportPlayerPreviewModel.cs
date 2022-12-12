namespace DCL.Social.Passports
{
    public record PassportPlayerPreviewModel
    {
        public readonly bool TutorialEnabled;

        public PassportPlayerPreviewModel(bool tutorialEnabled)
        {
            this.TutorialEnabled = tutorialEnabled;
        }
    }
}
