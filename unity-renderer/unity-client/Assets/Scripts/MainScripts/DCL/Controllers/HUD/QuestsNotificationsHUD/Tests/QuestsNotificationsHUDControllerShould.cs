using DCL;
using DCL.Huds.QuestsNotifications;
using DCL.QuestsController;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;

namespace Tests.QuestsNotificationsHUD
{
    public class QuestsNotificationsHUDControllerShould
    {
        private const string INEXISTENT_QUEST_ID = "noQuest";
        private const string INEXISTENT_SECTION_ID = "noSection";
        private const string MOCK_QUEST_ID = "quest0";
        private const string MOCK_SECTION_ID = "section0";
        private const string MOCK_TASK_ID = "task0";

        private IQuestsController questsController;
        private QuestsNotificationsHUDController hudController;
        private IQuestsNotificationsHUDView hudView;

        [SetUp]
        public void SetUp()
        {
            QuestModel questMock = new QuestModel { id = MOCK_QUEST_ID };
            QuestSection sectionMock = new QuestSection { id = MOCK_SECTION_ID };
            QuestTask taskMock = new QuestTask { id = MOCK_TASK_ID };
            sectionMock.tasks = new [] { taskMock };
            questMock.sections = new [] { sectionMock };
            DataStore.i.Quests.quests.Set(new []{(questMock.id, questMock)});

            questsController = Substitute.For<IQuestsController>();
            hudView = Substitute.For<IQuestsNotificationsHUDView>();
            hudController = Substitute.ForPartsOf<QuestsNotificationsHUDController>();
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
        public void CallViewWhenQuestCompleted()
        {
            hudController.Initialize(questsController);
            questsController.OnQuestCompleted += Raise.Event<QuestCompleted>(MOCK_QUEST_ID);

            hudView.Received().ShowQuestCompleted(DataStore.i.Quests.quests[MOCK_QUEST_ID]);
        }

        [Test]
        public void NotCallViewWhenQuestCompletedButQuestNotFound()
        {
            hudController.Initialize(questsController);
            questsController.OnQuestCompleted += Raise.Event<QuestCompleted>(INEXISTENT_QUEST_ID);

            hudView.DidNotReceiveWithAnyArgs().ShowQuestCompleted(null);
        }

        [Test]
        public void CallViewWhenSectionCompleted()
        {
            hudController.Initialize(questsController);
            questsController.OnSectionCompleted += Raise.Event<SectionCompleted>(MOCK_QUEST_ID, MOCK_SECTION_ID);

            hudView.Received().ShowSectionCompleted(DataStore.i.Quests.quests[MOCK_QUEST_ID].sections[0]);
        }

        [Test]
        public void NotCallViewWhenSectionCompletedButNoSectionFound()
        {
            hudController.Initialize(questsController);
            questsController.OnSectionCompleted += Raise.Event<SectionCompleted>(MOCK_QUEST_ID, INEXISTENT_SECTION_ID);

            hudView.DidNotReceiveWithAnyArgs().ShowSectionCompleted(null);
        }

        [Test]
        public void CallViewWhenSectionUnlocked()
        {
            hudController.Initialize(questsController);
            questsController.OnSectionUnlocked += Raise.Event<SectionUnlocked>(MOCK_QUEST_ID, MOCK_SECTION_ID);

            hudView.Received().ShowSectionUnlocked(DataStore.i.Quests.quests[MOCK_QUEST_ID].sections[0]);
        }

        [Test]
        public void NotCallViewWhenSectionUnlockedButNoSectionFound()
        {
            hudController.Initialize(questsController);
            questsController.OnSectionUnlocked += Raise.Event<SectionUnlocked>(MOCK_QUEST_ID, INEXISTENT_SECTION_ID);

            hudView.DidNotReceiveWithAnyArgs().ShowSectionUnlocked(null);
        }

        [TearDown]
        public void TearDown()
        {
            DataStore.Clear();
        }
    }
}
