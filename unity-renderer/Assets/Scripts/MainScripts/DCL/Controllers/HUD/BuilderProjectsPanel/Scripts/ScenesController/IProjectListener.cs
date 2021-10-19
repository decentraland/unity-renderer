using System.Collections.Generic;

internal interface IProjectListener
{
    void SetPlaces(Dictionary<string, IPlaceCardView> scenes);
    void PlaceAdded(IPlaceCardView place);
    void PlaceRemoved(IPlaceCardView place);
}