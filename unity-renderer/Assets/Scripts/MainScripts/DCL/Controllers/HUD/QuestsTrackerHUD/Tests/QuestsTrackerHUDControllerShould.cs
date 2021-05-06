using System.Collections.Generic;
using DCL;
using DCL.Huds.QuestsTracker;
using DCL.QuestsController;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using System;

namespace Tests.QuestsTrackerHUD
{
    public class QuestsTrackerHUDControllerShould
    {
        private const string MOCK_QUEST_ID = "quest0";
        private const string MOCK_SECTION_ID = "section0";
        private const string MOCK_TASK_ID = "task0";
        private const string MOCK_REWARD_ID = "reward0";

        private IQuestsController questsController;
        private QuestsTrackerHUDController hudController;
        private IQuestsTrackerHUDView hudView;

        [SetUp]
        public void SetUp()
        {
            QuestModel questMock = new QuestModel { id = MOCK_QUEST_ID, status = QuestsLiterals.Status.ON_GOING };
            QuestSection sectionMock = new QuestSection { id = MOCK_SECTION_ID };
            QuestTask taskMock = new QuestTask { id = MOCK_TASK_ID };
            QuestReward rewardMock = new QuestReward { id = MOCK_REWARD_ID };
            sectionMock.tasks = new [] { taskMock };
            questMock.sections = new [] { sectionMock };
            questMock.rewards = new [] { rewardMock };
            DataStore.i.Quests.quests.Set(new []
            {
                (questMock.id, questMock)
            });

            questsController = Substitute.For<IQuestsController>();
            hudView = Substitute.For<IQuestsTrackerHUDView>();
            hudController = Substitute.ForPartsOf<QuestsTrackerHUDController>();
            hudController.Configure().CreateView().Returns(info => hudView);
        }

        [Test]
        public void InitializeProperly()
        {
            hudController.Initialize(questsController);

            Assert.AreEqual(questsController, hudController.questsController);
            Assert.AreEqual(hudView, hudController.view);
        }

        [Test]
        public void CallViewWhenQuestProgressed()
        {
            hudController.Initialize(questsController);
            questsController.OnQuestUpdated += Raise.Event<QuestUpdated>(MOCK_QUEST_ID, true);

            hudView.Received().UpdateQuest(MOCK_QUEST_ID, true);
        }

        [Test]
        public void CallViewWhenPinnedQuestAdded()
        {
            hudController.Initialize(questsController);
            DataStore.i.Quests.pinnedQuests.Add(MOCK_QUEST_ID);

            hudView.Received().PinQuest(MOCK_QUEST_ID);
        }

        [Test]
        public void CallViewWhenQuestRemoved()
        {
            hudController.Initialize(questsController);
            DataStore.i.Quests.quests.Remove(MOCK_QUEST_ID);

            hudView.Received().RemoveEntry(MOCK_QUEST_ID);
        }

        [Test]
        public void CallViewWhenPinnedQuestsSet()
        {
            DataStore.i.Quests.quests.Set(new []
            {
                ("newQuest1", new QuestModel { id = "newQuest1" }),
                ("newQuest2", new QuestModel { id = "newQuest2" })
            });

            hudController.Initialize(questsController);
            DataStore.i.Quests.pinnedQuests.Set(new []
            {
                "newQuest1", "newQuest2"
            });
            hudView.Received().ClearEntries();
            hudView.Received().PinQuest("newQuest1");
            hudView.Received().PinQuest("newQuest2");
        }

        [Test]
        public void CallViewWhenQuestsSet()
        {
            DataStore.i.Quests.pinnedQuests.Set(new []
            {
                "newQuest1", "newQuest2"
            });
            hudController.Initialize(questsController);
            DataStore.i.Quests.quests.Set(new List<(string, QuestModel)>
            {
                ("newQuest1", new QuestModel
                {
                    id = "newQuest1"
                }),
                ("newQuest2", new QuestModel
                {
                    id = "newQuest2"
                }),
            });

            hudView.Received().ClearEntries();
            hudView.Received().PinQuest("newQuest1");
            hudView.Received().PinQuest("newQuest2");
        }

        [Test]
        public void CallViewWhenRewardObtained()
        {
            hudController.Initialize(questsController);

            questsController.OnRewardObtained += Raise.Event<RewardObtained>(MOCK_QUEST_ID, MOCK_REWARD_ID);

            hudView.Received().AddReward(MOCK_QUEST_ID, DataStore.i.Quests.quests[MOCK_QUEST_ID].rewards[0]);
        }

        [TearDown]
        public void TearDown() { DataStore.Clear(); }
    }

}