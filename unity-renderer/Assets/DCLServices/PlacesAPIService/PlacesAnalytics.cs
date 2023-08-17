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

        void AddFavorite(string placeUUID, ActionSource source);
        void RemoveFavorite(string placeUUID, ActionSource source);
        void Like(string placeUUID, IPlacesAnalytics.ActionSource source);
        void Dislike(string placeUUID, IPlacesAnalytics.ActionSource source);
        void RemoveVote(string placeUUID, IPlacesAnalytics.ActionSource source);
    }

    public class PlacesAnalytics : IPlacesAnalytics
    {
        private const string ADD_FAVORITE_PLACE = "player_add_favorite_place";
        private const string REMOVE_FAVORITE_PLACE = "player_remove_favorite_place";
        private const string LIKE_PLACE = "player_like_place";
        private const string DISLIKE_PLACE = "player_dislike_place";
        private const string REMOVE_VOTE_PLACE = "player_remove_vote_place";

        public void AddFavorite(string placeUUID, IPlacesAnalytics.ActionSource source)
        {
            var data = new Dictionary<string, string>
            {
                ["place_id"] = placeUUID,
                ["source"] = source.ToString()
            };
            GenericAnalytics.SendAnalytic(ADD_FAVORITE_PLACE, data);
        }

        public void RemoveFavorite(string placeUUID, IPlacesAnalytics.ActionSource source)
        {
            var data = new Dictionary<string, string>
            {
                ["place_id"] = placeUUID,
                ["source"] = source.ToString()
            };
            GenericAnalytics.SendAnalytic(REMOVE_FAVORITE_PLACE, data);
        }

        public void Like(string placeUUID, IPlacesAnalytics.ActionSource source)
        {
            var data = new Dictionary<string, string>
            {
                ["place_id"] = placeUUID,
                ["source"] = source.ToString()
            };
            GenericAnalytics.SendAnalytic(LIKE_PLACE, data);
        }

        public void Dislike(string placeUUID, IPlacesAnalytics.ActionSource source)
        {
            var data = new Dictionary<string, string>
            {
                ["place_id"] = placeUUID,
                ["source"] = source.ToString()
            };
            GenericAnalytics.SendAnalytic(DISLIKE_PLACE, data);
        }

        public void RemoveVote(string placeUUID, IPlacesAnalytics.ActionSource source)
        {
            var data = new Dictionary<string, string>
            {
                ["place_id"] = placeUUID,
                ["source"] = source.ToString()
            };
            GenericAnalytics.SendAnalytic(REMOVE_VOTE_PLACE, data);
        }
    }
}
