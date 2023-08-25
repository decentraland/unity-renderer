using DCL;
using DCL.Helpers;
using DCLServices.WorldsAPIService;
using System;
using System.Collections.Generic;
using UnityEngine;
using static MainScripts.DCL.Controllers.HotScenes.IHotScenesController;
using Object = UnityEngine.Object;

public static class PlacesAndEventsCardsFactory
{
    internal const string EVENT_CARD_MODAL_ID = "EventCard_Modal";
    internal const string PLACE_CARD_MODAL_ID = "PlaceCard_Modal";
    internal const string ALL_ID = "all";
    internal const string ALL_TEXT = "All";

    /// <summary>
    /// Creates and configures a pool for cards.
    /// </summary>
    /// <param name="pool">Pool to configure.</param>
    /// <param name="poolName">Name of the pool.</param>
    /// <param name="cardPrefab">Card prefab to use by the pool.</param>
    /// <param name="maxPrewarmCount">Max number of pre-created cards.</param>
    public static Pool GetCardsPoolLazy(string poolName, BaseComponentView cardPrefab, int maxPrewarmCount)
    {
        Pool pool = PoolManager.i.GetPool(poolName);

        if (pool != null)
            return pool;

        pool = PoolManager.i.AddPool(poolName, Object.Instantiate(cardPrefab).gameObject, maxPrewarmCount: maxPrewarmCount, isPersistent: true);
        pool.ForcePrewarm();

        return pool;
    }

    public static EventCardComponentView CreateConfiguredEventCard(Pool pool, EventCardComponentModel eventInfo, Action<EventCardComponentModel> OnEventInfoClicked, Action<EventFromAPIModel> OnEventJumpInClicked, Action<string> OnEventSubscribeEventClicked, Action<string> OnEventUnsubscribeEventClicked) =>
        EventsCardsConfigurator.Configure(pool.Get<EventCardComponentView>(), eventInfo, OnEventInfoClicked, OnEventJumpInClicked, OnEventSubscribeEventClicked, OnEventUnsubscribeEventClicked);

    public static PlaceCardComponentView CreateConfiguredPlaceCard(Pool pool, PlaceCardComponentModel placeInfo, Action<PlaceCardComponentModel> OnPlaceInfoClicked, Action<PlaceInfo> OnPlaceJumpInClicked, Action<string, bool?> OnVoteChanged, Action<string, bool> OnFavoriteClicked) =>
        PlacesCardsConfigurator.Configure(pool.Get<PlaceCardComponentView>(), placeInfo, OnPlaceInfoClicked, OnPlaceJumpInClicked, OnVoteChanged, OnFavoriteClicked);

    /// <summary>
    /// Instantiates (if does not already exists) a place card modal from the given prefab.
    /// </summary>
    /// <param name="placeCardModalPrefab">Prefab to instantiate.</param>
    /// <returns>An instance of a place card modal.</returns>
    public static PlaceCardComponentView GetPlaceCardTemplateHiddenLazy(PlaceCardComponentView placeCardModalPrefab) =>
        GetCardTemplateHiddenLazy(placeCardModalPrefab, PLACE_CARD_MODAL_ID);

    /// <summary>
    /// Instantiates (if does not already exists) a event card modal from the given prefab.
    /// </summary>
    /// <param name="eventCardModalPrefab">Prefab to instantiate.</param>
    /// <returns>An instance of a event card modal.</returns>
    public static EventCardComponentView GetEventCardTemplateHiddenLazy(EventCardComponentView eventCardModalPrefab) =>
        GetCardTemplateHiddenLazy(eventCardModalPrefab, EVENT_CARD_MODAL_ID);

    private static TCardView GetCardTemplateHiddenLazy<TCardView>(TCardView modalPrefab, string cardModalId) where TCardView: BaseComponentView
    {
        TCardView modal;

        GameObject existingModal = GameObject.Find(cardModalId);

        if (existingModal != null)
            modal = existingModal.GetComponent<TCardView>();
        else
        {
            modal = Object.Instantiate(modalPrefab);
            modal.name = cardModalId;
        }

        modal.Hide(true);

        return modal;
    }

