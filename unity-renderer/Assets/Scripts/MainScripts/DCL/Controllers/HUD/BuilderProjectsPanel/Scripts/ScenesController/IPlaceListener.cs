using System.Collections.Generic;

internal interface IPlaceListener
{
    void SetPlaces(Dictionary<string, IPlaceCardView> scenes);
    void PlaceAdded(IPlaceCardView place);
    void PlaceRemoved(IPlaceCardView place);
}