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
        UniTask<CreateSubscriptionAPIResponse> CreateSubscription(string email, string walletId, string claimedName, CancellationToken ct);
    }

    public class SubscriptionsAPIClient : ISubscriptionsAPIClient
    {
        private const string PUBLICATION_ID = "";
        private const string TOKEN_ID = "";
        private const string UTM_SOURCE = "explorer";
        private const string CREATE_SUBSCRIPTION_URL = "https://api.beehiiv.com/v2/publications/{publicationId}/subscriptions";

        private readonly IWebRequestController webRequestController;

        public SubscriptionsAPIClient(IWebRequestController webRequestController)
        {
            this.webRequestController = webRequestController;
        }

        public async UniTask<CreateSubscriptionAPIResponse> CreateSubscription(string email, string walletId, string claimedName, CancellationToken ct)
        {
            SubscriptionCustomFieldPayload[] customFields = new SubscriptionCustomFieldPayload[string.IsNullOrEmpty(claimedName) ? 1 : 2];
            customFields[0] = new SubscriptionCustomFieldPayload { name = "Wallet Address", value = walletId };
            if (!string.IsNullOrEmpty(claimedName))
                customFields[1] = new SubscriptionCustomFieldPayload { name = "Claimed NAME", value = claimedName };

            string postData = JsonUtility.ToJson(new CreateSubscriptionPayload
            {
                email = email,
                utm_source = UTM_SOURCE,
                custom_fields = customFields,
            });

            UnityWebRequest postResult = await webRequestController.PostAsync(
                url: CREATE_SUBSCRIPTION_URL.Replace("{publicationId}", PUBLICATION_ID),
                postData: postData,
                isSigned: false,
                cancellationToken: ct,
                headers: new Dictionary<string, string>
                {
                    { "Accept", "application/json" },
                    { "Authorization", $"Bearer {TOKEN_ID}" },
                    { "Content-Type", "application/json" },
                    { "Prefer", "code=200" },
                });

            if (postResult.result != UnityWebRequest.Result.Success)
                throw new Exception($"Error creating subscription:\n{postResult.error}");

            var postResponse = Utils.SafeFromJson<CreateSubscriptionAPIResponse>(postResult.downloadHandler.text);
            if (postResponse?.data == null)
                throw new Exception($"Error creating subscription:\n{postResult.downloadHandler.text}");

            return postResponse;
        }
    }
}
