using System.Collections;
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
            Assert.IsTrue(hudView.questEntries[MOCK_QUEST_ID].progressInTitle.gameObject.activeSelf);
            Assert.IsFalse(hudView.questEntries[MOCK_QUEST_ID].completedProgressInTitle.gameObject.activeSelf);
            Assert.IsFalse(hudView.questEntries[MOCK_QUEST_ID].completedMarkInTitle.gameObject.activeSelf);
            Assert.AreEqual(0f, hudView.questEntries[MOCK_QUEST_ID].progressInTitle.transform.localScale.x);
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
            Assert.IsTrue(hudView.questEntries[MOCK_QUEST_ID].progressInTitle.gameObject.activeSelf);
            Assert.IsFalse(hudView.questEntries[MOCK_QUEST_ID].completedProgressInTitle.gameObject.activeSelf);
            Assert.IsFalse(hudView.questEntries[MOCK_QUEST_ID].completedMarkInTitle.gameObject.activeSelf);
            Assert.AreEqual(0.8f, hudView.questEntries[MOCK_QUEST_ID].progressInTitle.transform.localScale.x);
        }

        [Test]
        public void AddOrUpdateQuestCompletedProperly()
        {
            DataStore.i.Quests.quests[MOCK_QUEST_ID].name = "newName";
            DataStore.i.Quests.quests[MOCK_QUEST_ID].description = "newDescription";
            DataStore.i.Quests.quests[MOCK_QUEST_ID].sections[0].tasks[0].progress = 1f;
            DataStore.i.Quests.quests[MOCK_QUEST_ID].sections[0].progress = 1f;
            DataStore.i.Quests.quests[MOCK_QUEST_ID].status = QuestsLiterals.Status.COMPLETED;

            hudView.AddOrUpdateQuest(MOCK_QUEST_ID);

            Assert.AreEqual(1, hudView.questEntries.Count);
            Assert.IsTrue(hudView.questEntries.ContainsKey(MOCK_QUEST_ID));
            Assert.AreEqual("newName", hudView.questEntries[MOCK_QUEST_ID].questName.text);
            Assert.AreEqual("newDescription", hudView.questEntries[MOCK_QUEST_ID].description.text);
            Assert.IsTrue(hudView.questEntries[MOCK_QUEST_ID].completedProgressInTitle.gameObject.activeSelf);
            Assert.IsTrue(hudView.questEntries[MOCK_QUEST_ID].completedMarkInTitle.gameObject.activeSelf);
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

        [UnityTest]
        public IEnumerator OrderQuests()
        {
            DataStore.i.Quests.quests.Add("0", new QuestModel { id = "0", sections = new [] { new QuestSection { id = "0_0", tasks = new [] { new QuestTask { id = "0_0_0" } } } } });
            DataStore.i.Quests.quests.Add("1", new QuestModel { id = "1", sections = new [] { new QuestSection { id = "1_0", tasks = new [] { new QuestTask { id = "1_0_0" } } } } });
            DataStore.i.Quests.quests.Add("2", new QuestModel { id = "2", sections = new [] { new QuestSection { id = "2_0", tasks = new [] { new QuestTask { id = "2_0_0" } } } }, status = QuestsLiterals.Status.COMPLETED });
            DataStore.i.Quests.quests.Add("3", new QuestModel { id = "3", sections = new [] { new QuestSection { id = "3_0", tasks = new [] { new QuestTask { id = "3_0_0" } } } } });
            /* Order should be
             * Available
             * 0
             * 1
             * 3
             *
             * Completed
             * 2
             */

            hudView.AddOrUpdateQuest("0");
            hudView.AddOrUpdateQuest("1");
            hudView.AddOrUpdateQuest("2");
            hudView.AddOrUpdateQuest("3");

            yield return null;

            Assert.AreEqual(0, hudView.questEntries["0"].transform.GetSiblingIndex());
            Assert.AreEqual(1, hudView.questEntries["1"].transform.GetSiblingIndex());
            Assert.AreEqual(2, hudView.questEntries["3"].transform.GetSiblingIndex());
            Assert.AreEqual(0, hudView.questEntries["2"].transform.GetSiblingIndex());
            Assert.AreEqual(hudView.availableQuestsContainer, hudView.questEntries["0"].transform.parent);
            Assert.AreEqual(hudView.availableQuestsContainer, hudView.questEntries["1"].transform.parent);
            Assert.AreEqual(hudView.availableQuestsContainer, hudView.questEntries["3"].transform.parent);
            Assert.AreEqual(hudView.completedQuestsContainer, hudView.questEntries["2"].transform.parent);
        }

        [TearDown]
        public void TearDown() { Object.Destroy(hudView.gameObject); }
    }
}