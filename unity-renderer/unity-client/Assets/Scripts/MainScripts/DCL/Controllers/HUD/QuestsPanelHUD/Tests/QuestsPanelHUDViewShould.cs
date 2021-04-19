using DCL;
using DCL.Huds.QuestsPanel;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.QuestsPanelHUD
{
    public class QuestsPanelHUDViewShould
    {
        private const string MOCK_QUEST_ID = "quest0";
        private const string MOCK_SECTION_ID = "section0";
        private const string MOCK_TASK_ID = "task0";

        private QuestsPanelHUDView hudView;

        [SetUp]
        public void SetUp()
        {
            QuestModel questMock = new QuestModel
            {
                id = MOCK_QUEST_ID,
                name = "name",
                description = "description",
            };
            QuestSection sectionMock = new QuestSection { id = MOCK_SECTION_ID };
            QuestTask taskMock = new QuestTask
            {
                id = MOCK_TASK_ID,
                type = "single",
                payload = JsonUtility.ToJson(new TaskPayload_Single { isDone = false })
            };
            sectionMock.tasks = new [] { taskMock };
            questMock.sections = new [] { sectionMock };
            DataStore.i.Quests.quests.Set(new [] { (questMock.id, questMock) });

            QuestsPanelHUDView.ENTRIES_PER_FRAME = 1;
            hudView = Object.Instantiate(Resources.Load<GameObject>("QuestsPanelHUD")).GetComponent<QuestsPanelHUDView>();
        }

        [Test]
        public void BeEmptyOnInitialize()
        {
            Assert.AreEqual(0, hudView.questEntries.Count);
            Assert.AreEqual(0, hudView.questsToBeAdded.Count);
            Assert.IsFalse(hudView.questPopup.gameObject.activeSelf);
        }

        [Test]
        public void ReactToRequestAddOrUpdateQuest()
        {
            hudView.RequestAddOrUpdateQuest(MOCK_QUEST_ID);

            Assert.AreEqual(1, hudView.questsToBeAdded.Count);
        }

        [Test]
        public void ReactToRequestAddOrUpdateQuest_NoRepeat()
        {
            hudView.RequestAddOrUpdateQuest(MOCK_QUEST_ID);
            hudView.RequestAddOrUpdateQuest(MOCK_QUEST_ID);
            Assert.AreEqual(1, hudView.questsToBeAdded.Count);
        }

        [Test]
        public void AddOrUpdateQuestProperly_NotExistentQuest()
        {
            hudView.AddOrUpdateQuest("NoQuest");
            LogAssert.Expect(LogType.Error, $"Couldn't find quest with ID NoQuest in DataStore");
            Assert.AreEqual(0, hudView.questEntries.Count);
        }

        [Test]
        public void AddOrUpdateQuestProperly_AddQuest()
        {
            hudView.AddOrUpdateQuest(MOCK_QUEST_ID);
            Assert.AreEqual(1, hudView.questEntries.Count);
            Assert.IsTrue(hudView.questEntries.ContainsKey(MOCK_QUEST_ID));
            Assert.AreEqual("name", hudView.questEntries[MOCK_QUEST_ID].questName.text);
            Assert.AreEqual("description", hudView.questEntries[MOCK_QUEST_ID].description.text);
        }

        [Test]
        public void AddOrUpdateQuestProperly_UpdateQuest()
        {
            hudView.AddOrUpdateQuest(MOCK_QUEST_ID);
            DataStore.i.Quests.quests[MOCK_QUEST_ID].name = "newName";
            DataStore.i.Quests.quests[MOCK_QUEST_ID].description = "newDescription";
            DataStore.i.Quests.quests[MOCK_QUEST_ID].sections[0].progress = 0.8f;

            hudView.AddOrUpdateQuest(MOCK_QUEST_ID);

            Assert.AreEqual(1, hudView.questEntries.Count);
            Assert.IsTrue(hudView.questEntries.ContainsKey(MOCK_QUEST_ID));
            Assert.AreEqual("newName", hudView.questEntries[MOCK_QUEST_ID].questName.text);
            Assert.AreEqual("newDescription", hudView.questEntries[MOCK_QUEST_ID].description.text);
            Assert.AreEqual(0.8f, hudView.questEntries[MOCK_QUEST_ID].progressInTitle.fillAmount);
        }

        [Test]
        public void RemoveQuestProperly_QuestEntries()
        {
            hudView.AddOrUpdateQuest(MOCK_QUEST_ID);

            hudView.RemoveQuest(MOCK_QUEST_ID);
            Assert.AreEqual(0, hudView.questEntries.Count);
        }

        [Test]
        public void RemoveQuestProperly_QuestRequest()
        {
            hudView.RequestAddOrUpdateQuest(MOCK_QUEST_ID);

            hudView.RemoveQuest(MOCK_QUEST_ID);
            Assert.AreEqual(0, hudView.questsToBeAdded.Count);
        }

        [Test]
        public void ShowPopup_NoExistentQuest()
        {
            hudView.ShowQuestPopup("NoQuest");

            Assert.IsFalse(hudView.questPopup.gameObject.activeSelf);
        }

        [Test]
        public void ShowPopup()
        {
            hudView.AddOrUpdateQuest(MOCK_QUEST_ID);
            hudView.ShowQuestPopup(MOCK_QUEST_ID);

            Assert.IsTrue(hudView.questPopup.gameObject.activeSelf);
        }

        [TearDown]
        public void TearDown() { Object.Destroy(hudView.gameObject); }
    }
}