using MainScripts.DCL.Controllers.HotScenes;
using System;
using System.Collections.Generic;

public interface IFavoriteSubSectionComponentView : IPlacesAndEventsSubSectionComponentView
{
    event Action OnRequestFavorites;
    event Action<int> OnRequestAllPlaces;
    event Action<int> OnRequestAllWorlds;
    public event Action<PlaceCardComponentModel, int> OnPlaceInfoClicked;
    public event Action<string, bool?> OnVoteChanged;
    public event Action<IHotScenesController.PlaceInfo> OnPlaceJumpInClicked;
    public event Action<string, bool> OnPlaceFavoriteChanged;

    void ShowPlaces(List<PlaceCardComponentModel> places);
    void ShowAllPlaces(List<PlaceCardComponentModel> places, bool showMoreButton);
    void ShowWorlds(List<PlaceCardComponentModel> worlds);
    void ShowAllWorlds(List<PlaceCardComponentModel> worlds, bool showMoreButton);
    void ShowPlaceModal(PlaceCardComponentModel placeModel);
    void SetHeaderEnabled(bool isFullHeaderActive);
}