    public static List<PlaceCardComponentModel> ConvertPlaceResponseToModel(IEnumerable<PlaceInfo> placeInfo, int amountToTake)
    {
        List<PlaceCardComponentModel> modelsList = new List<PlaceCardComponentModel>();
        int count = 0;
        foreach (var place in placeInfo)
        {
            modelsList.Add(
                new PlaceCardComponentModel()
                {
                    placePictureUri = place.image,
                    placeName = place.title,
                    placeDescription = place.description,
                    placeAuthor = place.contact_name,
                    numberOfUsers = place.user_count,
                    coords = Utils.ConvertStringToVector(place.base_position),
                    parcels = place.Positions,
                    isFavorite = place.user_favorite,
                    userVisits = place.user_visits,
                    userRating = place.like_rate_as_float,
                    placeInfo = place,
                    isUpvote = place.user_like,
                    isDownvote = place.user_dislike,
                    totalVotes = place.likes + place.dislikes,
                    numberOfFavorites = place.favorites,
                    deployedAt = place.deployed_at,
                });
            count++;
            if(count >= amountToTake)
                break;
        }

        return modelsList;
    }

    public static List<PlaceCardComponentModel> ConvertWorldsResponseToModel(IEnumerable<WorldsResponse.WorldInfo> worldInfos, int amountToTake)
    {
        List<PlaceCardComponentModel> modelsList = new List<PlaceCardComponentModel>();
        int count = 0;
        foreach (var world in worldInfos)
        {
            modelsList.Add(
                new PlaceCardComponentModel()
                {
                    placePictureUri = world.image,
                    placeName = world.title,
                    placeDescription = world.description,
                    placeAuthor = world.contact_name,
                    numberOfUsers = world.user_count,
                    coords = Utils.ConvertStringToVector(world.base_position),
                    parcels = world.Positions,
                    isFavorite = world.user_favorite,
                    userVisits = world.user_visits,
                    userRating = world.like_rate_as_float,
                    placeInfo = new PlaceInfo()
                    {
                        base_position = world.base_position,
                        categories = new []{""},
                        contact_name = world.contact_name,
                        description = world.description,
                        world_name = world.world_name,
                        id = world.id,
                        image = world.image,
                        likes = world.likes,
                        dislikes = world.dislikes,
                        title = world.title,
                        user_count = world.user_count,
                        user_favorite = world.user_favorite,
                        user_like = world.user_like,
                        user_dislike = world.user_dislike,
                        user_visits = world.user_visits,
                        favorites = world.favorites,
                        deployed_at = world.deployed_at,
                        Positions = world.Positions,

                    },
                    isUpvote = world.user_like,
                    isDownvote = world.user_dislike,
                    totalVotes = world.likes + world.dislikes,
                    numberOfFavorites = world.favorites,
                    deployedAt = world.deployed_at,
                });
            count++;
            if(count >= amountToTake)
                break;
        }

        return modelsList;
    }

    public static List<PlaceCardComponentModel> ConvertPlaceResponseToModel(
        IList<PlaceInfo> placeInfo,
        Predicate<(int index, PlaceInfo place)> filter)
    {
        List<PlaceCardComponentModel> modelsList = new List<PlaceCardComponentModel>();

        for (var index = 0; index < placeInfo.Count; index++)
        {
            PlaceInfo place = placeInfo[index];
            if(!filter((index, place)))
                continue;
            modelsList.Add(
                new PlaceCardComponentModel()
                {
                    placePictureUri = place.image,
                    placeName = place.title,
                    placeDescription = place.description,
                    placeAuthor = place.contact_name,
                    numberOfUsers = place.user_count,
                    coords = Utils.ConvertStringToVector(place.base_position),
                    parcels = place.Positions,
                    isFavorite = place.user_favorite,
                    userVisits = place.user_visits,
                    userRating = place.like_rate_as_float,
                    placeInfo = place,
                    isUpvote = place.user_like,
                    isDownvote = place.user_dislike,
                    totalVotes = place.likes + place.dislikes,
                    numberOfFavorites = place.favorites,
                    deployedAt = place.deployed_at,
                });
        }

        return modelsList;
    }

