using System.Collections;
using DCL;
using DCL.Huds.QuestsTracker;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.QuestsTrackerHUD
{
    public class QuestsTrackerHUDViewShould
    {
        private const string MOCK_QUEST_ID = "quest0";
        private const string MOCK_SECTION_ID = "section0";
        private const string MOCK_TASK_ID = "task0";

        private QuestsTrackerHUDView hudView;

        [SetUp]
        public void SetUp()
        {
            QuestModel questMock = new QuestModel
            {
                id = MOCK_QUEST_ID, name = "name", description = "description",
            };
            QuestSection sectionMock = new QuestSection
            {
                id = MOCK_SECTION_ID
            };
            QuestTask taskMock = new QuestTask
            {
                id = MOCK_TASK_ID,
                type = "single",
                payload = JsonUtility.ToJson(new TaskPayload_Single
                {
                    isDone = false
                })
            };
            sectionMock.tasks = new []
            {
                taskMock
            };
            questMock.sections = new []
            {
                sectionMock
            };
            DataStore.i.Quests.quests.Set(new []
            {
                (questMock.id, questMock)
            });

            QuestsTrackerHUDView.ENTRIES_PER_FRAME = int.MaxValue;

            hudView = Object.Instantiate(Resources.Load<GameObject>("QuestsTrackerHUD")).GetComponent<QuestsTrackerHUDView>();
        }

        [Test]
        public void BeEmptyOnInitialize() { Assert.AreEqual(0, hudView.currentEntries.Count); }

        [Test]
        public void TrackQuestProgress()
        {
            hudView.UpdateQuest(MOCK_QUEST_ID, true);

            Assert.AreEqual(1, hudView.currentEntries.Count);
            Assert.IsTrue(hudView.currentEntries.ContainsKey(MOCK_QUEST_ID));
        }

        [Test]
        public void AddPinnedQuestToTracker()
        {
            hudView.AddOrUpdateQuest(MOCK_QUEST_ID, true);

            Assert.AreEqual(1, hudView.questsContainer.childCount);
            Assert.IsTrue(hudView.currentEntries.ContainsKey(MOCK_QUEST_ID));
            Assert.IsTrue(hudView.currentEntries[MOCK_QUEST_ID].isPinned);
        }

        [Test]
        public void AddNotPinnedQuestToTracker()
        {
            hudView.AddOrUpdateQuest(MOCK_QUEST_ID, false);

            Assert.AreEqual(1, hudView.questsContainer.childCount);
            Assert.IsTrue(hudView.currentEntries.ContainsKey(MOCK_QUEST_ID));
            Assert.IsFalse(hudView.currentEntries[MOCK_QUEST_ID].isPinned);
        }

        [Test]
        public void PinUntrackedQuestProperly()
        {
            hudView.PinQuest(MOCK_QUEST_ID);
            Assert.IsTrue(hudView.currentEntries.ContainsKey(MOCK_QUEST_ID));
        }

        [Test]
        public void PinTrackedQuestProperly()
        {
            hudView.AddOrUpdateQuest(MOCK_QUEST_ID, false);

            hudView.PinQuest(MOCK_QUEST_ID);

            Assert.IsTrue(hudView.currentEntries[MOCK_QUEST_ID].isPinned);
        }

        [Test]
        public void UnpinTrackedQuestProperly()
        {
            hudView.AddOrUpdateQuest(MOCK_QUEST_ID, true);

            hudView.UnpinQuest(MOCK_QUEST_ID);

            Assert.IsFalse(hudView.currentEntries[MOCK_QUEST_ID].isPinned);
        }

        [UnityTest]
        public IEnumerator RemoveTrackedEntry()
        {
            hudView.AddOrUpdateQuest(MOCK_QUEST_ID, false);

            hudView.RemoveEntry(MOCK_QUEST_ID);

            yield return null; //Let Unity destroy the entry properly

            Assert.AreEqual(0, hudView.currentEntries.Count);
        }

        [UnityTest]
        public IEnumerator PopulateQuestEntryProperly_SingleTask()
        {
            DataStore.i.Quests.quests[MOCK_QUEST_ID].name = "questName";
            DataStore.i.Quests.quests[MOCK_QUEST_ID].sections = new QuestSection[2];
            for (int i = 0; i < 2; i++)
            {
                DataStore.i.Quests.quests[MOCK_QUEST_ID].sections[i] = new QuestSection()
                {
                    id = $"section{i}",
                    name = $"sectionName{i}",
                    progress = 1 - i, //So the first section is completed and the second one not
                    tasks = new []
                    {
                        new QuestTask
                        {
                            id = $"task{i}",
                            name = $"task{i}",
                            type = "single",
                            payload = JsonUtility.ToJson(new TaskPayload_Single { isDone = false })
                        }
                    }
                };
            }

            DataStore.i.Quests.quests[MOCK_QUEST_ID].name = "questName";
            hudView.AddOrUpdateQuest(MOCK_QUEST_ID, false);

            yield return null; //wait for all the instantiation/destruction of items to be done by unity

            Assert.AreEqual( "questName", hudView.currentEntries[MOCK_QUEST_ID].questTitle.text);
            Assert.AreEqual( 0, hudView.currentEntries[MOCK_QUEST_ID].progress.fillAmount);
            Assert.AreEqual( 2, hudView.currentEntries[MOCK_QUEST_ID].sectionContainer.childCount);
            Assert.AreEqual( 1, hudView.currentEntries[MOCK_QUEST_ID].sectionEntries["section0"].taskEntries.Count);

            var taskEntry = hudView.currentEntries[MOCK_QUEST_ID].sectionEntries["section0"].taskContainer.GetComponentInChildren<QuestsTrackerTask>();
            Assert.NotNull(taskEntry);
            Assert.AreEqual("task0" , taskEntry.taskTitle.text);
            Assert.AreEqual("0/1" , taskEntry.progressText.text);

            var taskEntry1 = hudView.currentEntries[MOCK_QUEST_ID].sectionEntries["section1"].taskContainer.GetComponentInChildren<QuestsTrackerTask>();
            Assert.NotNull(taskEntry1);
            Assert.AreEqual("task1" , taskEntry1.taskTitle.text);
            Assert.AreEqual("0/1" , taskEntry1.progressText.text);
        }

        [UnityTest]
        public IEnumerator PopulateQuestEntryProperly_NumericTask()
        {
            DataStore.i.Quests.quests[MOCK_QUEST_ID].name = "questName";
            DataStore.i.Quests.quests[MOCK_QUEST_ID].sections = new QuestSection[2];
            for (int i = 0; i < 2; i++)
            {
                DataStore.i.Quests.quests[MOCK_QUEST_ID].sections[i] = new QuestSection()
                {
                    id = $"section{i}",
                    name = $"sectionName{i}",
                    progress = 1 - i, //So the first section is completed and the second one not
                    tasks = new []
                    {
                        new QuestTask
                        {
                            id = $"task{i}",
                            name = $"task{i}",
                            type = "numeric",
                            payload = JsonUtility.ToJson(new TaskPayload_Numeric { start = 10, current = 15, end = 20 })
                        }
                    }
                };
            }

            DataStore.i.Quests.quests[MOCK_QUEST_ID].name = "questName";
            hudView.AddOrUpdateQuest(MOCK_QUEST_ID, false);

            yield return null; //wait for all the instantiation/destruction of items to be done by unity

            Assert.AreEqual( "questName", hudView.currentEntries[MOCK_QUEST_ID].questTitle.text);
            Assert.AreEqual( 0, hudView.currentEntries[MOCK_QUEST_ID].progress.fillAmount);
            Assert.AreEqual( 2, hudView.currentEntries[MOCK_QUEST_ID].sectionContainer.childCount);
            Assert.AreEqual( 1, hudView.currentEntries[MOCK_QUEST_ID].sectionEntries["section0"].taskEntries.Count);

            var taskEntry = hudView.currentEntries[MOCK_QUEST_ID].sectionEntries["section0"].taskContainer.GetComponentInChildren<QuestsTrackerTask>();
            Assert.NotNull(taskEntry);
            Assert.AreEqual("task0" , taskEntry.taskTitle.text);
            Assert.AreEqual("15/20" , taskEntry.progressText.text);

            var taskEntry1 = hudView.currentEntries[MOCK_QUEST_ID].sectionEntries["section1"].taskContainer.GetComponentInChildren<QuestsTrackerTask>();
            Assert.NotNull(taskEntry1);
            Assert.AreEqual("task1" , taskEntry1.taskTitle.text);
            Assert.AreEqual("15/20" , taskEntry1.progressText.text);
        }

        [UnityTest]
        public IEnumerator PopulateQuestEntryProperly_StepBased()
        {
            DataStore.i.Quests.quests[MOCK_QUEST_ID].name = "questName";
            DataStore.i.Quests.quests[MOCK_QUEST_ID].sections = new QuestSection[2];
            for (int i = 0; i < 2; i++)
            {
                DataStore.i.Quests.quests[MOCK_QUEST_ID].sections[i] = new QuestSection()
                {
                    id = $"section{i}",
                    name = $"sectionName{i}",
                    progress = 1 - i, //So the first section is completed and the second one not,
                    tasks = new []
                    {
                        new QuestTask
                        {
                            id = $"task{i}",
                            name = $"task{i}",
                            type = "step-based",
                            payload = JsonUtility.ToJson(new TaskPayload_Numeric { start = 10, current = 15, end = 20 })
                        }
                    }
                };
            }

            DataStore.i.Quests.quests[MOCK_QUEST_ID].name = "questName";
            hudView.AddOrUpdateQuest(MOCK_QUEST_ID, false);

            yield return null; //wait for all the instantiation/destruction of items to be done by unity

            Assert.AreEqual( "questName", hudView.currentEntries[MOCK_QUEST_ID].questTitle.text);
            Assert.AreEqual( 0, hudView.currentEntries[MOCK_QUEST_ID].progress.fillAmount);
            Assert.AreEqual( 2, hudView.currentEntries[MOCK_QUEST_ID].sectionContainer.childCount);
            Assert.AreEqual( 1, hudView.currentEntries[MOCK_QUEST_ID].sectionEntries["section0"].taskEntries.Count);

            var taskEntry = hudView.currentEntries[MOCK_QUEST_ID].sectionEntries["section0"].taskContainer.GetComponentInChildren<QuestsTrackerTask>();
            Assert.NotNull(taskEntry);
            Assert.AreEqual("task0" , taskEntry.taskTitle.text);
            Assert.AreEqual("15/20" , taskEntry.progressText.text);

            var taskEntry1 = hudView.currentEntries[MOCK_QUEST_ID].sectionEntries["section1"].taskContainer.GetComponentInChildren<QuestsTrackerTask>();
            Assert.NotNull(taskEntry1);
            Assert.AreEqual("task1" , taskEntry1.taskTitle.text);
            Assert.AreEqual("15/20" , taskEntry1.progressText.text);
        }
        [TearDown]
        public void TearDown() { Object.Destroy(hudView.gameObject); }
    }
}