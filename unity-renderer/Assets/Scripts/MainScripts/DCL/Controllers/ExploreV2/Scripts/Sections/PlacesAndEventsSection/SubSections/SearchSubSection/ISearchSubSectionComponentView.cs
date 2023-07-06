using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISearchSubSectionComponentView : IPlacesAndEventsSubSectionComponentView
{
    event Action<int> OnRequestAllEvents;
    void ShowEvents(List<EventCardComponentModel> events, string searchText);
    void ShowAllEvents(List<EventCardComponentModel> events, bool showMoreButton);
}
