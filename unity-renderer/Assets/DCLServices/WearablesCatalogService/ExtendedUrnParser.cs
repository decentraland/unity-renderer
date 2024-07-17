namespace DCLServices.WearablesCatalogService
{
    public static class ExtendedUrnParser
    {
        private const int QUANTITY_OF_PARTS_ON_SHORTENED_ITEMS_URN = 6;
        private const string COLLECTIONS_THIRDPARTY = "collections-thirdparty";

        public static string GetShortenedUrn(string urnReceived)
        {
            int lastIndex = urnReceived.LastIndexOf(':');

            return lastIndex != -1 && IsExtendedUrn(urnReceived)
                ? urnReceived.Substring(0, lastIndex)
                : urnReceived;
        }

        public static bool IsExtendedUrn(string urn) =>
            urn.Split(':').Length > QUANTITY_OF_PARTS_ON_SHORTENED_ITEMS_URN && !urn.Contains(COLLECTIONS_THIRDPARTY);
    }

}
