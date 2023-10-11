namespace DCL.ContentModeration
{
    public interface IContentModerationAnalytics
    {
        void OpenReportForm(string placeId);
        void CloseReportForm(string placeId, bool isCancelled);
        void SubmitReportForm(string placeId, string rating, string[] issues, string comment);
        void ClickLearnMoreContentModeration(string placeId);
        void ErrorSendingReportingForm(string placeId);
        void OpenSettingsFromContentWarning();
        void SwitchAdultContentSetting(bool isEnabled);
    }
}
