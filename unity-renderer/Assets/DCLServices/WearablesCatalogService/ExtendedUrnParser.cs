namespace DCLServices.WearablesCatalogService
{
    public static class ExtendedUrnParser
    {
        private const int QUANTITY_OF_PARTS_ON_SHORTENED_ITEMS_URN = 6;

        public static string GetShortenedUrn(string urnReceived)
        {
            int lastIndex = urnReceived.LastIndexOf(':');

            return lastIndex != -1 && urnReceived.Split(':').Length > QUANTITY_OF_PARTS_ON_SHORTENED_ITEMS_URN
                ? urnReceived.Substring(0, lastIndex)
                : urnReceived;
        }
    }

}
