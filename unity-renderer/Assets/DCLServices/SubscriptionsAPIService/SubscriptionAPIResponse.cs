using System;

namespace DCLServices.SubscriptionsAPIService
{
    [Serializable]
    public class SubscriptionAPIResponse
    {
        public bool ok;
        public SubscriptionData data;
    }

    [Serializable]
    public class SubscriptionData
    {
        public Subscription data;
    }

    [Serializable]
    public class Subscription
    {
        public string id;
        public string email;
        public string status;
        public string created;
        public string subscription_tier;
        public string utm_source;
        public string utm_medium;
        public string utm_channel;
        public string utm_campaign;
        public string referring_site;
        public string referral_code;
    }
}
