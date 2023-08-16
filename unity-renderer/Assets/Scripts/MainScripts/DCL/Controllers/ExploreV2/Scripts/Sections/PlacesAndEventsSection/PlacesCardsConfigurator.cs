using System;
using System.Linq;
using MainScripts.DCL.Controllers.HotScenes;
using static MainScripts.DCL.Controllers.HotScenes.IHotScenesController;
using Environment = DCL.Environment;

/// <summary>
/// Utils related to the places management in ExploreV2.
/// </summary>
public static class PlacesCardsConfigurator
{
    private const string NO_PLACE_DESCRIPTION_WRITTEN = "The author hasn't written a description yet.";

    /// <summary>
    /// Configure a place card with the given model.
    /// </summary>
    /// <param name="placeCard">Place card to configure.</param>
    /// <param name="placeInfo">Model to apply.</param>
    /// <param name="OnPlaceInfoClicked">Action to inform when the Info button has been clicked.</param>
    /// <param name="OnPlaceJumpInClicked">Action to inform when the JumpIn button has been clicked.</param>
    public static PlaceCardComponentView Configure(PlaceCardComponentView placeCard, PlaceCardComponentModel placeInfo, Action<PlaceCardComponentModel> OnPlaceInfoClicked, Action<PlaceInfo> OnPlaceJumpInClicked, Action<string, bool?> OnVoteChanged, Action<string, bool> OnFavoriteChanged)
    {
        placeCard.Configure(placeInfo);

        placeCard.onInfoClick.RemoveAllListeners();
        placeCard.onInfoClick.AddListener(() => OnPlaceInfoClicked?.Invoke(placeInfo));
        placeCard.onBackgroundClick.RemoveAllListeners();
        placeCard.onBackgroundClick.AddListener(() => OnPlaceInfoClicked?.Invoke(placeInfo));
        placeCard.onJumpInClick.RemoveAllListeners();
        placeCard.onJumpInClick.AddListener(() => OnPlaceJumpInClicked?.Invoke(placeInfo.placeInfo));
        placeCard.OnVoteChanged -= OnVoteChanged;
        placeCard.OnVoteChanged += OnVoteChanged;
        placeCard.OnFavoriteChanged -= OnFavoriteChanged;
        placeCard.OnFavoriteChanged += OnFavoriteChanged;

        return placeCard;
    }

    public static PlaceCardComponentModel ConfigureFromAPIData(PlaceCardComponentModel cardModel, HotSceneInfo placeFromAPI)
    {
        cardModel.placePictureUri = placeFromAPI.thumbnail;
        cardModel.placeName = placeFromAPI.name;
        cardModel.placeDescription = FormatDescription(placeFromAPI);
        cardModel.placeAuthor = FormatAuthorName(placeFromAPI);
        cardModel.numberOfUsers = placeFromAPI.usersTotalCount;
        cardModel.parcels = placeFromAPI.parcels;
        cardModel.coords = placeFromAPI.baseCoords;

        return cardModel;
    }

    internal static string FormatDescription(HotSceneInfo placeFromAPI) =>
        string.IsNullOrEmpty(placeFromAPI.description) ? NO_PLACE_DESCRIPTION_WRITTEN : placeFromAPI.description;

    internal static string FormatAuthorName(HotSceneInfo placeFromAPI) =>
        $"Author <b>{placeFromAPI.creator}</b>";
}
