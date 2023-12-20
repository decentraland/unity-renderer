namespace MainScripts.DCL.ServiceProviders.OpenSea
{
    internal static class OpenSeaAPI
    {
        public const int REQUESTS_RETRY_ATTEMPS = 3;

        private const string BASE_URL = "https://opensea.decentraland.org";
        private const string CHAIN = "ethereum";

        public static string GetSingleAssetUrl(string address, string id) =>
            $"{BASE_URL}/api/v2/chain/{CHAIN}/contract/{address}/nfts/{id}";

        public static string GetOwnedAssetsUrl(string address) =>
            $"{BASE_URL}/api/v2/chain/{CHAIN}/account/{address}/nfts";
    }
}
