using System;
using System.Collections.Generic;

namespace DCLServices.PlacesAPIService
{
    [Serializable]
    public class PlaceCategoriesAPIResponse
    {
        public bool ok;
        public List<PlaceCategoryInfo> data;
    }

    [Serializable]
    public class PlaceCategoryInfo
    {
        public string name;
        public PlaceCategoryLocalization i18n;
    }

    [Serializable]
    public class PlaceCategoryLocalization
    {
        public string en;
    }
}
