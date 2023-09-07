using System.Collections.Generic;

namespace DCLServices.PlacesAPIService
{
    public interface IPlacesAnalytics
    {
        public enum ActionSource
        {
            FromExplore,
            FromSearch,
            FromMinimap,
            FromNavmap
        }

        public enum FilterType
        {
            PointOfInterest,
            Featured
        }

        public enum SortingType
        {
            MostActive,
            Best
        }

        void AddFavorite(string placeUUID, ActionSource source, bool isWorld = false);
        void RemoveFavorite(string placeUUID, ActionSource source, bool isWorld = false);
        void Like(string placeUUID, IPlacesAnalytics.ActionSource source, bool isWorld = false);
        void Dislike(string placeUUID, IPlacesAnalytics.ActionSource source, bool isWorld = false);
        void RemoveVote(string placeUUID, IPlacesAnalytics.ActionSource source, bool isWorld = false);
        void Filter(FilterType filterType);
        void Sort(IPlacesAnalytics.SortingType sortingType);
        void SortWorlds(IPlacesAnalytics.SortingType sortingType);
    }

    public class PlacesAnalytics : IPlacesAnalytics
    {
        private const string ADD_FAVORITE_PLACE = "player_add_favorite_place";
        private const string REMOVE_FAVORITE_PLACE = "player_remove_favorite_place";
        private const string LIKE_PLACE = "player_like_place";
        private const string DISLIKE_PLACE = "player_dislike_place";
        private const string REMOVE_VOTE_PLACE = "player_remove_vote_place";
        private const string FILTER_PLACES = "player_filter_places";
        private const string SORT_PLACES = "player_sort_places";
        private const string SORT_WORLDS = "player_sort_worlds";

        public void AddFavorite(string placeUUID, IPlacesAnalytics.ActionSource source, bool isWorld = false)
        {
            var data = new Dictionary<string, string>
            {
                ["place_id"] = placeUUID,
                ["source"] = source.ToString(),
                ["is_world"] = isWorld.ToString()
            };
            GenericAnalytics.SendAnalytic(ADD_FAVORITE_PLACE, data);
        }

        public void RemoveFavorite(string placeUUID, IPlacesAnalytics.ActionSource source, bool isWorld = false)
        {
            var data = new Dictionary<string, string>
            {
                ["place_id"] = placeUUID,
                ["source"] = source.ToString(),
                ["is_world"] = isWorld.ToString()
            };
            GenericAnalytics.SendAnalytic(REMOVE_FAVORITE_PLACE, data);
        }

        public void Like(string placeUUID, IPlacesAnalytics.ActionSource source, bool isWorld = false)
        {
            var data = new Dictionary<string, string>
            {
                ["place_id"] = placeUUID,
                ["source"] = source.ToString(),
                ["is_world"] = isWorld.ToString()
            };
            GenericAnalytics.SendAnalytic(LIKE_PLACE, data);
        }

        public void Dislike(string placeUUID, IPlacesAnalytics.ActionSource source, bool isWorld = false)
        {
            var data = new Dictionary<string, string>
            {
                ["place_id"] = placeUUID,
                ["source"] = source.ToString(),
                ["is_world"] = isWorld.ToString()
            };
            GenericAnalytics.SendAnalytic(DISLIKE_PLACE, data);
        }

        public void RemoveVote(string placeUUID, IPlacesAnalytics.ActionSource source, bool isWorld = false)
        {
            var data = new Dictionary<string, string>
            {
                ["place_id"] = placeUUID,
                ["source"] = source.ToString(),
                ["is_world"] = isWorld.ToString()
            };
            GenericAnalytics.SendAnalytic(REMOVE_VOTE_PLACE, data);
        }

        public void Filter(IPlacesAnalytics.FilterType filterType)
        {
            var data = new Dictionary<string, string>
            {
                ["type"] = filterType.ToString()
            };
            GenericAnalytics.SendAnalytic(FILTER_PLACES, data);
        }

        public void Sort(IPlacesAnalytics.SortingType sortingType)
        {
            var data = new Dictionary<string, string>
            {
                ["type"] = sortingType.ToString()
            };
            GenericAnalytics.SendAnalytic(SORT_PLACES, data);
        }

        public void SortWorlds(IPlacesAnalytics.SortingType sortingType)
        {
            var data = new Dictionary<string, string>
            {
                ["type"] = sortingType.ToString()
            };
            GenericAnalytics.SendAnalytic(SORT_WORLDS, data);
        }
    }
}
