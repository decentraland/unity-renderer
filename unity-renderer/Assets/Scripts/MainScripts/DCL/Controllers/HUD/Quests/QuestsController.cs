using Cysharp.Threading.Tasks;
using DCL.Tasks;
using DCLServices.QuestsService;
using Decentraland.Quests;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DCL.Quests
{
    public class QuestsController : IDisposable
    {
        private readonly IQuestsService questsService;
        private readonly IQuestTrackerComponentView questTrackerComponentView;
        private readonly IQuestCompletedComponentView questCompletedComponentView;
        private readonly IQuestStartedPopupComponentView questStartedPopupComponentView;
        private readonly IQuestLogComponentView questLogComponentView;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly DataStore dataStore;

        private CancellationTokenSource disposeCts = null;
        private CancellationTokenSource profileCts = null;

        private BaseVariable<string> pinnedQuestId => dataStore.Quests.pinnedQuest;
        private Dictionary<string, QuestInstance> quests;

        public QuestsController(
            IQuestsService questsService,
            IQuestTrackerComponentView questTrackerComponentView,
            IQuestCompletedComponentView questCompletedComponentView,
            IQuestStartedPopupComponentView questStartedPopupComponentView,
            IQuestLogComponentView questLogComponentView,
            IUserProfileBridge userProfileBridge,
            DataStore dataStore)
        {
            this.questsService = questsService;
            this.questTrackerComponentView = questTrackerComponentView;
            this.questCompletedComponentView = questCompletedComponentView;
            this.questStartedPopupComponentView = questStartedPopupComponentView;
            this.questLogComponentView = questLogComponentView;
            this.userProfileBridge = userProfileBridge;
            this.dataStore = dataStore;
            disposeCts = new CancellationTokenSource();
            quests = new ();
            if (questsService != null)
            {
                StartTrackingQuests(disposeCts.Token).Forget();
                StartTrackingStartedQuests(disposeCts.Token).Forget();
            }
            questLogComponentView.SetIsGuest(userProfileBridge.GetOwn().isGuest);
            questStartedPopupComponentView.OnOpenQuestLog += () => { dataStore.HUDs.questsPanelVisible.Set(true); };
            dataStore.exploreV2.configureQuestInFullscreenMenu.OnChange += ConfigureQuestLogInFullscreenMenuChanged;
            ConfigureQuestLogInFullscreenMenuChanged(dataStore.exploreV2.configureQuestInFullscreenMenu.Get(), null);
            questLogComponentView.OnPinChange += ChangePinnedQuest;
            foreach (var questsServiceQuestInstance in questsService.QuestInstances)
                AddOrUpdateQuestToLog(questsServiceQuestInstance.Value);

        }

        private void ChangePinnedQuest(string questId, bool isPinned)
        {
            string previousPinnedQuestId = pinnedQuestId.Get();

            pinnedQuestId.Set(isPinned ? questId : "");

            if (!string.IsNullOrEmpty(previousPinnedQuestId))
                AddOrUpdateQuestToLog(quests[previousPinnedQuestId]);

            if (string.IsNullOrEmpty(pinnedQuestId.Get()))
            {
                questTrackerComponentView.SetVisible(false);
                return;
            }

            questTrackerComponentView.SetVisible(true);
            questTrackerComponentView.SetQuestTitle(quests[questId].Quest.Name);
            questTrackerComponentView.SetQuestSteps(GetQuestSteps(quests[questId]));

            AddOrUpdateQuestToLog(quests[pinnedQuestId.Get()]);
        }

        private void ConfigureQuestLogInFullscreenMenuChanged(Transform current, Transform previous) =>
            questLogComponentView.SetAsFullScreenMenuMode(current);

        private async UniTaskVoid StartTrackingQuests(CancellationToken ct)
        {
            await foreach (var questInstance in questsService.QuestUpdated.WithCancellation(ct))
            {
                await UniTask.SwitchToMainThread(ct);
                AddOrUpdateQuestToLog(questInstance, true);
                if (questInstance.Id != pinnedQuestId.Get())
                    continue;

                questTrackerComponentView.SetQuestTitle(questInstance.Quest.Name);
                questTrackerComponentView.SetQuestSteps(GetQuestSteps(questInstance));
            }
        }

        private List<QuestStepComponentModel> GetQuestSteps(QuestInstance questInstance, bool includePreviousSteps = false)
        {
            List<QuestStepComponentModel> questSteps = new List<QuestStepComponentModel>();

            if(includePreviousSteps)
                foreach (var step in questInstance.State.StepsCompleted)
                    questSteps.Add(new QuestStepComponentModel { isCompleted = true, text = step });

            foreach (var step in questInstance.State.CurrentSteps)
            {
                foreach (Task task in step.Value.TasksCompleted)
                    questSteps.Add(new QuestStepComponentModel { isCompleted = true, text = task.Id });

                foreach (Task task in step.Value.ToDos)
                    questSteps.Add(new QuestStepComponentModel { isCompleted = false, text = task.Id });
            }

            return questSteps;
        }

        private async UniTaskVoid StartTrackingStartedQuests(CancellationToken ct)
        {
            await foreach (var questStateUpdate in questsService.QuestStarted.WithCancellation(ct))
            {
                await UniTask.SwitchToMainThread(ct);
                AddOrUpdateQuestToLog(questStateUpdate);
                questStartedPopupComponentView.SetQuestName(questStateUpdate.Quest.Name);
                questStartedPopupComponentView.SetVisible(true);
            }
        }

        private void AddOrUpdateQuestToLog(QuestInstance questInstance, bool showCompletedQuestHUD = false)
        {
            if (!quests.ContainsKey(questInstance.Id))
                quests.Add(questInstance.Id, questInstance);

            /*
            profileCts?.Cancel();
            profileCts?.Dispose();
            profileCts = new CancellationTokenSource();

            quests[questInstance.Id] = questInstance;
            string userName = await GetCreatorName(questInstance.Quest.CreatorAddress, profileCts);
            */
            QuestDetailsComponentModel quest = new QuestDetailsComponentModel()
            {
                questName = questInstance.Quest.Name,
                questCreator = "userName",
                questDescription = questInstance.Quest.Description,
                questId = questInstance.Id,
                isPinned = questInstance.Id == pinnedQuestId.Get(),
                //coordinates = questInstance.Quest.Coordinates,
                questSteps = GetQuestSteps(questInstance, true),
                questRewards = new List<QuestRewardComponentModel>()
            };

            if(questInstance.State.CurrentSteps.Count > 0)
                questLogComponentView.AddActiveQuest(quest);
            else
            {
                ChangePinnedQuest(questInstance.Id, false);

                if (showCompletedQuestHUD)
                {
                    questCompletedComponentView.SetTitle(quest.questName);
                    questCompletedComponentView.SetIsGuest(userProfileBridge.GetOwn().isGuest);
                    questCompletedComponentView.SetVisible(true);
                }

                questLogComponentView.AddCompletedQuest(quest);
            }
        }

        private async UniTask<string> GetCreatorName(string userId, CancellationTokenSource cts)
        {
            UserProfile creatorProfile = userProfileBridge.Get(userId);
            creatorProfile ??= await userProfileBridge.RequestFullUserProfileAsync(userId, cts.Token);
            return creatorProfile.hasClaimedName ? creatorProfile.userName : creatorProfile.userName + userId;
        }

        public void Dispose() =>
            disposeCts?.SafeCancelAndDispose();
    }
}
