using Cysharp.Threading.Tasks;
using DCL;
using System.Threading;

namespace DCLServices.SubscriptionsAPIService
{
    public interface ISubscriptionsAPIService : IService
    {
        UniTask<CreateSubscriptionAPIResponse> CreateSubscription(string email, string walletId, string claimedName, CancellationToken ct);
    }

    public class SubscriptionsAPIService : ISubscriptionsAPIService
    {
        private readonly ISubscriptionsAPIClient client;

        public SubscriptionsAPIService(ISubscriptionsAPIClient client) =>
            this.client = client;

        public void Initialize() { }

        public void Dispose() { }

        public async UniTask<CreateSubscriptionAPIResponse> CreateSubscription(string email, string walletId, string claimedName, CancellationToken ct) =>
            await client.CreateSubscription(email, walletId, claimedName, ct);
    }
}
