using System;
using System.Collections.Generic;
using UnityEngine;

namespace ExploreV2Analytics
{
    public interface IExploreV2Analytics
    {
        void SendExploreMainMenuVisibility(bool isVisible, ExploreUIVisibilityMethod method);
        void SendExploreSectionVisibility(ExploreSection section, bool isVisible);
        void SendEventTeleport(string eventId, string eventName, Vector2Int coords);
        void SendClickOnEventInfo(string eventId, string eventName);
        void SendPlaceTeleport(string placeId, string placeName, Vector2Int coords);
        void SendClickOnPlaceInfo(string placeId, string placeName);
        bool anyActionExecutedFromLastOpen { get; set; }

    }

    public class ExploreV2Analytics : IExploreV2Analytics
    {
        private const string EXPLORE_MAIN_MENU_VIBILILITY = "explore_main_menu_visibility";
        private const string EXPLORE_SECTION_VISIBILITY = "explore_section_visibility";
        private const string EXPLORE_EVENT_TELEPORT = "explore_event_teleport";
        private const string EXPLORE_CLICK_EVENT_INFO = "explore_click_event_info";
        private const string EXPLORE_PLACE_TELEPORT = "explore_place_teleport";
        private const string EXPLORE_CLICK_PLACE_INFO = "explore_click_place_info";

        private static DateTime? exploreMainMenuSetVisibleTimeStamp = null;
        private static DateTime? exploreSectionSetVisibleTimeStamp = null;
        private static bool anyActionExecutedFromLastOpenValue = false;

        public bool anyActionExecutedFromLastOpen { get => anyActionExecutedFromLastOpenValue; set => anyActionExecutedFromLastOpenValue = value; }

        public void SendExploreMainMenuVisibility(bool isVisible, ExploreUIVisibilityMethod method)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("visible", isVisible.ToString());
            data.Add("method", method.ToString());

            if (isVisible)
                exploreMainMenuSetVisibleTimeStamp = DateTime.Now;
            else
            {
                if (exploreMainMenuSetVisibleTimeStamp.HasValue)
                {
                    data.Add("open_duration_ms", (DateTime.Now - exploreMainMenuSetVisibleTimeStamp.Value).TotalMilliseconds.ToString());
                    exploreMainMenuSetVisibleTimeStamp = null;
                }

                data.Add("any_action_after_close", anyActionExecutedFromLastOpenValue.ToString());
            }

            GenericAnalytics.SendAnalytic(EXPLORE_MAIN_MENU_VIBILILITY, data);
        }

        public void SendExploreSectionVisibility(ExploreSection section, bool isVisible)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("section", section.ToString());
            data.Add("visible", isVisible.ToString());

            if (isVisible)
                exploreSectionSetVisibleTimeStamp = DateTime.Now;
            else
            {
                if (exploreSectionSetVisibleTimeStamp.HasValue)
                {
                    data.Add("open_duration_ms", (DateTime.Now - exploreSectionSetVisibleTimeStamp.Value).TotalMilliseconds.ToString());
                    exploreSectionSetVisibleTimeStamp = null;
                }
            }

            GenericAnalytics.SendAnalytic(EXPLORE_SECTION_VISIBILITY, data);
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