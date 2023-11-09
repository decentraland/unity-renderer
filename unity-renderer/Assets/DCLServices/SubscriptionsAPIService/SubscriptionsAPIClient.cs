using Cysharp.Threading.Tasks;
using DCL;
using DCL.Helpers;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace DCLServices.SubscriptionsAPIService
{
    public interface ISubscriptionsAPIClient
    {
        UniTask<Subscription> CreateSubscription(string email, CancellationToken ct);
        UniTask DeleteSubscription(string subscriptionId, CancellationToken ct);
        UniTask<Subscription> GetSubscription(string subscriptionId, CancellationToken ct);
    }

    public class SubscriptionsAPIClient : ISubscriptionsAPIClient
    {
        private const string UTM_SOURCE = "explorer";
        private const string BASE_SUBSCRIPTION_URL = "https://builder-api.decentraland.org/v1/newsletter";

        private readonly IWebRequestController webRequestController;

        public SubscriptionsAPIClient(IWebRequestController webRequestController)
        {
            this.webRequestController = webRequestController;
        }

        public async UniTask<Subscription> CreateSubscription(string email, CancellationToken ct)
        {
            string postData = JsonUtility.ToJson(new CreateSubscriptionPayload
            {
                email = email,
                source = UTM_SOURCE,
            });

            UnityWebRequest postResult = await webRequestController.PostAsync(
                url: BASE_SUBSCRIPTION_URL,
                postData: postData,
                cancellationToken: ct,
                headers: new Dictionary<string, string> { { "Content-Type", "application/json" } });

            if (postResult.result != UnityWebRequest.Result.Success)
                throw new Exception($"Error creating subscription:\n{postResult.error}");

            var postResponse = Utils.SafeFromJson<SubscriptionAPIResponse>(postResult.downloadHandler.text);
            if (postResponse?.data?.data == null)
                throw new Exception($"Error creating subscription:\n{postResult.downloadHandler.text}");

            return postResponse.data.data;
        }

        public async UniTask DeleteSubscription(string subscriptionId, CancellationToken ct)
        {
            UnityWebRequest deleteResult = await webRequestController.DeleteAsync(
                url: $"{BASE_SUBSCRIPTION_URL}/{subscriptionId}",
                cancellationToken: ct);

            if (deleteResult.result != UnityWebRequest.Result.Success)
                throw new Exception($"Error deleting subscription:\n{deleteResult.error}");
        }

        public async UniTask<Subscription> GetSubscription(string subscriptionId, CancellationToken ct)
        {
            UnityWebRequest getResult = await webRequestController.GetAsync(
                url: $"{BASE_SUBSCRIPTION_URL}/{subscriptionId}",
                cancellationToken: ct);

            if (getResult.result != UnityWebRequest.Result.Success)
                throw new Exception($"Error getting subscription:\n{getResult.error}");

            var getResponse = Utils.SafeFromJson<SubscriptionAPIResponse>(getResult.downloadHandler.text);
            if (getResponse?.data?.data == null)
                throw new Exception($"Error parsing get subscription response:\n{getResult.downloadHandler.text}");

            return getResponse.data.data;
        }
    }
}
