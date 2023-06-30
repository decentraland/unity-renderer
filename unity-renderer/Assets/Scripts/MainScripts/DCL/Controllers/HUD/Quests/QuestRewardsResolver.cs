using Cysharp.Threading.Tasks;
using DCL;
using DCL.Helpers;
using DCLServices.QuestsService;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace DCL.Quests
{
    public class QuestRewardsResolver : IQuestRewardsResolver
    {
        private const string QUESTS_API_URL = "https://quests.decentraland.zone/quests";

        private readonly Service<IWebRequestController> webRequestController;

        public async UniTask<IReadOnlyList<QuestReward>> ResolveRewards(string questId, CancellationToken cancellationToken = default)
        {
            try
            {
                UnityWebRequest result = await webRequestController.Ref.GetAsync($"{QUESTS_API_URL}/{questId}/rewards", cancellationToken: cancellationToken);
                QuestRewardResponse placesAPIResponse = Utils.SafeFromJson<QuestRewardResponse>(result.downloadHandler.text);
                return new List<QuestReward>(placesAPIResponse.items);
            }
            catch (Exception e)
            {
                return new List<QuestReward>() {};
            }
        }
    }
}
