using System.Collections.Generic;

namespace DCL.MyAccount
{
    public class MyAccountAnalyticsService : IMyAccountAnalyticsService
    {
        private const string PLAYER_SWAP_NAME = "player_swap_name";
        private const string PLAYER_OPEN_CLAIM_NAME = "player_open_claim_name";
        private const string WALLET_BUY_ETHEREUM_MANA = "player_wallet_buy_mana";
        private const string PROFILE_INFO_EDIT = "profile_info_edit";
        private const string PROFILE_INFO_ADDITIONAL_INFO_ADD = "profile_info_additional_info_add";
        private const string PROFILE_INFO_ADDITIONAL_INFO_REMOVE = "profile_info_additional_info_remove";
        private const string PROFILE_LINK_ADD = "profile_link_add";
        private const string PROFILE_LINK_REMOVE = "profile_link_remove";

        private readonly IAnalytics analytics;

        public MyAccountAnalyticsService(IAnalytics analytics)
        {
            this.analytics = analytics;
        }

        public void SendPlayerSwapNameAnalytic(bool claimed, int totalNames)
        {
            Dictionary<string, string> data = new Dictionary<string, string>
            {
                { "claimed", claimed.ToString() },
                { "total_names", totalNames.ToString() },
            };

            analytics.SendAnalytic(PLAYER_SWAP_NAME, data);
        }

        public void SendPlayerOpenClaimNameAnalytic()
        {
            analytics.SendAnalytic(PLAYER_OPEN_CLAIM_NAME, null);
        }

        public void SendPlayerWalletBuyManaAnalytic(bool isPolygonNetwork)
        {
            Dictionary<string, string> data = new Dictionary<string, string>
            {
                { "is_polygon_network", isPolygonNetwork.ToString() },
            };

            analytics.SendAnalytic(WALLET_BUY_ETHEREUM_MANA, data);
        }

        public void SendProfileInfoEditAnalytic(int lenght)
        {
            Dictionary<string, string> data = new Dictionary<string, string>
            {
                { "lenght", lenght.ToString() },
            };

            analytics.SendAnalytic(PROFILE_INFO_EDIT, data);
        }

        public void SendProfileInfoAdditionalInfoAddAnalytic(string type, string value)
        {
            Dictionary<string, string> data = new Dictionary<string, string>
            {
                { "type", type },
                { "value", value },
            };

            analytics.SendAnalytic(PROFILE_INFO_ADDITIONAL_INFO_ADD, data);
        }

        public void SendProfileInfoAdditionalInfoRemoveAnalytic(string type)
        {
            Dictionary<string, string> data = new Dictionary<string, string>
            {
                { "type", type },
            };

            analytics.SendAnalytic(PROFILE_INFO_ADDITIONAL_INFO_REMOVE, data);
        }

        public void SendProfileLinkAddAnalytic(string name, string url)
        {
            Dictionary<string, string> data = new Dictionary<string, string>
            {
                { "name", name },
                { "url", url },
            };

            analytics.SendAnalytic(PROFILE_LINK_ADD, data);
        }

        public void SendProfileLinkRemoveAnalytic(string name, string url)
        {
            Dictionary<string, string> data = new Dictionary<string, string>
            {
                { "name", name },
                { "url", url },
            };

            analytics.SendAnalytic(PROFILE_LINK_REMOVE, data);
        }
    }
}
