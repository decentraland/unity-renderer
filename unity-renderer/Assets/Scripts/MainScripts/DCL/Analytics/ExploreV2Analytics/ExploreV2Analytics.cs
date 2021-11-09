using System.Collections.Generic;

namespace ExploreV2Analytics
{
    public interface IExploreV2Analytics
    {
        void SendExploreVisibility(bool isVisible, ExploreUIVisibilityMethod method);
        void SendExploreSectionElapsedTime(ExploreSection section, float time);

    }

    public class ExploreV2Analytics : IExploreV2Analytics
    {
        private const string EXPLORE_VIBILILITY = "explore_visibility";
        private const string EXPLORE_SECTION_ELAPSED_TIME = "explore_section_elapsed_time";

        public void SendExploreVisibility(bool isVisible, ExploreUIVisibilityMethod method)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("visible", isVisible.ToString());
            data.Add("method", method.ToString());
            GenericAnalytics.SendAnalytic(EXPLORE_VIBILILITY, data);
        }

        public void SendExploreSectionElapsedTime(ExploreSection section, float time)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("section", section.ToString());
            data.Add("elapsed_time", time.ToString());
            GenericAnalytics.SendAnalytic(EXPLORE_SECTION_ELAPSED_TIME, data);
        }
    }
}