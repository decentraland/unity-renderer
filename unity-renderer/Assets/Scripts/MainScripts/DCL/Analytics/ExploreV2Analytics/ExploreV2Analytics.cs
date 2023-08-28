using System;
using System.Collections.Generic;
using UnityEngine;

namespace ExploreV2Analytics
{
    public interface IExploreV2Analytics
    {
        void SendStartMenuVisibility(bool isVisible, ExploreUIVisibilityMethod method);
        void SendStartMenuSectionVisibility(ExploreSection section, bool isVisible);
        void SendEventTeleport(string eventId, string eventName, Vector2Int coords, ActionSource source = ActionSource.FromExplore);
        void SendClickOnEventInfo(string eventId, string eventName, int resultPosition = -1, ActionSource source = ActionSource.FromExplore);
        void SendPlaceTeleport(string placeId, string placeName, Vector2Int coords, ActionSource source = ActionSource.FromExplore);
        void SendWorldTeleport(string worldId, string worldName, ActionSource source = ActionSource.FromExplore);
        void SendClickOnPlaceInfo(string placeId, string placeName, int resultPosition = -1, ActionSource source = ActionSource.FromExplore);
        void SendClickOnWorldInfo(string worldId, string worldName, int resultPosition = -1, ActionSource source = ActionSource.FromExplore);
        void SendParticipateEvent(string eventId, ActionSource source = ActionSource.FromExplore);
        void SendRemoveParticipateEvent(string eventId, ActionSource source = ActionSource.FromExplore);
        void TeleportToPlaceFromFavorite(string placeUUID, string placeName);
        void SendSearchEvents(string searchString, Vector2Int[] firstResultsCoordinates, string[] firstResultsIds);
        void SendSearchPlaces(string searchString, Vector2Int[] firstResultsCoordinates, string[] firstResultsIds);
        void SendSearchWorlds(string searchString, string[] firstResultsIds);
        void SendPlacesTabOpen();
        void SendWorldsTabOpen();
        void SendEventsTabOpen();
        void SendFavoritesTabOpen();
        void SendFilterEvents(FilterType filterType, string filterValue = "");
    }

    public class ExploreV2Analytics : IExploreV2Analytics
    {
        private const string START_MENU_VISIBILITY = "start_menu_visibility";
        private const string START_MENU_SECTION_VISIBILITY = "start_menu_section_visibility";
        private const string EXPLORE_EVENT_TELEPORT = "explore_event_teleport";
        private const string EXPLORE_CLICK_EVENT_INFO = "explore_click_event_info";
        private const string EXPLORE_SEARCH_EVENTS = "explore_search_events";
        private const string EXPLORE_SEARCH_PLACES = "explore_search_places";
        private const string EXPLORE_SEARCH_WORLDS = "explore_search_worlds";
        private const string EXPLORE_PARTICIPATE_EVENT = "explore_participate_event";
        private const string EXPLORE_REMOVE_PARTICIPATE_EVENT = "explore_remove_participate_event";
        private const string EXPLORE_PLACE_TELEPORT = "explore_place_teleport";
        private const string EXPLORE_WORLD_TELEPORT = "explore_world_teleport";
        private const string EXPLORE_CLICK_PLACE_INFO = "explore_click_place_info";
        private const string EXPLORE_CLICK_WORLD_INFO = "explore_click_world_info";
        private const string TELEPORT_FAVORITE_PLACE = "player_teleport_to_favorite_place";
        private const string EXPLORE_PLACES_TAB_OPEN = "explore_places_tab_open";
        private const string EXPLORE_WORLDS_TAB_OPEN = "explore_worlds_tab_open";
        private const string EXPLORE_EVENTS_TAB_OPEN = "explore_events_tab_open";
        private const string EXPLORE_FAVORITES_TAB_OPEN = "explore_favorites_tab_open";
        private const string FILTER_EVENTS = "player_filter_events";

        private static DateTime? exploreMainMenuSetVisibleTimeStamp = null;
        private static DateTime? exploreSectionSetVisibleTimeStamp = null;
        private static DateTime? eventTimeFilterTimeStamp = null;

        private const int MIN_TIME_TO_SEND_EVENT_TIME_FILTER_METRIC = 5;

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

            GenericAnalytics.SendAnalytic(START_MENU_VISIBILITY, data);
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

        public void SendEventTeleport(string eventId, string eventName, Vector2Int coords, ActionSource source = ActionSource.FromExplore)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("event_id", eventId);
            data.Add("event_name", eventName);
            data.Add("event_coords_x", coords.x.ToString());
            data.Add("event_coords_y", coords.y.ToString());
            data.Add("source", source.ToString());
            GenericAnalytics.SendAnalytic(EXPLORE_EVENT_TELEPORT, data);
        }

