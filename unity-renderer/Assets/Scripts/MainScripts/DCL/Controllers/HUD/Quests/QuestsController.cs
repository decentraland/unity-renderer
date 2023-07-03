using Cysharp.Threading.Tasks;
using DCL.Helpers;
using DCL.Tasks;
using DCLServices.QuestsService;
using Decentraland.Quests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Action = Decentraland.Quests.Action;
using Task = Decentraland.Quests.Task;

namespace DCL.Quests
{
    public class QuestsController : IDisposable
    {
        private const string PINNED_QUEST_KEY = "PinnedQuestId";

        private readonly IQuestsService questsService;
        private readonly IQuestTrackerComponentView questTrackerComponentView;
        private readonly IQuestCompletedComponentView questCompletedComponentView;
        private readonly IQuestStartedPopupComponentView questStartedPopupComponentView;
        private readonly QuestLogController questLogController;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly IPlayerPrefs playerPrefs;
        private readonly DataStore dataStore;
        private readonly ITeleportController teleportController;
        private readonly IQuestAnalyticsService questAnalyticsService;
        private Service<IWebRequestController> webRequestController;

        private CancellationTokenSource disposeCts = null;
        private CancellationTokenSource profileCts = null;

        private BaseVariable<string> pinnedQuestId => dataStore.Quests.pinnedQuest;
        private Dictionary<string, QuestInstance> quests;

        public QuestsController(
            IQuestsService questsService,
            IQuestTrackerComponentView questTrackerComponentView,
            IQuestCompletedComponentView questCompletedComponentView,
            IQuestStartedPopupComponentView questStartedPopupComponentView,
            IUserProfileBridge userProfileBridge,
            IPlayerPrefs playerPrefs,
            DataStore dataStore,
            ITeleportController teleportController,
            QuestLogController questLogController,
            IQuestAnalyticsService questAnalyticsService)
        {
            this.questsService = questsService;
            this.questTrackerComponentView = questTrackerComponentView;
            this.questCompletedComponentView = questCompletedComponentView;
            this.questStartedPopupComponentView = questStartedPopupComponentView;
            this.userProfileBridge = userProfileBridge;
            this.playerPrefs = playerPrefs;
            this.dataStore = dataStore;
            this.teleportController = teleportController;
            this.questLogController = questLogController;
            this.questAnalyticsService = questAnalyticsService;

            disposeCts = new CancellationTokenSource();
            quests = new ();

            StartTrackingQuests(disposeCts.Token).Forget();
            StartTrackingStartedQuests(disposeCts.Token).Forget();

            questLogController.SetIsGuest(userProfileBridge.GetOwn().isGuest);

            questStartedPopupComponentView.OnOpenQuestLog += () => { dataStore.HUDs.questsPanelVisible.Set(true); };
            dataStore.exploreV2.configureQuestInFullscreenMenu.OnChange += ConfigureQuestLogInFullscreenMenuChanged;
            ConfigureQuestLogInFullscreenMenuChanged(dataStore.exploreV2.configureQuestInFullscreenMenu.Get(), null);
            questLogController.OnPinChange += ChangePinnedQuest;
            questLogController.OnQuestAbandon += AbandonQuest;
            questTrackerComponentView.OnJumpIn += JumpIn;
            questLogController.OnJumpIn += JumpIn;

            foreach (var questsServiceQuestInstance in questsService.QuestInstances)
                AddOrUpdateQuestToLog(questsServiceQuestInstance.Value);

            ChangePinnedQuest(playerPrefs.GetString(PINNED_QUEST_KEY, ""), true, false);
        }

        private void AbandonQuest(string questId)
        {
            if(pinnedQuestId.Get().Equals(questId))
                ChangePinnedQuest(questId, false);
            questsService.AbortQuest(questId).Forget();
            questLogController.RemoveQuestIfExists(questId);
            questAnalyticsService.SendQuestCancelled(questId);
        }

        private void JumpIn(Vector2Int obj) =>
            teleportController.Teleport(obj.x, obj.y);

        private void ChangePinnedQuest(string questId, bool isPinned) =>
            ChangePinnedQuest(questId, isPinned, true);

        private void ChangePinnedQuest(string questId, bool isPinned, bool sendAnalytics)
        {
            if(sendAnalytics)
                questAnalyticsService.SendPinnedQuest(questId, isPinned);

            string previousPinnedQuestId = pinnedQuestId.Get();

            pinnedQuestId.Set(isPinned ? questId : "");

            playerPrefs.Set(PINNED_QUEST_KEY, pinnedQuestId.Get());
            playerPrefs.Save();

            if (!string.IsNullOrEmpty(previousPinnedQuestId))
                AddOrUpdateQuestToLog(quests[previousPinnedQuestId]);

            if (string.IsNullOrEmpty(pinnedQuestId.Get()) || !quests.ContainsKey(questId))
            {
                questTrackerComponentView.SetVisible(false);
                return;
            }

            questTrackerComponentView.SetVisible(true);
            questTrackerComponentView.SetQuestTitle(quests[questId].Quest.Name);
            questTrackerComponentView.SetQuestSteps(GetQuestSteps(quests[questId]));

            AddOrUpdateQuestToLog(quests[pinnedQuestId.Get()]);
        }

