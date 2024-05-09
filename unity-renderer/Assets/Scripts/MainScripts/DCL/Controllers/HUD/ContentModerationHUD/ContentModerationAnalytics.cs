using System;
using System.Collections.Generic;

namespace DCL.ContentModeration
{
    public class ContentModerationAnalytics : IContentModerationAnalytics
    {
        private const string OPEN_REPORT_FORM = "open_report_form";
        private const string CLOSE_REPORT_FORM = "close_report_form";
        private const string SUBMIT_REPORT_FORM = "submit_report_form";
        private const string OPEN_LEARN_MORE = "click_learn_more_content_moderation";
        private const string SUBMIT_REPORT_FORM_ERROR = "submit_report_form_error";
        private const string OPEN_SETTINGS_FROM_CONTENT_WARNING = "open_settings_from_content_warning";
        private const string SWITCH_ADULT_CONTENT_SETTING = "switch_adult_content_setting";

        private string reportingTrackId;

        public void OpenReportForm(string placeId)
        {
            reportingTrackId = GenerateUniqueID();

            var data = new Dictionary<string, string>
            {
                { "place_id", placeId },
                { "report_track_id", reportingTrackId },
            };
            GenericAnalytics.SendAnalytic(OPEN_REPORT_FORM, data);
        }

        public void CloseReportForm(string placeId, bool isCancelled)
        {
            var data = new Dictionary<string, string>
            {
                { "place_id", placeId },
                { "report_track_id", reportingTrackId },
                { "cancelled", isCancelled.ToString() },
            };
            GenericAnalytics.SendAnalytic(CLOSE_REPORT_FORM, data);
        }

        public void SubmitReportForm(string placeId, string rating, string[] issues, string comment)
        {
            var data = new Dictionary<string, string>
            {
                { "place_id", placeId },
                { "report_track_id", reportingTrackId },
                { "scene_rating", rating },
                { "issues", string.Join(";", issues) },
                { "comment", comment },
            };
            GenericAnalytics.SendAnalytic(SUBMIT_REPORT_FORM, data);
        }

        public void ClickLearnMoreContentModeration(string placeId)
        {
            var data = new Dictionary<string, string>
            {
                { "place_id", placeId },
                { "report_track_id", reportingTrackId },
            };
            GenericAnalytics.SendAnalytic(OPEN_LEARN_MORE, data);
        }

        public void ErrorSendingReportingForm(string placeId)
        {
            var data = new Dictionary<string, string>
            {
                { "report_track_id", reportingTrackId },
            };
            GenericAnalytics.SendAnalytic(SUBMIT_REPORT_FORM_ERROR, data);
        }

        public void OpenSettingsFromContentWarning() =>
            GenericAnalytics.SendAnalytic(OPEN_SETTINGS_FROM_CONTENT_WARNING);

        public void SwitchAdultContentSetting(bool isEnabled)
        {
            var data = new Dictionary<string, string>
            {
                { "enabled", isEnabled.ToString() },
            };
            GenericAnalytics.SendAnalytic(SWITCH_ADULT_CONTENT_SETTING, data);
        }

        private static string GenerateUniqueID()
        {
            Guid uniqueGuid = Guid.NewGuid();
            return uniqueGuid.ToString("N");
        }
    }
}
