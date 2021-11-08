namespace DCL.Helpers.NFT.Markets.OpenSea_Internal
{
    public class RequestAssetSingle : RequestBase<SingleAssetResponse>
    {
        public string contractAddress { get; }
        public string tokenId { get; }
        public override string requestId => GetId(contractAddress, tokenId);

        internal RequestAssetSingle(string contractAddress, string tokenId)
        {
            this.contractAddress = contractAddress;
            this.tokenId = tokenId;
        }

        internal static string GetId(string contractAddress, string tokenId) { return $"{contractAddress}/{tokenId}"; }
    }
}