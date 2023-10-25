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
        UniTask<SubscriptionResponseData> CreateSubscription(string email, CancellationToken ct);
        UniTask DeleteSubscription(string subscriptionId, CancellationToken ct);
        UniTask<SubscriptionResponseData> GetSubscription(string subscriptionId, CancellationToken ct);
    }

    public class SubscriptionsAPIClient : ISubscriptionsAPIClient
    {
        private const string PUBLICATION_ID = "pub_9a0ea9f4-8e14-4f2a-a9c7-fc88512427d4";
        private const string TOKEN_ID = "";
        private const string UTM_SOURCE = "explorer";
        private const string SUBSCRIPTION_BASE_URL = "https://api.beehiiv.com/v2/publications/{publicationId}/subscriptions";
        private const string CREATE_SUBSCRIPTION_URL = "https://builder-api.decentraland.org/v1/newsletter";

        private readonly IWebRequestController webRequestController;

        public SubscriptionsAPIClient(IWebRequestController webRequestController)
        {
            this.webRequestController = webRequestController;
        }

        public async UniTask<SubscriptionResponseData> CreateSubscription(string email, CancellationToken ct)
        {
            string postData = JsonUtility.ToJson(new CreateSubscriptionPayload
            {
                email = email,
                source = UTM_SOURCE,
            });

            UnityWebRequest postResult = await webRequestController.PostAsync(
                url: CREATE_SUBSCRIPTION_URL,
                postData: postData,
                cancellationToken: ct,
                headers: new Dictionary<string, string> { { "Content-Type", "application/json" } });

            if (postResult.result != UnityWebRequest.Result.Success)
                throw new Exception($"Error creating subscription:\n{postResult.error}");

            var postResponse = Utils.SafeFromJson<CreateSubscriptionResponse>(postResult.downloadHandler.text);
            if (postResponse?.data?.data == null)
                throw new Exception($"Error creating subscription:\n{postResult.downloadHandler.text}");

            return postResponse.data.data;
        }

        public async UniTask DeleteSubscription(string subscriptionId, CancellationToken ct)
        {
            UnityWebRequest deleteResult = await webRequestController.DeleteAsync(
                url: $"{SUBSCRIPTION_BASE_URL.Replace("{publicationId}", PUBLICATION_ID)}/{subscriptionId}",
                cancellationToken: ct,
                headers: new Dictionary<string, string>
                {
                    { "Accept", "application/json" },
                    { "Authorization", $"Bearer {TOKEN_ID}" },
                });

            if (deleteResult.result != UnityWebRequest.Result.Success)
                throw new Exception($"Error deleting subscription:\n{deleteResult.error}");
        }

        public async UniTask<SubscriptionResponseData> GetSubscription(string subscriptionId, CancellationToken ct)
        {
            UnityWebRequest getResult = await webRequestController.GetAsync(
                url: $"{SUBSCRIPTION_BASE_URL.Replace("{publicationId}", PUBLICATION_ID)}/{subscriptionId}",
                cancellationToken: ct,
                headers: new Dictionary<string, string>
                {
                    { "Accept", "application/json" },
                    { "Authorization", $"Bearer {TOKEN_ID}" },
                });

            if (getResult.result != UnityWebRequest.Result.Success)
                throw new Exception($"Error getting subscription:\n{getResult.error}");

            var getResponse = Utils.SafeFromJson<GetSubscriptionAPIResponse>(getResult.downloadHandler.text);
            if (getResponse == null)
                throw new Exception($"Error parsing get subscription response:\n{getResult.downloadHandler.text}");

            return getResponse.data;
        }
    }
}
