using Cysharp.Threading.Tasks;
using DCL;
using System.Threading;

namespace DCLServices.SubscriptionsAPIService
{
    public interface ISubscriptionsAPIService : IService
    {
        UniTask<Subscription> CreateSubscription(string email, CancellationToken ct);
        UniTask DeleteSubscription(string subscriptionId, CancellationToken ct);
        UniTask<Subscription> GetSubscription(string subscriptionId, CancellationToken ct);
    }

    public class SubscriptionsAPIService : ISubscriptionsAPIService
    {
        private readonly ISubscriptionsAPIClient client;

        public SubscriptionsAPIService(ISubscriptionsAPIClient client) =>
            this.client = client;

        public void Initialize() { }

        public void Dispose() { }

        public async UniTask<Subscription> CreateSubscription(string email, CancellationToken ct) =>
            await client.CreateSubscription(email, ct);

        public async UniTask DeleteSubscription(string subscriptionId, CancellationToken ct) =>
            await client.DeleteSubscription(subscriptionId, ct);

        public async UniTask<Subscription> GetSubscription(string subscriptionId, CancellationToken ct) =>
            await client.GetSubscription(subscriptionId, ct);
    }
}
