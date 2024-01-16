namespace MainScripts.DCL.ServiceProviders.OpenSea.Requests
{
    public class RequestOwnedNFTs : RequestBase<OpenSeaManyNftDto>
    {
        public string address { private set; get; }
        public override string requestId => GetId(address);

        internal RequestOwnedNFTs(string address) { this.address = address; }

        internal static string GetId(string address) { return address; }
    }
}
