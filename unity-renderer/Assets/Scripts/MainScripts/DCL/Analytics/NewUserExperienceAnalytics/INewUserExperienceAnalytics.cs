public interface INewUserExperienceAnalytics
{
    void AvatarEditSuccessNux();
    void SendTermsOfServiceAcceptedNux();
    void SendClickOnboardingJumpIn(string nameChosen, string email);
}
