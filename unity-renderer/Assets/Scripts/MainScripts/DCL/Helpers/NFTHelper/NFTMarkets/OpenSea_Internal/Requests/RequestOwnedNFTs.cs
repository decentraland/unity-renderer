namespace DCL.Helpers.NFT.Markets.OpenSea_Internal
{
    public class RequestOwnedNFTs : RequestBase<AssetsResponse>
    {
        public string address { private set; get; }
        public override string requestId => GetId(address);

        internal RequestOwnedNFTs(string address) { this.address = address; }

        internal static string GetId(string address) { return address; }
    }
}