    public static List<PlaceCardComponentModel> ConvertWorldsResponseToModel(IEnumerable<WorldsResponse.WorldInfo> worldInfo)
    {
        List<PlaceCardComponentModel> modelsList = new List<PlaceCardComponentModel>();
        foreach (var world in worldInfo)
        {
            modelsList.Add(
                new PlaceCardComponentModel()
                {
                    placePictureUri = world.image,
                    placeName = world.title,
                    placeDescription = world.description,
                    placeAuthor = world.contact_name,
                    numberOfUsers = world.user_count,
                    coords = Utils.ConvertStringToVector(world.base_position),
                    parcels = world.Positions,
                    isFavorite = world.user_favorite,
                    userVisits = world.user_visits,
                    userRating = world.like_rate_as_float,
                    placeInfo = new PlaceInfo()
                    {
                        base_position = world.base_position,
                        categories = new []{""},
                        contact_name = world.contact_name,
                        world_name = world.world_name,
                        description = world.description,
                        id = world.id,
                        image = world.image,
                        likes = world.likes,
                        dislikes = world.dislikes,
                        title = world.title,
                        user_count = world.user_count,
                        user_favorite = world.user_favorite,
                        user_like = world.user_like,
                        user_dislike = world.user_dislike,
                        user_visits = world.user_visits,
                        favorites = world.favorites,
                        deployed_at = world.deployed_at,
                        Positions = world.Positions,
                    },
                    isUpvote = world.user_like,
                    isDownvote = world.user_dislike,
                    totalVotes = world.likes + world.dislikes,
                    numberOfFavorites = world.favorites,
                    deployedAt = world.deployed_at,
                });
        }

        return modelsList;
    }

    public static List<PlaceCardComponentModel> ConvertPlaceResponseToModel(IEnumerable<PlaceInfo> placeInfo)
    {
        List<PlaceCardComponentModel> modelsList = new List<PlaceCardComponentModel>();
        foreach (var place in placeInfo)
        {
            modelsList.Add(
                new PlaceCardComponentModel()
                {
                    placePictureUri = place.image,
                    placeName = place.title,
                    placeDescription = place.description,
                    placeAuthor = place.contact_name,
                    numberOfUsers = place.user_count,
                    coords = Utils.ConvertStringToVector(place.base_position),
                    parcels = place.Positions,
                    isFavorite = place.user_favorite,
                    userVisits = place.user_visits,
                    userRating = place.like_rate_as_float,
                    placeInfo = place,
                    isUpvote = place.user_like,
                    isDownvote = place.user_dislike,
                    totalVotes = place.likes + place.dislikes,
                    numberOfFavorites = place.favorites,
                    deployedAt = place.deployed_at,
                });
        }

        return modelsList;
    }

    public static List<ToggleComponentModel> ConvertCategoriesResponseToToggleModel(List<CategoryFromAPIModel> categories)
    {
        List<ToggleComponentModel> modelsList = new List<ToggleComponentModel>
        {
            new()
            {
                id = ALL_ID,
                text = ALL_TEXT,
                isOn = true,
                isTextActive = true,
                changeTextColorOnSelect = true,
            },
        };

        foreach (var category in categories)
        {
            if (!category.active)
                continue;

            modelsList.Add(
                new ToggleComponentModel
                {
                    id = category.name,
                    text = category.i18n.en,
                    isOn = false,
                    isTextActive = true,
                    changeTextColorOnSelect = true,
                });
        }

        return modelsList;
    }

    /// <summary>
    /// Returns a event card model from the given API data.
    /// </summary>
    /// <param name="filteredEvents">Data received from the API.</param>
    /// <returns>An event card model.</returns>
    public static List<EventCardComponentModel> CreateEventsCards(List<EventFromAPIModel> filteredEvents) =>
        CreateModelsListFromAPI<EventCardComponentModel, EventFromAPIModel>(filteredEvents, EventsCardsConfigurator.ConfigureFromAPIData);

    private static List<TModel> CreateModelsListFromAPI<TModel, TInfo>(List<TInfo> filteredAPIModels, Func<TModel, TInfo, TModel> configureModelByApiData)
        where TModel: BaseComponentModel, new()
    {
        List<TModel> loadedCards = new List<TModel>();

        foreach (TInfo filteredCardInfo in filteredAPIModels)
            loadedCards.Add(configureModelByApiData(new TModel(), filteredCardInfo));

        return loadedCards;
    }
}
