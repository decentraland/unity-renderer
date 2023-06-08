namespace DCL.Wallet
{
    public record WalletSectionHUDModel
    {
        public bool IsGuest;
        public string WalletAddress;
        public double EthereumManaBalance;
        public double PolygonManaBalance;
    }
}
