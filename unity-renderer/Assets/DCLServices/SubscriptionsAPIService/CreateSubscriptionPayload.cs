using System;

namespace DCLServices.SubscriptionsAPIService
{
    [Serializable]
    public class CreateSubscriptionPayload
    {
        public string email;
        public string utm_source;
        public SubscriptionCustomFieldPayload[] custom_fields;
    }

    [Serializable]
    public class SubscriptionCustomFieldPayload
    {
        public string name;
        public string value;
    }
}
