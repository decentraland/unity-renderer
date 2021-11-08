using System.Collections.Generic;

namespace ExploreV2Analytics
{
    public interface IExploreV2Analytics
    {
        void SendExploreVisibility(bool isVisible, ExploreUIVisibilityMethod method);
        void SendExploreElapsedTime(float time);

    }

    public class ExploreV2Analytics : IExploreV2Analytics
    {
        private const string EXPLORE_VIBILILITY_CHANGED = "explore_visibility_changed";
        private const string EXPLORE_ELAPSED_TIME = "explore_elapsed_time";

        public void SendExploreVisibility(bool isVisible, ExploreUIVisibilityMethod method)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("visible", isVisible.ToString());
            data.Add("method", method.ToString());
            GenericAnalytics.SendAnalytic(EXPLORE_VIBILILITY_CHANGED, data);
        }

        public void SendExploreElapsedTime(float time)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("elapsed_time", time.ToString());
            GenericAnalytics.SendAnalytic(EXPLORE_ELAPSED_TIME, data);
        }
    }
}