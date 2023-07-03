using System;
using UnityEngine;

namespace DCLServices.PlacesAPIService
{
    public class NotAPlaceException : Exception
    {
        public NotAPlaceException(string placeUUID) : base($"Couldnt find place with ID {placeUUID}") { }
        public NotAPlaceException(Vector2Int coords) : base($"Scene at {coords} is not a place") { }
    }
}
