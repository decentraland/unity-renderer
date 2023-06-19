namespace DCL
{
    public class DataStore_Wallet
    {
        public readonly BaseVariable<bool> isWalletSectionVisible = new (false);
        public readonly BaseVariable<bool> isWalletCardVisible = new (false);
        public readonly BaseVariable<bool> isInitialized = new (false);
        public readonly BaseVariable<double> currentEthereumManaBalance = new (0f);
        public readonly BaseVariable<double> currentPolygonManaBalance = new (0f);
    }
}
