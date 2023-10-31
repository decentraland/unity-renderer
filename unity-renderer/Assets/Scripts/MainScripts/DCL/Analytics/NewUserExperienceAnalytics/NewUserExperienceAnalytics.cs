using System.Collections.Generic;

public class NewUserExperienceAnalytics : INewUserExperienceAnalytics
{
    private const string AVATAR_EDIT_SUCCESS_NUX = "avatar_edit_success_nux";
    private const string TERMS_OF_SERVICE_SUCCESS_NUX = "terms_of_service_success_nux";
    private const string CLICK_ONBOARDING_JUMP_IN = "click_onboarding_jump_in";

    private readonly IAnalytics analytics;

    public NewUserExperienceAnalytics(IAnalytics analytics)
    {
        this.analytics = analytics;
    }

    public void AvatarEditSuccessNux()
    {
        SendAnalytic(AVATAR_EDIT_SUCCESS_NUX);
    }

    public void SendTermsOfServiceAcceptedNux()
    {
        SendAnalytic(TERMS_OF_SERVICE_SUCCESS_NUX);
    }

    public void SendClickOnboardingJumpIn(string nameChosen, string email)
    {
        Dictionary<string, string> data = new Dictionary<string, string>
        {
            { "name", nameChosen },
            { "email", email },
        };

        SendAnalytic(CLICK_ONBOARDING_JUMP_IN, data);
    }

    private static void SendAnalytic(string eventName, Dictionary<string, string> data = null)
    {
        data ??= new Dictionary<string, string>();

        IAnalytics analytics = DCL.Environment.i?.platform?.serviceProviders?.analytics;
        analytics?.SendAnalytic(eventName, data);
    }
}
