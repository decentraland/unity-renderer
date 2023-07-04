using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISearchSubSectionComponentView : IPlacesAndEventsSubSectionComponentView
{
    event Action OnRequestAllEvents;
    void ShowEvents(List<EventCardComponentModel> events);
    void ShowAllEvents(List<EventCardComponentModel> events);
}
