using DCL.Huds.QuestsTracker;
using NUnit.Framework;
using UnityEngine;

namespace Tests.QuestsTrackerHUD
{
    public class QuestsTrackerEntryShould
    {
        private QuestsTrackerEntry questEntry;

        private readonly QuestTask task_noProgressed = new QuestTask
        {
            id = "task_no_progressed",
            progress = 0.5f,
            justProgressed = false,
            justUnlocked = false,
            status = QuestsLiterals.Status.ON_GOING,
            type = "single",
            payload = JsonUtility.ToJson(new TaskPayload_Single { isDone = false }),
            oldProgress = 0.5f,
        };

        private readonly QuestTask task_justProgressed = new QuestTask
        {
            id = "task_just_progressed",
            progress = 0.75f,
            justProgressed = true,
            justUnlocked = false,
            status = QuestsLiterals.Status.ON_GOING,
            type = "numeric",
            payload = JsonUtility.ToJson(new TaskPayload_Numeric() { start = 0, current = 2, end = 5 }),
            oldProgress = 0.5f,
        };

        private readonly QuestTask task_justUnlocked = new QuestTask
        {
            id = "task_just_unlocked",
            progress = 0.75f,
            justProgressed = false,
            justUnlocked = true,
            status = QuestsLiterals.Status.ON_GOING,
            type = "numeric",
            payload = JsonUtility.ToJson(new TaskPayload_Numeric() { start = 0, current = 2, end = 5 }),
            oldProgress = 0.5f,
        };

        private readonly QuestTask task_justCompleted = new QuestTask
        {
            id = "task_just_completed",
            progress = 1f,
            justProgressed = true,
            justUnlocked = false,
            status = QuestsLiterals.Status.ON_GOING,
            type = "numeric",
            payload = JsonUtility.ToJson(new TaskPayload_Numeric() { start = 0, current = 2, end = 5 }),
            oldProgress = 0.5f,
        };

        private readonly QuestTask task_completed = new QuestTask
        {
            id = "task_completed",
            progress = 1f,
            justProgressed = false,
            justUnlocked = false,
            status = QuestsLiterals.Status.ON_GOING,
            type = "numeric",
            payload = JsonUtility.ToJson(new TaskPayload_Numeric() { start = 0, current = 2, end = 5 }),
            oldProgress = 1f,
        };

        [SetUp]
        public void SetUp() { questEntry = Object.Instantiate(Resources.Load<GameObject>("QuestsTrackerEntry")).GetComponent<QuestsTrackerEntry>(); }

        [Test]
        public void PopulateProperly()
        {
            QuestModel quest = new QuestModel
            {
                id = "0",
                name = "quest0",
                description = "description0",
                oldProgress = 0.1f,
                status = QuestsLiterals.Status.ON_GOING,
                sections = new [] { new QuestSection { id = "section", progress = task_noProgressed.progress, tasks = new [] { task_noProgressed } }, },
            };
            questEntry.Populate(quest);

            Assert.AreEqual(quest.name, questEntry.questTitle.text);
            Assert.AreEqual(quest.oldProgress, questEntry.progress.fillAmount);
        }

        [Test]
        public void PopulateAndPrepareSectionsWith_NoProgressedTask()
        {
            QuestModel quest = new QuestModel
            {
                id = "0",
                name = "quest0",
                description = "description0",
                sections = new [] { new QuestSection { id = "section", progress = task_noProgressed.progress, tasks = new [] { task_noProgressed } }, }
            };
            questEntry.Populate(quest);

            //should be visible
            Assert.IsTrue(questEntry.sectionEntries["section"].gameObject.activeSelf);
        }

        [Test]
        public void PopulateAndPrepareSectionsWith_JustProgressedTask()
        {
            QuestModel quest = new QuestModel
            {
                id = "0",
                sections = new [] { new QuestSection { id = "section", progress = task_justProgressed.progress, tasks = new [] { task_justProgressed } }, }
            };
            questEntry.Populate(quest);

            //should be visible
            Assert.IsTrue(questEntry.sectionEntries["section"].gameObject.activeSelf);
        }

        [Test]
        public void PopulateAndPrepareSectionsWith_JustUnlockedTask()
        {
            QuestModel quest = new QuestModel
            {
                id = "0",
                sections = new [] { new QuestSection { id = "section", progress = task_justUnlocked.progress, tasks = new [] { task_justUnlocked } }, }
            };
            questEntry.Populate(quest);

            //should be invisible
            Assert.IsFalse(questEntry.sectionEntries["section"].gameObject.activeSelf);
        }

        [Test]
        public void PopulateAndPrepareSectionsWith_JustCompletedTask()
        {
            QuestModel quest = new QuestModel
            {
                id = "0",
                sections = new [] { new QuestSection { id = "section", progress = task_justCompleted.progress, tasks = new [] { task_justCompleted } }, }
            };
            questEntry.Populate(quest);

            //should be visible
            Assert.IsTrue(questEntry.sectionEntries["section"].gameObject.activeSelf);
        }

        [Test]
        public void PopulateAndPrepareSectionsWith_CompletedTask()
        {
            QuestModel quest = new QuestModel
            {
                id = "0",
                sections = new [] { new QuestSection { id = "section", progress = task_completed.progress, tasks = new [] { task_completed } }, }
            };
            questEntry.Populate(quest);

            //shouldn't exist
            Assert.IsFalse(questEntry.sectionEntries.ContainsKey("section"));
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetPinStatus(bool status)
        {
            questEntry.SetPinStatus(status);
            Assert.AreEqual(status, questEntry.pinQuestToggle.isOn);
        }

        [TearDown]
        public void TearDown() { Object.Destroy(questEntry.gameObject); }
    }