        public void SendParticipateEvent(string eventId, ActionSource source = ActionSource.FromExplore)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("event_id", eventId);
            data.Add("source", source.ToString());
            GenericAnalytics.SendAnalytic(EXPLORE_PARTICIPATE_EVENT, data);
        }

        public void SendRemoveParticipateEvent(string eventId, ActionSource source = ActionSource.FromExplore)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("event_id", eventId);
            data.Add("source", source.ToString());
            GenericAnalytics.SendAnalytic(EXPLORE_REMOVE_PARTICIPATE_EVENT, data);
        }

        public void SendClickOnEventInfo(string eventId, string eventName, int resultPosition = -1, ActionSource source = ActionSource.FromExplore)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("event_id", eventId);
            data.Add("event_name", eventName);
            data.Add("source", source.ToString());
            data.Add("result_position", resultPosition.ToString());
            GenericAnalytics.SendAnalytic(EXPLORE_CLICK_EVENT_INFO, data);
        }

        public void SendPlaceTeleport(string placeId, string placeName, Vector2Int coords, ActionSource source = ActionSource.FromExplore)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("place_id", placeId);
            data.Add("place_name", placeName);
            data.Add("place_coords_x", coords.x.ToString());
            data.Add("place_coords_y", coords.y.ToString());
            data.Add("source", source.ToString());
            GenericAnalytics.SendAnalytic(EXPLORE_PLACE_TELEPORT, data);
        }

        public void SendWorldTeleport(string worldId, string worldName, ActionSource source = ActionSource.FromExplore)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("place_id", worldId);
            data.Add("place_name", worldName);
            data.Add("source", source.ToString());
            GenericAnalytics.SendAnalytic(EXPLORE_WORLD_TELEPORT, data);
        }

        public void SendClickOnPlaceInfo(string placeId, string placeName, int resultPosition = -1, ActionSource source = ActionSource.FromExplore)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("place_id", placeId);
            data.Add("place_name", placeName);
            data.Add("source", source.ToString());
            data.Add("result_position", resultPosition.ToString());
            GenericAnalytics.SendAnalytic(EXPLORE_CLICK_PLACE_INFO, data);
        }

        public void SendClickOnWorldInfo(string worldId, string worldName, int resultPosition = -1, ActionSource source = ActionSource.FromExplore)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("world_id", worldId);
            data.Add("world_name", worldName);
            data.Add("source", source.ToString());
            data.Add("result_position", resultPosition.ToString());
            GenericAnalytics.SendAnalytic(EXPLORE_CLICK_WORLD_INFO, data);
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

        public void SendSearchEvents(string searchString, Vector2Int[] firstResultsCoordinates, string[] firstResultsIds)
        {
            var data = new Dictionary<string, string>
            {
                ["search_string"] = searchString,
                ["first_results_coordinates"] = string.Join(",", firstResultsCoordinates),
                ["first_results_ids"] = string.Join(",", firstResultsIds)
            };
            GenericAnalytics.SendAnalytic(EXPLORE_SEARCH_EVENTS, data);
        }

        public void SendSearchPlaces(string searchString, Vector2Int[] firstResultsCoordinates, string[] firstResultsIds)
        {
            var data = new Dictionary<string, string>
            {
                ["search_string"] = searchString,
                ["first_results_coordinates"] = string.Join(",", firstResultsCoordinates),
                ["first_results_ids"] = string.Join(",", firstResultsIds)
            };
            GenericAnalytics.SendAnalytic(EXPLORE_SEARCH_PLACES, data);
        }

        public void SendSearchWorlds(string searchString, string[] firstResultsIds)
        {
            var data = new Dictionary<string, string>
            {
                ["search_string"] = searchString,
                ["first_results_ids"] = string.Join(",", firstResultsIds)
            };
            GenericAnalytics.SendAnalytic(EXPLORE_SEARCH_WORLDS, data);
        }

        public void SendPlacesTabOpen()
        {
            var data = new Dictionary<string, string>();
            GenericAnalytics.SendAnalytic(EXPLORE_PLACES_TAB_OPEN, data);
        }

        public void SendWorldsTabOpen()
        {
            var data = new Dictionary<string, string>();
            GenericAnalytics.SendAnalytic(EXPLORE_WORLDS_TAB_OPEN, data);
        }

        public void SendEventsTabOpen()
        {
            var data = new Dictionary<string, string>();
            GenericAnalytics.SendAnalytic(EXPLORE_EVENTS_TAB_OPEN, data);
        }

        public void SendFavoritesTabOpen()
        {
            var data = new Dictionary<string, string>();
            GenericAnalytics.SendAnalytic(EXPLORE_FAVORITES_TAB_OPEN, data);
        }

        public void SendFilterEvents(FilterType filterType, string filterValue = "")
        {
            if (eventTimeFilterTimeStamp == null)
                eventTimeFilterTimeStamp = DateTime.Now;
            else
            {
                // Due to the possible amount of times that we could send this metric, we are setting a min time of 5 secs
                if ((DateTime.Now - eventTimeFilterTimeStamp.Value).TotalSeconds <= MIN_TIME_TO_SEND_EVENT_TIME_FILTER_METRIC)
                    return;

                eventTimeFilterTimeStamp = null;
            }

            var data = new Dictionary<string, string>
            {
                ["type"] = filterType.ToString(),
                ["value"] = filterValue
            };
            GenericAnalytics.SendAnalytic(FILTER_EVENTS, data);
        }
    }
}
