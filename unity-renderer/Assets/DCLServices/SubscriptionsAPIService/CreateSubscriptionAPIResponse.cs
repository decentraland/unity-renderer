using System;

namespace DCLServices.SubscriptionsAPIService
{
    [Serializable]
    public class CreateSubscriptionAPIResponse
    {
        public CreateSubscriptionAPIResponseData data;

    }

    [Serializable]
    public class CreateSubscriptionAPIResponseData
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
