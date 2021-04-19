using System.Collections.Generic;
using DCL;
using DCL.Huds.QuestsTracker;
using DCL.QuestsController;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Tests.QuestsTrackerHUD
{
    public class QuestsTrackerHUDControllerShould
    {
        private const string MOCK_QUEST_ID = "quest0";
        private const string MOCK_SECTION_ID = "section0";
        private const string MOCK_TASK_ID = "task0";

        private IQuestsController questsController;
        private QuestsTrackerHUDController hudController;
        private IQuestsTrackerHUDView hudView;

        [SetUp]
        public void SetUp()
        {
            QuestModel questMock = new QuestModel { id = MOCK_QUEST_ID };
            QuestSection sectionMock = new QuestSection { id = MOCK_SECTION_ID };
            QuestTask taskMock = new QuestTask { id = MOCK_TASK_ID };
            sectionMock.tasks = new [] { taskMock };
            questMock.sections = new [] { sectionMock };
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
            questsController.OnQuestProgressed += Raise.Event<QuestProgressed>(MOCK_QUEST_ID);

            hudView.Received().UpdateQuest(MOCK_QUEST_ID);
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

        [TearDown]
        public void TearDown() { DataStore.Clear(); }
    }

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
        public void BeEmptyOnInitialize()
        {
            Assert.AreEqual(0, hudView.currentEntries.Count);
            Assert.AreEqual(0, hudView.questsToBeAdded.Count);
        }

        [Test]
        public void TrackQuestProgress()
        {
            hudView.UpdateQuest(MOCK_QUEST_ID);

            Assert.AreEqual(1, hudView.questsToBeAdded.Count);
            Assert.IsTrue(hudView.questsToBeAdded.Contains(MOCK_QUEST_ID));
            Assert.AreEqual(0, hudView.currentEntries.Count);
        }

        [Test]
        public void AddPinnedQuestToTracker()
        {
            hudView.AddOrUpdateQuest(MOCK_QUEST_ID, true);

            Assert.AreEqual(1, hudView.questsContainer.childCount);
            Assert.IsTrue(hudView.currentEntries.ContainsKey(MOCK_QUEST_ID));
            Assert.AreEqual(DateTime.MaxValue, hudView.lastUpdateTimestamp[MOCK_QUEST_ID]);
        }

        [Test]
        public void AddNotPinnedQuestToTracker()
        {
            hudView.AddOrUpdateQuest(MOCK_QUEST_ID, false);

            Assert.AreEqual(1, hudView.questsContainer.childCount);
            Assert.IsTrue(hudView.currentEntries.ContainsKey(MOCK_QUEST_ID));
            Assert.Less((DateTime.Now - hudView.lastUpdateTimestamp[MOCK_QUEST_ID]), TimeSpan.FromSeconds(1)); //Approximately comparing timestamps
        }

        [Test]
        public void PinUntrackedQuestProperly()
        {
            hudView.PinQuest(MOCK_QUEST_ID);

            Assert.AreEqual(1, hudView.questsToBeAdded.Count);
            Assert.IsTrue(hudView.questsToBeAdded.Contains(MOCK_QUEST_ID));
            Assert.AreEqual(0, hudView.currentEntries.Count);
        }

        [Test]
        public void PinTrackedQuestProperly()
        {
            hudView.AddOrUpdateQuest(MOCK_QUEST_ID, false);

            hudView.PinQuest(MOCK_QUEST_ID);

            Assert.AreEqual(DateTime.MaxValue, hudView.lastUpdateTimestamp[MOCK_QUEST_ID]);
        }

        [Test]
        public void UnpinTrackedQuestProperly()
        {
            hudView.AddOrUpdateQuest(MOCK_QUEST_ID, true);

            hudView.UnpinQuest(MOCK_QUEST_ID);

            Assert.AreEqual(DateTime.MinValue, hudView.lastUpdateTimestamp[MOCK_QUEST_ID]); //Approximately comparing timestamps
        }

        [UnityTest]
        public IEnumerator RemoveTrackedEntry()
        {
            hudView.AddOrUpdateQuest(MOCK_QUEST_ID, false);

            hudView.RemoveEntry(MOCK_QUEST_ID);

            yield return null; //Let Unity destroy the entry properly

            Assert.AreEqual(0, hudView.currentEntries.Count);
            Assert.AreEqual(0, hudView.questsToBeAdded.Count);
            Assert.AreEqual(0, hudView.lastUpdateTimestamp.Count);
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
            Assert.AreEqual( $"sectionName1 - {0:0.0}%", hudView.currentEntries[MOCK_QUEST_ID].sectionTitle.text);
            Assert.AreEqual( 0, hudView.currentEntries[MOCK_QUEST_ID].progress.fillAmount);
            Assert.AreEqual( 1, hudView.currentEntries[MOCK_QUEST_ID].tasksContainer.childCount);

            var taskEntry = hudView.currentEntries[MOCK_QUEST_ID].tasksContainer.GetComponentInChildren<QuestsTrackerTask>();
            Assert.NotNull(taskEntry);
            Assert.AreEqual("task1" , taskEntry.taskTitle.text);
            Assert.AreEqual("0/1" , taskEntry.progressText.text);
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
            Assert.AreEqual( $"sectionName1 - {0:0.0}%", hudView.currentEntries[MOCK_QUEST_ID].sectionTitle.text);
            Assert.AreEqual( 0, hudView.currentEntries[MOCK_QUEST_ID].progress.fillAmount);
            Assert.AreEqual( 1, hudView.currentEntries[MOCK_QUEST_ID].tasksContainer.childCount);

            var taskEntry = hudView.currentEntries[MOCK_QUEST_ID].tasksContainer.GetComponentInChildren<QuestsTrackerTask>();
            Assert.NotNull(taskEntry);
            Assert.AreEqual("task1" , taskEntry.taskTitle.text);
            Assert.AreEqual("15/20" , taskEntry.progressText.text);
        }

        [TearDown]
        public void TearDown() { Object.Destroy(hudView.gameObject); }
    }
}