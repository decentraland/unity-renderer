namespace DCL.Helpers.NFT.Markets.OpenSea_Internal
{
    internal static class Constants
    {
        public const string SINGLE_ASSET_URL = "https://opensea.decentraland.org/api/v1/asset";
        public const string MULTIPLE_ASSETS_URL = "https://opensea.decentraland.org/api/v1/assets";
        public const string OWNED_ASSETS_URL = "https://opensea.decentraland.org/api/v1/assets?owner=";
        public const int REQUESTS_RETRY_ATTEMPS = 3;
    }
}