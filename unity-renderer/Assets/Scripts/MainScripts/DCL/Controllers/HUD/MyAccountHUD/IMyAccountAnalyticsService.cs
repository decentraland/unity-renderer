namespace DCL.MyAccount
{
    public interface IMyAccountAnalyticsService
    {
        void SendPlayerSwapNameAnalytic(bool claimed, int totalNames);
        void SendPlayerOpenClaimNameAnalytic();
        void SendPlayerWalletBuyManaAnalytic(bool isPolygonNetwork);
        void SendProfileInfoEditAnalytic(int lenght);
        void SendProfileInfoAdditionalInfoAddAnalytic(string type, string value);
        void SendProfileInfoAdditionalInfoRemoveAnalytic(string type);
        void SendProfileLinkAddAnalytic(string name, string url);
        void SendProfileLinkRemoveAnalytic(string name, string url);
    }
}
