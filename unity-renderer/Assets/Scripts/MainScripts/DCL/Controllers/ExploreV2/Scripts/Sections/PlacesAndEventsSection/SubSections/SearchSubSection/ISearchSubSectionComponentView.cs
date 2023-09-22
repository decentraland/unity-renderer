using MainScripts.DCL.Controllers.HotScenes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISearchSubSectionComponentView : IPlacesAndEventsSubSectionComponentView
{
    event Action<int> OnRequestAllEvents;
    event Action<int> OnRequestAllPlaces;
    event Action<int> OnRequestAllWorlds;
    event Action OnBackFromSearch;
    public event Action<EventCardComponentModel, int> OnEventInfoClicked;
    public event Action<PlaceCardComponentModel, int> OnPlaceInfoClicked;
    public event Action<PlaceCardComponentModel, int> OnWorldInfoClicked;
    public event Action<string, bool?> OnVoteChanged;
    public event Action<EventFromAPIModel> OnEventJumpInClicked;
    public event Action<IHotScenesController.PlaceInfo> OnPlaceJumpInClicked;
    public event Action<IHotScenesController.PlaceInfo> OnWorldJumpInClicked;
    public event Action<string, bool> OnPlaceFavoriteChanged;
    public event Action<string, bool> OnSubscribeEventClicked;
    public event Action<string, bool> OnUnsubscribeEventClicked;

    void ShowEvents(List<EventCardComponentModel> events, string searchText);
    void ShowAllEvents(List<EventCardComponentModel> events, bool showMoreButton);
    void ShowPlaces(List<PlaceCardComponentModel> places, string searchText);
    void ShowAllPlaces(List<PlaceCardComponentModel> places, bool showMoreButton);
    void ShowWorlds(List<PlaceCardComponentModel> worlds, string searchText);
    void ShowAllWorlds(List<PlaceCardComponentModel> worlds, bool showMoreButton);
    void ShowEventModal(EventCardComponentModel eventModel);
    void ShowPlaceModal(PlaceCardComponentModel placeModel);
    void ShowWorldModal(PlaceCardComponentModel placeModel);
    void HideWorldModal();
    void SetHeaderEnabled(string searchText);
}
