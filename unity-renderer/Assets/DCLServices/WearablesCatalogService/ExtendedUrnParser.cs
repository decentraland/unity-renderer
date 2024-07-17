namespace DCLServices.WearablesCatalogService
{
    public static class ExtendedUrnParser
    {
        private const int REGULAR_NFTS_SHORT_PARTS = 6;
        private const int LINKED_NFTS_SHORT_PARTS = 8;
        private const string COLLECTIONS_THIRD_PARTY = "collections-thirdparty";
        private const string COLLECTIONS_LINKED_WEARABLES = "collections-linked-wearables";

        public static string GetShortenedUrn(string urnReceived)
        {
            if (!IsExtendedUrn(urnReceived)) return urnReceived;

            int lastIndex = urnReceived.LastIndexOf(':');

            return lastIndex != -1
                ? urnReceived[..lastIndex]
                : urnReceived;
        }

        public static bool IsExtendedUrn(string urn)
        {
            if (urn.Contains(COLLECTIONS_THIRD_PARTY))
                return false;

            if (urn.Contains(COLLECTIONS_LINKED_WEARABLES))
                return CountParts(urn) > LINKED_NFTS_SHORT_PARTS;

            return CountParts(urn) > REGULAR_NFTS_SHORT_PARTS;
        }

        private static int CountParts(string urn)
        {
            int count = 1;
            int index = urn.IndexOf(':');

            while (index != -1)
            {
                count++;
                index = urn.IndexOf(':', index + 1);
            }

            return count;
        }
    }
}
