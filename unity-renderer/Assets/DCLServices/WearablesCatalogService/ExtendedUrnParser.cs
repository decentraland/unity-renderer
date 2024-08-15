namespace DCLServices.WearablesCatalogService
{
    public static class ExtendedUrnParser
    {
        private const int REGULAR_NFTS_SHORT_PARTS = 6;
        private const int THIRD_PARTY_V2_SHORTEN_URN_PARTS = 7;
        private const string COLLECTIONS_THIRD_PARTY = "collections-thirdparty";

        public static string GetShortenedUrn(string urnReceived)
        {
            if (string.IsNullOrEmpty(urnReceived)) return urnReceived;
            if (CountParts(urnReceived) <= REGULAR_NFTS_SHORT_PARTS) return urnReceived;

            int index;

            if (IsThirdPartyCollection(urnReceived))
            {
                index = -1;

                // Third party v2 contains 10 parts, on which 3 are reserved for the tokenId
                // "id": urn:decentraland:amoy:collections-thirdparty:back-to-the-future:amoy-eb54:tuxedo-6751:amoy:0x1d9fb685c257e74f869ba302e260c0b68f5ebb37:12
                // "tokenId": amoy:0x1d9fb685c257e74f869ba302e260c0b68f5ebb37:12
                for (var i = 0; i < THIRD_PARTY_V2_SHORTEN_URN_PARTS; i++)
                {
                    index = urnReceived.IndexOf(':', index + 1);
                    if (index == -1) break;
                }

                return index != -1 ? urnReceived[..index] : urnReceived;
            }

            // TokenId is always placed in the last part for regular nfts
            index = urnReceived.LastIndexOf(':');

            return index != -1 ? urnReceived[..index] : urnReceived;
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

        private static bool IsThirdPartyCollection(string urn) =>
            !string.IsNullOrEmpty(urn) && urn.Contains(COLLECTIONS_THIRD_PARTY);
    }
}
