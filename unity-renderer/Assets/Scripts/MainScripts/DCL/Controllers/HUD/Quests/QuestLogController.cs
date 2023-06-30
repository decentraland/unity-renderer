using Cysharp.Threading.Tasks;
using DCL.Tasks;
using DCLServices.QuestsService;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DCL.Quests
{
    public class QuestLogController : IDisposable
    {
        private readonly IUserProfileBridge userProfileBridge;
        private readonly IQuestLogComponentView questLogComponentView;
        private readonly IQuestsService questsService;

        public event Action<string, bool> OnPinChange;
        public event Action<Vector2Int> OnJumpIn;
        public event Action<string> OnQuestAbandon;
        private CancellationTokenSource disposeCts;

        public QuestLogController(
            IQuestLogComponentView questLogComponentView,
            IUserProfileBridge userProfileBridge,
            IQuestsService questsService)
        {
            disposeCts = new CancellationTokenSource();
            this.questLogComponentView = questLogComponentView;
            this.userProfileBridge = userProfileBridge;
            this.questsService = questsService;

            questLogComponentView.OnPinChange += (id, isPinned) => OnPinChange?.Invoke(id, isPinned);
            questLogComponentView.OnQuestAbandon += (id) => OnQuestAbandon?.Invoke(id);
            questLogComponentView.OnJumpIn += (coords) => OnJumpIn?.Invoke(coords);
        }

        public async UniTaskVoid AddActiveQuest(QuestDetailsComponentModel activeQuest, CancellationToken ct = default)
        {
            string creatorName = await GetUsername(activeQuest.questCreator, CancellationTokenSource.CreateLinkedTokenSource(ct, disposeCts.Token).Token);
            List<QuestRewardComponentModel> questRewards = new List<QuestRewardComponentModel>();

            foreach (QuestReward questReward in await questsService.GetQuestRewards(activeQuest.questDefinitionId, ct))
            {
                questRewards.Add(new QuestRewardComponentModel()
                {
                    imageUri = questReward.image_link,
                    name = questReward.name
                });
                activeQuest.questRewards = questRewards;
            }

            Debug.Log(activeQuest.questName + " " + activeQuest.questRewards.Count);
            questLogComponentView.AddActiveQuest(activeQuest, creatorName);
        }

        public async UniTaskVoid AddCompletedQuest(QuestDetailsComponentModel completedQuest, CancellationToken ct = default)
        {
            string creatorName = await GetUsername(completedQuest.questCreator, CancellationTokenSource.CreateLinkedTokenSource(ct, disposeCts.Token).Token);
            List<QuestRewardComponentModel> questRewards = new List<QuestRewardComponentModel>();

            foreach (QuestReward questReward in await questsService.GetQuestRewards(completedQuest.questDefinitionId, ct))
            {
                questRewards.Add(new QuestRewardComponentModel()
                {
                    imageUri = questReward.image_link,
                    name = questReward.name
                });
                completedQuest.questRewards = questRewards;
            }

            questLogComponentView.AddCompletedQuest(completedQuest, creatorName);
        }

        public void RemoveQuestIfExists(string questId) =>
            questLogComponentView.RemoveQuestIfExists(questId);

        public void SetAsFullScreenMenuMode(Transform parentTransform) =>
            questLogComponentView.SetAsFullScreenMenuMode(parentTransform);

        public void SetIsGuest(bool isGuest) =>
            questLogComponentView.SetIsGuest(isGuest);

        private async UniTask<string> GetUsername(string userId, CancellationToken cancellationToken)
        {
            var profile = userProfileBridge.Get(userId);
            if (profile != null)
                return profile.userName.ToUpper();

            profile ??= await userProfileBridge.RequestFullUserProfileAsync(userId, cancellationToken);
            return profile.userName.ToUpper();
        }

        public void Dispose() =>
            disposeCts?.SafeCancelAndDispose();
    }
}
