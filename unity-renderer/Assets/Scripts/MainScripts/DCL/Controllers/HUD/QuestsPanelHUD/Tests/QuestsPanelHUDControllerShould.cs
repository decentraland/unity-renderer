using DCL;
using DCL.Huds.QuestsPanel;
using DCL.QuestsController;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using System.Collections.Generic;

namespace Tests.QuestsPanelHUD
{
    public class QuestsPanelHUDControllerShould
    {
        private const string MOCK_QUEST_ID = "quest0";
        private const string MOCK_SECTION_ID = "section0";
        private const string MOCK_TASK_ID = "task0";

        private IQuestsController questsController;
        private QuestsPanelHUDController hudController;
        private IQuestsPanelHUDView hudView;

        [SetUp]
        public void SetUp()
        {
            QuestModel questMock = new QuestModel { id = MOCK_QUEST_ID };
            QuestSection sectionMock = new QuestSection { id = MOCK_SECTION_ID };
            QuestTask taskMock = new QuestTask { id = MOCK_TASK_ID };
            sectionMock.tasks = new [] { taskMock };
            questMock.sections = new [] { sectionMock };
            DataStore.i.Quests.quests.Set(new [] { (questMock.id, questMock) });

            questsController = Substitute.For<IQuestsController>();
            hudView = Substitute.For<IQuestsPanelHUDView>();
            hudController = Substitute.ForPartsOf<QuestsPanelHUDController>();
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
        [TestCase(true)]
        [TestCase(false)]
        public void ToggleViewVisibilityWhenActionPerformedForInitialState(bool initialState)
        {
            hudView.Configure().isVisible.Returns(initialState);

            hudController.Initialize(questsController);
            hudController.toggleQuestsPanel.RaiseOnTriggered();

            hudView.Received().SetVisibility(!initialState);
        }

        [Test]
        public void CallViewWhenQuestProgressed()
        {
            hudController.Initialize(questsController);
            questsController.OnQuestUpdated += Raise.Event<QuestUpdated>(MOCK_QUEST_ID, true);

            hudView.Received().RequestAddOrUpdateQuest(MOCK_QUEST_ID);
        }

        [Test]
        public void CallViewWhenQuestBlocked()
        {
            DataStore.i.Quests.quests[MOCK_QUEST_ID].status = QuestsLiterals.Status.BLOCKED;
            hudController.Initialize(questsController);
            questsController.OnQuestUpdated += Raise.Event<QuestUpdated>(MOCK_QUEST_ID, true);

            hudView.Received().RemoveQuest(MOCK_QUEST_ID);
        }

        [Test]
        public void CallViewWhenQuestRemoved()
        {
            hudController.Initialize(questsController);
            DataStore.i.Quests.quests.Remove(MOCK_QUEST_ID);

            hudView.Received().RemoveQuest(MOCK_QUEST_ID);
        }

        [Test]
        public void CallViewWhenQuestsSet()
        {
            hudController.Initialize(questsController);
            DataStore.i.Quests.quests.Set(new List<(string, QuestModel)>
            {
                ("newQuest1", new QuestModel { id = "newQuest1" }),
                ("newQuest2", new QuestModel { id = "newQuest2" }),
            });

            hudView.Received().RequestAddOrUpdateQuest("newQuest1");
            hudView.Received().RequestAddOrUpdateQuest("newQuest2");
        }

        [Test]
        public void NotShowNotStartedSecretQuests_Add()
        {
            hudController.Initialize(questsController);
            DataStore.i.Quests.quests.Set(new List<(string, QuestModel)>
            {
                ("newQuest1", new QuestModel
                {
                    id = "newQuest1",
                    status = QuestsLiterals.Status.NOT_STARTED,
                    visibility = QuestsLiterals.Visibility.SECRET
                }),
            });

            hudView.DidNotReceive().RequestAddOrUpdateQuest("newQuest1");
        }

        [Test]
        public void ShowNewStartedSecretQuests_Add()
        {
            hudController.Initialize(questsController);
            DataStore.i.Quests.quests.Set(new List<(string, QuestModel)>
            {
                ("newQuest1", new QuestModel
                {
                    id = "newQuest1",
                    status = QuestsLiterals.Status.ON_GOING,
                    visibility = QuestsLiterals.Visibility.SECRET
                }),
            });

            hudView.Received().RequestAddOrUpdateQuest("newQuest1");
        }

        [Test]
        public void NotShowNotStartedSecretQuests_Update()
        {
            DataStore.i.Quests.quests.Set(new List<(string, QuestModel)>
            {
                ("newQuest1", new QuestModel
                {
                    id = "newQuest1",
                    status = QuestsLiterals.Status.NOT_STARTED,
                    visibility = QuestsLiterals.Visibility.VISIBLE
                }),
            });

            hudController.Initialize(questsController);
            hudView.ClearReceivedCalls();
            DataStore.i.Quests.quests["newQuest1"].visibility = QuestsLiterals.Visibility.SECRET;
            questsController.OnQuestUpdated += Raise.Event<QuestUpdated>("newQuest1", true);
            hudView.DidNotReceive().RequestAddOrUpdateQuest("newQuest1");
        }

        [Test]
        public void ShowNewStartedSecretQuests_Update()
        {
            DataStore.i.Quests.quests.Set(new List<(string, QuestModel)>
            {
                ("newQuest1", new QuestModel
                {
                    id = "newQuest1",
                    status = QuestsLiterals.Status.NOT_STARTED,
                    visibility = QuestsLiterals.Visibility.SECRET
                }),
            });

            hudController.Initialize(questsController);
            hudView.ClearReceivedCalls();
            DataStore.i.Quests.quests["newQuest1"].status = QuestsLiterals.Status.NOT_STARTED;
            questsController.OnQuestUpdated += Raise.Event<QuestUpdated>("newQuest1", true);
            hudView.DidNotReceive().RequestAddOrUpdateQuest("newQuest1");
        }

        [TearDown]
        public void TearDown() { DataStore.Clear(); }
    }

}