        private void ConfigureQuestLogInFullscreenMenuChanged(Transform current, Transform previous)
        {
            questLogController.SetAsFullScreenMenuMode(current);
        }

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

        private async UniTaskVoid StartTrackingStartedQuests(CancellationToken ct)
        {
            await foreach (var questStateUpdate in questsService.QuestStarted.WithCancellation(ct))
            {
                await UniTask.SwitchToMainThread(ct);
                AddOrUpdateQuestToLog(questStateUpdate);
                questStartedPopupComponentView.SetQuestName(questStateUpdate.Quest.Name);
                questStartedPopupComponentView.SetVisible(true);
                questAnalyticsService.SendQuestStarted(questStateUpdate.Quest.Id);

                if(string.IsNullOrEmpty(pinnedQuestId.Get()) || quests.ContainsKey(pinnedQuestId.Get()))
                    ChangePinnedQuest(questStateUpdate.Id, true, false);
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
                {
                    var supportsJumpIn = false;
                    Vector2Int coordinates = Vector2Int.zero;
                    foreach (Action taskActionItem in task.ActionItems)
                    {
                        if (taskActionItem.Type == "LOCATION" && taskActionItem.Parameters.TryGetValue("x", out string xCoordinate) && taskActionItem.Parameters.TryGetValue("y", out string yCoordinate))
                        {
                            supportsJumpIn = true;
                            coordinates = new Vector2Int(int.Parse(xCoordinate), int.Parse(yCoordinate));
                        }
                    }

                    questSteps.Add(new QuestStepComponentModel { isCompleted = false, text = task.Id, supportsJumpIn = supportsJumpIn, coordinates = coordinates});
                }
            }

            return questSteps;
        }

        private void AddOrUpdateQuestToLog(QuestInstance questInstance, bool showCompletedQuestHUD = false, bool isQuestUpdate = false)
        {
            quests.TryAdd(questInstance.Id, questInstance);

            QuestDetailsComponentModel quest = new QuestDetailsComponentModel()
            {
                questName = questInstance.Quest.Name,
                questCreator = questInstance.Quest.CreatorAddress,
                questDescription = questInstance.Quest.Description,
                questId = questInstance.Id,
                questDefinitionId = questInstance.Quest.Id,
                isPinned = questInstance.Id == pinnedQuestId.Get(),
                questImageUri = questInstance.Quest.ImageUrl,
                questSteps = GetQuestSteps(questInstance, true),
                questRewards = new List<QuestRewardComponentModel>()
            };

            if (questInstance.State.CurrentSteps.Count > 0)
            {
                questLogController.AddActiveQuest(quest).Forget();
                questAnalyticsService.SendQuestProgressed(questInstance.Quest.Id, questInstance.State.CurrentSteps.Keys.ToList(), questInstance.State.CurrentSteps.Keys.ToList());
            }
            else
            {
                ChangePinnedQuest(questInstance.Id, false, false);

                if (showCompletedQuestHUD)
                {
                    ShowQuestCompleted(questInstance, disposeCts.Token).Forget();
                }

                questLogController.AddCompletedQuest(quest).Forget();
            }
        }

        private async UniTaskVoid ShowQuestCompleted(QuestInstance questInstance, CancellationToken ct)
        {
            List<QuestRewardComponentModel> questRewards = new List<QuestRewardComponentModel>();
            foreach (QuestReward questReward in await questsService.GetQuestRewards(questInstance.Quest.Id, ct))
            {
                questRewards.Add(new QuestRewardComponentModel()
                {
                    imageUri = questReward.image_link,
                    name = questReward.name
                });
            }
            questAnalyticsService.SendQuestCompleted(questInstance.Quest.Id);
            questCompletedComponentView.SetTitle(questInstance.Quest.Name);
            questCompletedComponentView.SetRewards(questRewards);
            questCompletedComponentView.SetIsGuest(userProfileBridge.GetOwn().isGuest);
            questCompletedComponentView.SetVisible(true);
        }

        public void Dispose()
        {
            questLogController.OnPinChange -= ChangePinnedQuest;
            questTrackerComponentView.OnJumpIn -= JumpIn;
            questLogController.OnJumpIn -= JumpIn;
            dataStore.exploreV2.configureQuestInFullscreenMenu.OnChange -= ConfigureQuestLogInFullscreenMenuChanged;
            disposeCts?.SafeCancelAndDispose();
        }
    }
}
