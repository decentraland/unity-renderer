using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ExploreV2EventsSubSectionData", menuName = "ExploreV2/Events sub-section")]
public class EventsSubSectionData : ScriptableObject
{
    public List<EventCardComponentModel> featureEvents;
    public List<EventCardComponentModel> upcomingEvents;
    public List<EventCardComponentModel> goingEvents;
}