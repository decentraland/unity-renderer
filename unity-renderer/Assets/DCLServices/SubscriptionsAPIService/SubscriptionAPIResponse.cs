using System;

namespace DCLServices.SubscriptionsAPIService
{
    [Serializable]
    public class CreateSubscriptionResponse
    {
        public CreateSubscriptionResponseData data;
    }

    [Serializable]
    public class CreateSubscriptionResponseData
    {
        public SubscriptionResponseData data;
    }

    [Serializable]
    public class GetSubscriptionAPIResponse
    {
        public SubscriptionResponseData data;
    }

    [Serializable]
    public class SubscriptionResponseData
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
