using Cysharp.Threading.Tasks;
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

        public event Action<string, bool> OnPinChange;
        public event Action<Vector2Int> OnJumpIn;
        public event Action<string> OnQuestAbandon;

        public QuestLogController(
            IQuestLogComponentView questLogComponentView,
            IUserProfileBridge userProfileBridge)
        {
            this.questLogComponentView = questLogComponentView;
            this.userProfileBridge = userProfileBridge;

            questLogComponentView.OnPinChange += (id, isPinned) => OnPinChange?.Invoke(id, isPinned);
            questLogComponentView.OnQuestAbandon += (id) => OnQuestAbandon?.Invoke(id);
            questLogComponentView.OnJumpIn += (coords) => OnJumpIn?.Invoke(coords);
        }

        public async UniTaskVoid AddActiveQuest(QuestDetailsComponentModel activeQuest)
        {
            string creatorName = await GetUsername(activeQuest.questCreator, CancellationToken.None);
            questLogComponentView.AddActiveQuest(activeQuest, creatorName);
        }

        public async UniTaskVoid AddCompletedQuest(QuestDetailsComponentModel completedQuest)
        {
            string creatorName = await GetUsername(completedQuest.questCreator, CancellationToken.None);
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

        public void Dispose() { }
    }
}
