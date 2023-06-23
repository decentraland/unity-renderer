using Cysharp.Threading.Tasks;
using DCL;
using DCLServices.QuestsService;
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
            UnityWebRequest result = await webRequestController.Ref.GetAsync($"{QUESTS_API_URL}/{questId}/rewards", cancellationToken: cancellationToken);
            Debug.Log(result.downloadHandler.text);
            return new List<QuestReward>() { new QuestReward("", "")};
        }
    }
}
