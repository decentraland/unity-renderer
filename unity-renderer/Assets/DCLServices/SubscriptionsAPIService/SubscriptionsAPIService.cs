using Cysharp.Threading.Tasks;
using DCL;
using System.Threading;

namespace DCLServices.SubscriptionsAPIService
{
    public interface ISubscriptionsAPIService : IService
    {
        UniTask<SubscriptionAPIResponseData> CreateSubscription(string email, CancellationToken ct);
        UniTask<SubscriptionAPIResponseData> GetSubscription(string subscriptionId, CancellationToken ct);
    }

    public class SubscriptionsAPIService : ISubscriptionsAPIService
    {
        private readonly ISubscriptionsAPIClient client;

        public SubscriptionsAPIService(ISubscriptionsAPIClient client) =>
            this.client = client;

        public void Initialize() { }

        public void Dispose() { }

        public async UniTask<SubscriptionAPIResponseData> CreateSubscription(string email, CancellationToken ct) =>
            await client.CreateSubscription(email, ct);

        public async UniTask<SubscriptionAPIResponseData> GetSubscription(string subscriptionId, CancellationToken ct) =>
            await client.GetSubscription(subscriptionId, ct);
    }
}
