using System;

namespace DCLServices.PlacesAPIService
{
    [Serializable]
    public class PlaceContentReportPayload
    {
        public string placeId;
        public bool guest;
        public string coordinates;
        public string rating;
        public string[] issues;
        public string comment;
    }
}
