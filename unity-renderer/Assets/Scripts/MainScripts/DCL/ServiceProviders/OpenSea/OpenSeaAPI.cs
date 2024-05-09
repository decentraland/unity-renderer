namespace MainScripts.DCL.ServiceProviders.OpenSea
{
    internal static class OpenSeaAPI
    {
        public const int REQUESTS_RETRY_ATTEMPS = 3;

        private const string BASE_URL = "https://opensea.decentraland";
        private const string DEFAULT_CHAIN = "ethereum";

        public static string GetSingleAssetUrl(string chain, string address, string id, string tld) =>
            $"{BASE_URL}{tld}/api/v2/chain/{chain}/contract/{address}/nfts/{id}";

        public static string GetOwnedAssetsUrl(string address, string tld) =>
            $"{BASE_URL}{tld}/api/v2/chain/{DEFAULT_CHAIN}/account/{address}/nfts";
    }
}
