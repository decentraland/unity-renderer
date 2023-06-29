using Cysharp.Threading.Tasks;
using DCL.Helpers;
using DCL.Tasks;
using DCLServices.QuestsService;
using Decentraland.Quests;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Action = Decentraland.Quests.Action;

namespace DCL.Quests
{
    public class QuestsController : IDisposable
    {
        private const string PINNED_QUEST_KEY = "PinnedQuestId";

        private readonly IQuestsService questsService;
        private readonly IQuestTrackerComponentView questTrackerComponentView;
        private readonly IQuestCompletedComponentView questCompletedComponentView;
        private readonly IQuestStartedPopupComponentView questStartedPopupComponentView;
        private readonly IQuestLogComponentView questLogComponentView;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly IPlayerPrefs playerPrefs;
        private readonly DataStore dataStore;
        private readonly ITeleportController teleportController;

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
            IPlayerPrefs playerPrefs,
            DataStore dataStore,
            ITeleportController teleportController)
        {
            this.questsService = questsService;
            this.questTrackerComponentView = questTrackerComponentView;
            this.questCompletedComponentView = questCompletedComponentView;
            this.questStartedPopupComponentView = questStartedPopupComponentView;
            this.questLogComponentView = questLogComponentView;
            this.userProfileBridge = userProfileBridge;
            this.playerPrefs = playerPrefs;
            this.dataStore = dataStore;
            this.teleportController = teleportController;

            disposeCts = new CancellationTokenSource();
            quests = new ();

            StartTrackingQuests(disposeCts.Token).Forget();
            StartTrackingStartedQuests(disposeCts.Token).Forget();

            questLogComponentView.SetIsGuest(userProfileBridge.GetOwn().isGuest);

            questStartedPopupComponentView.OnOpenQuestLog += () => { dataStore.HUDs.questsPanelVisible.Set(true); };
            dataStore.exploreV2.configureQuestInFullscreenMenu.OnChange += ConfigureQuestLogInFullscreenMenuChanged;
            ConfigureQuestLogInFullscreenMenuChanged(dataStore.exploreV2.configureQuestInFullscreenMenu.Get(), null);
            questLogComponentView.OnPinChange += ChangePinnedQuest;
            questLogComponentView.OnQuestAbandon += AbandonQuest;
            questTrackerComponentView.OnJumpIn += JumpIn;
            questLogComponentView.OnJumpIn += JumpIn;

            foreach (var questsServiceQuestInstance in questsService.QuestInstances)
                AddOrUpdateQuestToLog(questsServiceQuestInstance.Value);

            ChangePinnedQuest(playerPrefs.GetString(PINNED_QUEST_KEY, ""), true);
        }

        private void AbandonQuest(string questId)
        {
            if(pinnedQuestId.Get().Equals(questId))
                ChangePinnedQuest(questId, false);
            questsService.AbortQuest(questId).Forget();
            questLogComponentView.RemoveQuestIfExists(questId);
        }

        private void JumpIn(Vector2Int obj) =>
            teleportController.Teleport(obj.x, obj.y);

        private void ChangePinnedQuest(string questId, bool isPinned)
        {
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

        private async UniTaskVoid StartTrackingStartedQuests(CancellationToken ct)
        {
            await foreach (var questStateUpdate in questsService.QuestStarted.WithCancellation(ct))
            {
                await UniTask.SwitchToMainThread(ct);
                AddOrUpdateQuestToLog(questStateUpdate);
                questStartedPopupComponentView.SetQuestName(questStateUpdate.Quest.Name);
                questStartedPopupComponentView.SetVisible(true);

                if(string.IsNullOrEmpty(pinnedQuestId.Get()) || quests.ContainsKey(pinnedQuestId.Get()))
                    ChangePinnedQuest(questStateUpdate.Id, true);
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

        private void AddOrUpdateQuestToLog(QuestInstance questInstance, bool showCompletedQuestHUD = false)
        {
            quests.TryAdd(questInstance.Id, questInstance);

            QuestDetailsComponentModel quest = new QuestDetailsComponentModel()
            {
                questName = questInstance.Quest.Name,
                questCreator = "userName",
                questDescription = questInstance.Quest.Description,
                questId = questInstance.Id,
                isPinned = questInstance.Id == pinnedQuestId.Get(),
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
                    questCompletedComponentView.SetRewards(new List<QuestRewardComponentModel>());
                    questCompletedComponentView.SetVisible(true);
                }

                questLogComponentView.AddCompletedQuest(quest);
            }
        }

        public void Dispose()
        {
            questLogComponentView.OnPinChange -= ChangePinnedQuest;
            questTrackerComponentView.OnJumpIn -= JumpIn;
            questLogComponentView.OnJumpIn -= JumpIn;
            dataStore.exploreV2.configureQuestInFullscreenMenu.OnChange -= ConfigureQuestLogInFullscreenMenuChanged;
            disposeCts?.SafeCancelAndDispose();
        }
    }
}
