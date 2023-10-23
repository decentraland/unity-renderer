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
        UniTask<SubscriptionAPIResponseData> GetSubscription(string subscriptionId, CancellationToken ct);
    }

    public class SubscriptionsAPIClient : ISubscriptionsAPIClient
    {
        private const string PUBLICATION_ID = "pub_9a0ea9f4-8e14-4f2a-a9c7-fc88512427d4";
        private const string TOKEN_ID = "hvG5KRhqXQ1PnggzKUh6ceh6uIUtOVYy3FybRjp5XoDLfVzAl4ar1G7U2qsKQIc4";
        private const string UTM_SOURCE = "explorer";
        private const string CREATE_SUBSCRIPTION_URL = "https://api.beehiiv.com/v2/publications/{publicationId}/subscriptions";
        private const string GET_SUBSCRIPTION_URL =    "https://api.beehiiv.com/v2/publications/{publicationId}/subscriptions/{subscriptionId}";

        private readonly IWebRequestController webRequestController;

        public SubscriptionsAPIClient(IWebRequestController webRequestController)
        {
            this.webRequestController = webRequestController;
        }

        public async UniTask<SubscriptionAPIResponseData> CreateSubscription(string email, CancellationToken ct)
        {
            string postData = JsonUtility.ToJson(new CreateSubscriptionPayload
            {
                email = email,
                utm_source = UTM_SOURCE,
            });

            UnityWebRequest postResult = await webRequestController.PostAsync(
                url: CREATE_SUBSCRIPTION_URL.Replace("{publicationId}", PUBLICATION_ID),
                postData: postData,
                cancellationToken: ct,
                headers: new Dictionary<string, string>
                {
                    { "Accept", "application/json" },
                    { "Authorization", $"Bearer {TOKEN_ID}" },
                    { "Content-Type", "application/json" },
                });

            if (postResult.result != UnityWebRequest.Result.Success)
                throw new Exception($"Error creating subscription:\n{postResult.error}");

            var postResponse = Utils.SafeFromJson<CreateSubscriptionAPIResponse>(postResult.downloadHandler.text);
            if (postResponse?.data == null)
                throw new Exception($"Error creating subscription:\n{postResult.downloadHandler.text}");

            return postResponse.data;
        }

        public async UniTask<SubscriptionAPIResponseData> GetSubscription(string subscriptionId, CancellationToken ct)
        {
            UnityWebRequest result = await webRequestController.GetAsync(
                url: GET_SUBSCRIPTION_URL
                    .Replace("{publicationId}", PUBLICATION_ID)
                    .Replace("{subscriptionId}", subscriptionId),
                cancellationToken: ct,
                headers: new Dictionary<string, string>
                {
                    { "Accept", "application/json" },
                    { "Authorization", $"Bearer {TOKEN_ID}" },
                });

            var response = Utils.SafeFromJson<GetSubscriptionAPIResponse>(result.downloadHandler.text);

            if (response == null)
                throw new Exception($"Error parsing get subscription response:\n{result.downloadHandler.text}");

            return response.data;
        }
    }
}
