using System.Collections.Generic;
using UnityEngine;

namespace ExploreV2Analytics
{
    public interface IExploreV2Analytics
    {
        void SendExploreVisibility(bool isVisible, ExploreUIVisibilityMethod method);
        void SendExploreExitWithoutActions(float elapsedTime);
        void SendExploreSectionElapsedTime(ExploreSection section, float elapsedTime);
        void SendEventTeleport(string eventId, string eventName, Vector2Int coords);
        void SendClickOnEventInfo(string eventId, string eventName);
        void SendPlaceTeleport(string placeId, string placeName, Vector2Int coords);
        void SendClickOnPlaceInfo(string placeId, string placeName);

    }

    public class ExploreV2Analytics : IExploreV2Analytics
    {
        private const string EXPLORE_VIBILILITY = "explore_visibility";
        private const string EXPLORE_EXIT_WITHOUT_ACTIONS = "explore_exit_without_actions";
        private const string EXPLORE_SECTION_ELAPSED_TIME = "explore_section_elapsed_time";
        private const string EXPLORE_EVENT_TELEPORT = "explore_event_teleport";
        private const string EXPLORE_CLICK_EVENT_INFO = "explore_click_event_info";
        private const string EXPLORE_PLACE_TELEPORT = "explore_place_teleport";
        private const string EXPLORE_CLICK_PLACE_INFO = "explore_click_place_info";

        public void SendExploreVisibility(bool isVisible, ExploreUIVisibilityMethod method)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("visible", isVisible.ToString());
            data.Add("method", method.ToString());
            GenericAnalytics.SendAnalytic(EXPLORE_VIBILILITY, data);
        }

        public void SendExploreExitWithoutActions(float elapsedTime)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            GenericAnalytics.SendAnalytic(EXPLORE_EXIT_WITHOUT_ACTIONS, data);
        }

        public void SendExploreSectionElapsedTime(ExploreSection section, float elapsedTime)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("section", section.ToString());
            data.Add("elapsed_time", elapsedTime.ToString());
            GenericAnalytics.SendAnalytic(EXPLORE_SECTION_ELAPSED_TIME, data);
        }

        public void SendEventTeleport(string eventId, string eventName, Vector2Int coords)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("event_id", eventId);
            data.Add("event_name", eventName);
            data.Add("event_coords_x", coords.x.ToString());
            data.Add("event_coords_y", coords.y.ToString());
            GenericAnalytics.SendAnalytic(EXPLORE_EVENT_TELEPORT, data);
        }

        public void SendClickOnEventInfo(string eventId, string eventName)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("event_id", eventId);
            data.Add("event_name", eventName);
            GenericAnalytics.SendAnalytic(EXPLORE_CLICK_EVENT_INFO, data);
        }

        public void SendPlaceTeleport(string placeId, string placeName, Vector2Int coords)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("place_id", placeId);
            data.Add("place_name", placeName);
            data.Add("place_coords_x", coords.x.ToString());
            data.Add("place_coords_y", coords.y.ToString());
            GenericAnalytics.SendAnalytic(EXPLORE_PLACE_TELEPORT, data);
        }

        public void SendClickOnPlaceInfo(string placeId, string placeName)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("event_id", placeId);
            data.Add("event_name", placeName);
            GenericAnalytics.SendAnalytic(EXPLORE_CLICK_PLACE_INFO, data);
        }
    }
}