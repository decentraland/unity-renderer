using System;

namespace DCLServices.SubscriptionsAPIService
{
    [Serializable]
    public class CreateSubscriptionPayload
    {
        public string email;
        public string source;
    }
}
