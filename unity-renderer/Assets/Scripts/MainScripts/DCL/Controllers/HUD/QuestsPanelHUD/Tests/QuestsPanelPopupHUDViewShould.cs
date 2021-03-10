using DCL;
using DCL.Huds.QuestsPanel;
using NUnit.Framework;
using UnityEngine;

namespace Tests.QuestsPanelHUD
{
    public class QuestsPanelPopupHUDViewShould
    {
        private const string MOCK_QUEST_ID = "quest0";
        private const string MOCK_SECTION_ID = "section0";
        private const string MOCK_TASK_ID = "task0";

        private QuestsPanelHUDView hudView;

        [SetUp]
        public void SetUp()
        {
            QuestModel questMock = new QuestModel { id = MOCK_QUEST_ID, };
            QuestSection sectionMock = new QuestSection { id = MOCK_SECTION_ID, };
            QuestTask taskMock = new QuestTask
            {
                id = MOCK_TASK_ID,
                type = "single",
                payload = JsonUtility.ToJson(new TaskPayload_Single{isDone = false})
            };
            sectionMock.tasks = new [] { taskMock };
            questMock.sections = new [] { sectionMock };
            DataStore.i.Quests.quests.Set(new [] { (questMock.id, questMock) });

            QuestsPanelHUDView.ENTRIES_PER_FRAME = 1;
            hudView = Object.Instantiate(Resources.Load<GameObject>("QuestsPanelHUD")).GetComponent<QuestsPanelHUDView>();
        }

        [Test]
        public void PopulateQuestProperly()
        {
            DataStore.i.Quests.quests[MOCK_QUEST_ID].name = "name";
            DataStore.i.Quests.quests[MOCK_QUEST_ID].description = "description";

            hudView.ShowQuestPopup(MOCK_QUEST_ID);

            Assert.AreEqual("name", hudView.questPopup.questName.text);
            Assert.AreEqual("description", hudView.questPopup.description.text);
        }

        [Test]
        public void PopulateSection()
        {
            DataStore.i.Quests.quests[MOCK_QUEST_ID].sections[0].name = "name";

            hudView.ShowQuestPopup(MOCK_QUEST_ID);

            Assert.AreEqual("name", hudView.questPopup.sections[0].sectionName.text);
        }

        [Test]
        [TestCase(false, true)]
        [TestCase(true, false)]
        public void PopulateTask_Single(bool isDone, bool hasCoordinates)
        {
            DataStore.i.Quests.quests[MOCK_QUEST_ID].sections[0].tasks[0].name = "name";
            DataStore.i.Quests.quests[MOCK_QUEST_ID].sections[0].tasks[0].type = "single";
            DataStore.i.Quests.quests[MOCK_QUEST_ID].sections[0].tasks[0].progress = isDone ? 1 : 0;
            DataStore.i.Quests.quests[MOCK_QUEST_ID].sections[0].tasks[0].coordinates = hasCoordinates ? "/goto 0,0" : null;
            DataStore.i.Quests.quests[MOCK_QUEST_ID].sections[0].tasks[0].payload = JsonUtility.ToJson(new TaskPayload_Single{isDone = isDone});

            hudView.ShowQuestPopup(MOCK_QUEST_ID);

            QuestsPanelTask_Single taskEntry = hudView.questPopup.sections[0].tasksContainer.GetComponentInChildren<QuestsPanelTask_Single>();

            Assert.NotNull(taskEntry);
            Assert.AreEqual("name", taskEntry.taskName.text);
            Assert.AreEqual(isDone, taskEntry.status.isOn);
            Assert.AreEqual(hasCoordinates, taskEntry.jumpInButton.gameObject.activeSelf);
        }

        [Test]
        [TestCase(0,1,2, true)]
        [TestCase(10,13,20, false)]
        public void PopulateTask_Numeric(int start, int current, int end, bool hasCoordinates)
        {
            DataStore.i.Quests.quests[MOCK_QUEST_ID].sections[0].tasks[0].name = "name";
            DataStore.i.Quests.quests[MOCK_QUEST_ID].sections[0].tasks[0].type = "numeric";
            DataStore.i.Quests.quests[MOCK_QUEST_ID].sections[0].tasks[0].progress = Mathf.InverseLerp(start, end, current);
            DataStore.i.Quests.quests[MOCK_QUEST_ID].sections[0].tasks[0].coordinates = hasCoordinates ? "/goto 0,0" : null;
            DataStore.i.Quests.quests[MOCK_QUEST_ID].sections[0].tasks[0].payload = JsonUtility.ToJson(new TaskPayload_Numeric
            {
                start = start,
                current = current,
                end = end
            });

            hudView.ShowQuestPopup(MOCK_QUEST_ID);

            QuestsPanelTask_Numeric taskEntry = hudView.questPopup.sections[0].tasksContainer.GetComponentInChildren<QuestsPanelTask_Numeric>();

            Assert.NotNull(taskEntry);
            Assert.AreEqual("name", taskEntry.taskName.text);
            Assert.AreEqual(start.ToString(), taskEntry.start.text);
            Assert.AreEqual(current.ToString(), taskEntry.current.text);
            Assert.AreEqual(end.ToString(), taskEntry.end.text);
            Assert.AreEqual(Mathf.InverseLerp(start, end, current), taskEntry.ongoingProgress.fillAmount);
            Assert.AreEqual(hasCoordinates, taskEntry.jumpInButton.gameObject.activeSelf);
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(hudView.gameObject);
        }
    }
}