    public class QuestsTrackerSectionShould
    {
        private QuestsTrackerSection sectionEntry;

        private readonly QuestTask task_noProgressed = new QuestTask
        {
            id = "task_no_progressed",
            progress = 0.5f,
            justProgressed = false,
            justUnlocked = false,
            status = QuestsLiterals.Status.ON_GOING,
            type = "single",
            payload = JsonUtility.ToJson(new TaskPayload_Single { isDone = false }),
            oldProgress = 0.5f,
        };

        private readonly QuestTask task_justProgressed = new QuestTask
        {
            id = "task_just_progressed",
            progress = 0.75f,
            justProgressed = true,
            justUnlocked = false,
            status = QuestsLiterals.Status.ON_GOING,
            type = "numeric",
            payload = JsonUtility.ToJson(new TaskPayload_Numeric() { start = 0, current = 2, end = 5 }),
            oldProgress = 0.5f,
        };

        private readonly QuestTask task_justUnlocked = new QuestTask
        {
            id = "task_just_unlocked",
            progress = 0.75f,
            justProgressed = false,
            justUnlocked = true,
            status = QuestsLiterals.Status.ON_GOING,
            type = "numeric",
            payload = JsonUtility.ToJson(new TaskPayload_Numeric() { start = 0, current = 2, end = 5 }),
            oldProgress = 0.5f,
        };

        private readonly QuestTask task_justCompleted = new QuestTask
        {
            id = "task_just_completed",
            progress = 1f,
            justProgressed = true,
            justUnlocked = false,
            status = QuestsLiterals.Status.ON_GOING,
            type = "numeric",
            payload = JsonUtility.ToJson(new TaskPayload_Numeric() { start = 0, current = 2, end = 5 }),
            oldProgress = 0.5f,
        };

        private readonly QuestTask task_completed = new QuestTask
        {
            id = "task_completed",
            progress = 1f,
            justProgressed = false,
            justUnlocked = false,
            status = QuestsLiterals.Status.ON_GOING,
            type = "numeric",
            payload = JsonUtility.ToJson(new TaskPayload_Numeric() { start = 0, current = 2, end = 5 }),
            oldProgress = 1f,
        };

        [SetUp]
        public void SetUp() { sectionEntry = Object.Instantiate(Resources.Load<GameObject>("QuestsTrackerSection")).GetComponent<QuestsTrackerSection>(); }

        [Test]
        public void PopulateProperly()
        {
            QuestSection section =  new QuestSection { id = "section", name = "sectionName", tasks = new QuestTask[0] };
            sectionEntry.Populate(section);
            Assert.AreEqual(section.name, sectionEntry.sectionTitle.text);
            Assert.IsTrue(sectionEntry.titleContainer.gameObject.activeSelf);
        }

        [Test]
        public void PopulateProperly_NoSectionName()
        {
            QuestSection section =  new QuestSection { id = "section", name = "", tasks = new QuestTask[0] };
            sectionEntry.Populate(section);
            Assert.IsFalse(sectionEntry.titleContainer.gameObject.activeSelf);
        }

        [Test]
        public void PopulateAndPrepareTasks_NoProgressed()
        {
            QuestSection section =  new QuestSection { id = "section", name = "", tasks = new [] { task_noProgressed } };
            sectionEntry.Populate(section);

            //No completed tasks without progress are visible
            Assert.IsTrue(sectionEntry.taskEntries[task_noProgressed.id].gameObject.activeSelf);
        }

        [Test]
        public void PopulateAndPrepareTasks_Completed()
        {
            QuestSection section =  new QuestSection { id = "section", name = "", tasks = new [] { task_completed } };
            sectionEntry.Populate(section);

            //Completed tasks that has not been just completed are not even added
            Assert.IsFalse(sectionEntry.taskEntries.ContainsKey(task_completed.id));
        }

        [Test]
        public void PopulateAndPrepareTasks_JustCompleted()
        {
            QuestSection section =  new QuestSection { id = "section", name = "", tasks = new [] { task_justCompleted } };
            sectionEntry.Populate(section);

            //Completed tasks that has not been just completed are hidden
            Assert.IsTrue(sectionEntry.taskEntries[task_justCompleted.id].gameObject.activeSelf);
        }

        [Test]
        public void PopulateAndPrepareTasks_JustProgressed()
        {
            QuestSection section =  new QuestSection { id = "section", name = "", tasks = new [] { task_justProgressed } };
            sectionEntry.Populate(section);

            //Just progressed tasks are visible
            Assert.IsTrue(sectionEntry.taskEntries[task_justProgressed.id].gameObject.activeSelf);
        }

        [Test]
        public void PopulateAndPrepareTasks_JustUnlocked()
        {
            QuestSection section =  new QuestSection { id = "section", name = "", tasks = new [] { task_justUnlocked } };
            sectionEntry.Populate(section);

            //Just unlocked tasks are hidden
            Assert.IsFalse(sectionEntry.taskEntries[task_justUnlocked.id].gameObject.activeSelf);
        }
    }
}