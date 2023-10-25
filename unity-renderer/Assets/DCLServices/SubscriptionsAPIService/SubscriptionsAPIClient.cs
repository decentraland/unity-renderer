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
        UniTask<SubscriptionAPIResponseData> CreateSubscription(string email, CancellationToken ct);
        UniTask DeleteSubscription(string subscriptionId, CancellationToken ct);
        UniTask<SubscriptionAPIResponseData> GetSubscription(string subscriptionId, CancellationToken ct);
    }

    public class SubscriptionsAPIClient : ISubscriptionsAPIClient
    {
        private const string PUBLICATION_ID = "pub_9a0ea9f4-8e14-4f2a-a9c7-fc88512427d4";
        private const string UTM_SOURCE = "explorer";
        private const string SUBSCRIPTION_BASE_URL = "https://api.beehiiv.com/v2/publications/{publicationId}/subscriptions";

        private readonly IWebRequestController webRequestController;

        private static string bearerToken => CommonScriptableObjects.subscriptionsBearerToken.Get();

        public SubscriptionsAPIClient(IWebRequestController webRequestController)
        {
            this.webRequestController = webRequestController;
        }

        public async UniTask<SubscriptionAPIResponseData> CreateSubscription(string email, CancellationToken ct)
        {
            Debug.Log($"[SANTI LOG - CreateSubscription] bearerToken: {bearerToken}");

            string postData = JsonUtility.ToJson(new CreateSubscriptionPayload
            {
                email = email,
                utm_source = UTM_SOURCE,
            });

            UnityWebRequest postResult = await webRequestController.PostAsync(
                url: SUBSCRIPTION_BASE_URL.Replace("{publicationId}", PUBLICATION_ID),
                postData: postData,
                cancellationToken: ct,
                headers: new Dictionary<string, string>
                {
                    { "Accept", "application/json" },
                    { "Authorization", $"Bearer {bearerToken}" },
                    { "Content-Type", "application/json" },
                });

            if (postResult.result != UnityWebRequest.Result.Success)
                throw new Exception($"Error creating subscription:\n{postResult.error}");

            var postResponse = Utils.SafeFromJson<CreateSubscriptionAPIResponse>(postResult.downloadHandler.text);
            if (postResponse?.data == null)
                throw new Exception($"Error creating subscription:\n{postResult.downloadHandler.text}");

            return postResponse.data;
        }

        public async UniTask DeleteSubscription(string subscriptionId, CancellationToken ct)
        {
            Debug.Log($"[SANTI LOG - DeleteSubscription] bearerToken: {bearerToken}");

            UnityWebRequest deleteResult = await webRequestController.DeleteAsync(
                url: $"{SUBSCRIPTION_BASE_URL.Replace("{publicationId}", PUBLICATION_ID)}/{subscriptionId}",
                cancellationToken: ct,
                headers: new Dictionary<string, string>
                {
                    { "Accept", "application/json" },
                    { "Authorization", $"Bearer {bearerToken}" },
                });

            if (deleteResult.result != UnityWebRequest.Result.Success)
                throw new Exception($"Error deleting subscription:\n{deleteResult.error}");
        }

        public async UniTask<SubscriptionAPIResponseData> GetSubscription(string subscriptionId, CancellationToken ct)
        {
            Debug.Log($"[SANTI LOG - GetSubscription] bearerToken: {bearerToken}");

            UnityWebRequest getResult = await webRequestController.GetAsync(
                url: $"{SUBSCRIPTION_BASE_URL.Replace("{publicationId}", PUBLICATION_ID)}/{subscriptionId}",
                cancellationToken: ct,
                headers: new Dictionary<string, string>
                {
                    { "Accept", "application/json" },
                    { "Authorization", $"Bearer {bearerToken}" },
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
