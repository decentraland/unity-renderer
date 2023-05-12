using System;
using System.Collections.Generic;
using UnityEngine;

namespace ExploreV2Analytics
{
    public interface IExploreV2Analytics
    {
        void SendStartMenuVisibility(bool isVisible, ExploreUIVisibilityMethod method);
        void SendStartMenuSectionVisibility(ExploreSection section, bool isVisible);
        void SendEventTeleport(string eventId, string eventName, Vector2Int coords);
        void SendClickOnEventInfo(string eventId, string eventName);
        void SendPlaceTeleport(string placeId, string placeName, Vector2Int coords);
        void SendClickOnPlaceInfo(string placeId, string placeName);
        void AddFavorite(string placeUUID);
        void RemoveFavorite(string placeUUID);
        void TeleportToPlaceFromFavorite(string placeUUID, string placeName);

    }

    public class ExploreV2Analytics : IExploreV2Analytics
    {
        private const string START_MENU_VIBILILITY = "start_menu_visibility";
        private const string START_MENU_SECTION_VISIBILITY = "start_menu_section_visibility";
        private const string EXPLORE_EVENT_TELEPORT = "explore_event_teleport";
        private const string EXPLORE_CLICK_EVENT_INFO = "explore_click_event_info";
        private const string EXPLORE_PLACE_TELEPORT = "explore_place_teleport";
        private const string EXPLORE_CLICK_PLACE_INFO = "explore_click_place_info";
        private const string ADD_FAVORITE_PLACE = "player_add_favorite_place";
        private const string REMOVE_FAVORITE_PLACE = "player_remove_favorite_place";
        private const string TELEPORT_FAVORITE_PLACE = "player_teleport_to_favorite_place";

        private static DateTime? exploreMainMenuSetVisibleTimeStamp = null;
        private static DateTime? exploreSectionSetVisibleTimeStamp = null;

        public void SendStartMenuVisibility(bool isVisible, ExploreUIVisibilityMethod method)
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
            }

            GenericAnalytics.SendAnalytic(START_MENU_VIBILILITY, data);
        }

        public void SendStartMenuSectionVisibility(ExploreSection section, bool isVisible)
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

            GenericAnalytics.SendAnalytic(START_MENU_SECTION_VISIBILITY, data);
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
            data.Add("place_id", placeId);
            data.Add("place_name", placeName);
            GenericAnalytics.SendAnalytic(EXPLORE_CLICK_PLACE_INFO, data);
        }

        public void AddFavorite(string placeUUID)
        {
            var data = new Dictionary<string, string>
            {
                ["place_id"] = placeUUID
            };
            GenericAnalytics.SendAnalytic(ADD_FAVORITE_PLACE, data);
        }

        public void RemoveFavorite(string placeUUID)
        {
            var data = new Dictionary<string, string>
            {
                ["place_id"] = placeUUID
            };
            GenericAnalytics.SendAnalytic(REMOVE_FAVORITE_PLACE, data);
        }

        public void TeleportToPlaceFromFavorite(string placeUUID, string placeName)
        {
            var data = new Dictionary<string, string>
            {
                ["place_id"] = placeUUID,
                ["place_name"] = placeName
            };
            GenericAnalytics.SendAnalytic(TELEPORT_FAVORITE_PLACE, data);
        }

    }
}
