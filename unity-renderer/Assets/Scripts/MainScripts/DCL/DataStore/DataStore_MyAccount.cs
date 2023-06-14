namespace DCL
{
    public class DataStore_MyAccount
    {
        public readonly BaseVariable<bool> isMyAccountSectionVisible = new (false);
        public readonly BaseVariable<bool> isInitialized = new (false);
        public readonly BaseVariable<bool> myAccountSectionOpenFromProfileHUD = new (false);
    }
}
