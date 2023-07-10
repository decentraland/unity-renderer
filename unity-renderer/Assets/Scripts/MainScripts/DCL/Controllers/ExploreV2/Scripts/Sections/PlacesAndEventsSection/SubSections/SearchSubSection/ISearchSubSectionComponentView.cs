using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISearchSubSectionComponentView : IPlacesAndEventsSubSectionComponentView
{
    event Action<int> OnRequestAllEvents;
    event Action OnBackFromSearch;
    public event Action<EventCardComponentModel> OnInfoClicked;
    public event Action<EventFromAPIModel> OnJumpInClicked;
    public event Action<string> OnSubscribeEventClicked;
    public event Action<string> OnUnsubscribeEventClicked;

    void ShowEvents(List<EventCardComponentModel> events, string searchText);
    void ShowAllEvents(List<EventCardComponentModel> events, bool showMoreButton);
    void ShowEventModal(EventCardComponentModel eventModel);
    void SetHeaderEnabled(bool isEnabled, string searchText);
}
