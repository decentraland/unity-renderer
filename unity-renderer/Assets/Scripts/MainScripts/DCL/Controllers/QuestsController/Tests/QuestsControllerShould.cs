using System.Collections.Generic;
using DCL;
using DCL.QuestsController;
using NUnit.Framework;

namespace Tests.QuestsTrackerHUD
{
    public class QuestsControllerShould
    {
        private QuestsController questsController;
        private BaseDictionary<string, QuestModel> quests => DataStore.i.Quests.quests;
        private BaseCollection<string> pinnedQuests => DataStore.i.Quests.pinnedQuests;

        [SetUp]
        public void SetUp()
        {
            // This is needed because GenericAnalytics.SendAnalytics use Environment
            ServiceLocator serviceLocator = ServiceLocatorTestFactory.CreateMocked();
            Environment.Setup(serviceLocator);

            questsController = new QuestsController();
        }

        [Test]
        public void InitializeQuestsProperly()
        {
            questsController.InitializeQuests(new List<QuestModel>
            {
                new QuestModel { id = "0", status = QuestsLiterals.Status.NOT_STARTED, sections = new [] { new QuestSection { id = "0_0", tasks = new [] { new QuestTask { id = "0_0_0" } } } } },
                new QuestModel { id = "1", status = QuestsLiterals.Status.NOT_STARTED, sections = new [] { new QuestSection { id = "1_0", tasks = new [] { new QuestTask { id = "1_0_0" } } } } },
            });

            Assert.IsTrue(quests.ContainsKey("0"));
            Assert.IsTrue(quests.ContainsKey("1"));
        }

        [Test]
        public void InitializeQuestsFilterQuestsWithoutSections()
        {
            questsController.InitializeQuests(new List<QuestModel>
            {
                new QuestModel { id = "0", status = QuestsLiterals.Status.NOT_STARTED, sections = new QuestSection[] { } },
                new QuestModel { id = "1", status = QuestsLiterals.Status.NOT_STARTED, sections = null },
            });

            Assert.IsFalse(quests.ContainsKey("0"));
            Assert.IsFalse(quests.ContainsKey("1"));
        }

        [Test]
        public void InitializeQuestsUnpinNotPinnableQuests()
        {
            pinnedQuests.Add("0");
            pinnedQuests.Add("1");
            questsController.InitializeQuests(new List<QuestModel>
            {
                new QuestModel { id = "0", status = QuestsLiterals.Status.COMPLETED, sections = new [] { new QuestSection { id = "0_0", tasks = new [] { new QuestTask { id = "0_0_0" } } } } },
                new QuestModel { id = "1", status = QuestsLiterals.Status.BLOCKED, sections = new [] { new QuestSection { id = "1_0", tasks = new [] { new QuestTask { id = "1_0_0" } } } } },
            });

            Assert.IsFalse(pinnedQuests.Contains("0"));
            Assert.IsFalse(pinnedQuests.Contains("1"));
        }

        [Test]
        public void InitializeQuestsSetProgressFlags()
        {
            questsController.InitializeQuests(new List<QuestModel>
            {
                new QuestModel { id = "0", status = QuestsLiterals.Status.COMPLETED, sections = new [] { new QuestSection { id = "0_0", tasks = new [] { new QuestTask { id = "0_0_0" } } } } },
            });

            Assert.AreEqual(quests["0"].oldProgress, quests["0"].progress);
            Assert.AreEqual(quests["0"].sections[0].tasks[0].oldProgress, quests["0"].sections[0].tasks[0].progress);
            Assert.IsFalse(quests["0"].justProgressed);
            Assert.IsFalse(quests["0"].sections[0].tasks[0].justProgressed);
            Assert.IsFalse(quests["0"].sections[0].tasks[0].justUnlocked);
        }

        [Test]
        public void UpdateQuestProgressUnpinNotPinnableQuests()
        {
            pinnedQuests.Add("0");
            pinnedQuests.Add("1");
            questsController.UpdateQuestProgress(
                new QuestModel
                {
                    id = "0", status = QuestsLiterals.Status.COMPLETED, sections = new [] { new QuestSection { id = "0_0", tasks = new [] { new QuestTask { id = "0_0_0" } } } }
                });
            questsController.UpdateQuestProgress(
                new QuestModel
                {
                    id = "1", status = QuestsLiterals.Status.BLOCKED, sections = new [] { new QuestSection { id = "1_0", tasks = new [] { new QuestTask { id = "1_0_0" } } } }
                });

            Assert.IsFalse(pinnedQuests.Contains("0"));
            Assert.IsFalse(pinnedQuests.Contains("1"));
        }

        [Test]
        public void UpdateQuestProgressProcessNewQuest()
        {
            bool newQuestReceived = false;
            bool questUpdatedReceived = false;
            bool rewardObtainedReceived = false;
            questsController.OnNewQuest += (questId) => newQuestReceived = true;
            questsController.OnQuestUpdated += (questId, hasProgressed) => questUpdatedReceived = true;
            questsController.OnRewardObtained += (questId, rewardId) => rewardObtainedReceived = true;

            questsController.UpdateQuestProgress(new QuestModel { id = "0", status = QuestsLiterals.Status.NOT_STARTED, sections = new [] { new QuestSection { id = "0_0", tasks = new [] { new QuestTask { id = "0_0_0" } } } } });

            Assert.IsTrue(quests.ContainsKey("0"));
            Assert.AreEqual(quests["0"].oldProgress, quests["0"].progress);
            Assert.AreEqual(quests["0"].sections[0].tasks[0].oldProgress, quests["0"].sections[0].tasks[0].progress);
            Assert.IsFalse(quests["0"].justProgressed);
            Assert.IsFalse(quests["0"].sections[0].tasks[0].justProgressed);
            Assert.IsFalse(quests["0"].sections[0].tasks[0].justUnlocked);
            Assert.IsTrue(newQuestReceived);
            Assert.IsFalse(questUpdatedReceived);
            Assert.IsFalse(rewardObtainedReceived);
        }

        [Test]
        public void UpdateQuestProgressUpdateQuest_NewReward()
        {
            quests.Add("0", new QuestModel
            {
                id = "0", status = QuestsLiterals.Status.NOT_STARTED,
                sections = new [] { new QuestSection { id = "0_0", tasks = new [] { new QuestTask { id = "0_0_0" } } } },
            });

            QuestModel progressedQuest = new QuestModel
            {
                id = "0", status = QuestsLiterals.Status.NOT_STARTED,
                sections = new [] { new QuestSection { id = "0_0", tasks = new [] { new QuestTask { id = "0_0_0" } } } },
                rewards = new [] { new QuestReward { id = "reward0", status = QuestsLiterals.RewardStatus.NOT_GIVEN } }
            };

            bool newQuestReceived = false;
            bool questUpdatedReceived = false;
            bool rewardObtainedReceived = false;
            questsController.OnNewQuest += (questId) => newQuestReceived = true;
            questsController.OnQuestUpdated += (questId, hasProgressed) =>
            {
                questUpdatedReceived = true;
            };
            questsController.OnRewardObtained += (questId, rewardId) => rewardObtainedReceived = true;

            questsController.UpdateQuestProgress(progressedQuest);

            Assert.IsFalse(newQuestReceived);
            Assert.IsTrue(questUpdatedReceived);
            Assert.IsFalse(rewardObtainedReceived);

            //Check progress flags are reverted
            Assert.AreEqual(quests["0"].oldProgress, quests["0"].progress);
            Assert.AreEqual(quests["0"].sections[0].tasks[0].oldProgress, quests["0"].sections[0].tasks[0].progress);
            Assert.IsFalse(quests["0"].justProgressed);
            Assert.IsFalse(quests["0"].sections[0].tasks[0].justProgressed);
            Assert.IsFalse(quests["0"].sections[0].tasks[0].justUnlocked);
            Assert.AreEqual(1, quests["0"].rewards.Length);
            Assert.AreEqual("reward0", quests["0"].rewards[0].id);
            Assert.AreEqual(QuestsLiterals.RewardStatus.NOT_GIVEN, quests["0"].rewards[0].status);
        }

        [Test]
        public void UpdateQuestProgressUpdateQuest()
        {
            quests.Add("0", new QuestModel
            {
                id = "0", status = QuestsLiterals.Status.NOT_STARTED,
                sections = new [] { new QuestSection { id = "0_0", tasks = new [] { new QuestTask { id = "0_0_0" } } } },
                rewards = new [] { new QuestReward { id = "reward0", status = QuestsLiterals.RewardStatus.NOT_GIVEN } }
            });

            QuestModel progressedQuest = new QuestModel
            {
                id = "0", status = QuestsLiterals.Status.NOT_STARTED,
                sections = new []
                {
                    new QuestSection
                    {
                        id = "0_0",
                        progress = 0.5f,
                        tasks = new []
                        {
                            new QuestTask { id = "0_0_0", progress = 0.5f }
                        }
                    },
                },
                rewards = new [] { new QuestReward { id = "reward0", status = QuestsLiterals.RewardStatus.NOT_GIVEN } }
            };

            bool newQuestReceived = false;
            bool questUpdatedReceived = false;
            bool rewardObtainedReceived = false;
            questsController.OnNewQuest += (questId) => newQuestReceived = true;
            questsController.OnQuestUpdated += (questId, hasProgressed) =>
            {
                questUpdatedReceived = true;
                //Check the progress flags are set propperly
                Assert.AreEqual(0, quests["0"].oldProgress);
                Assert.AreEqual(0.5f, quests["0"].progress);
                Assert.AreEqual(0, quests["0"].sections[0].tasks[0].oldProgress);
                Assert.AreEqual(0.5f, quests["0"].sections[0].tasks[0].progress);
            };
            questsController.OnRewardObtained += (questId, rewardId) => rewardObtainedReceived = true;

            questsController.UpdateQuestProgress(progressedQuest);

            Assert.IsFalse(newQuestReceived);
            Assert.IsTrue(questUpdatedReceived);
            Assert.IsFalse(rewardObtainedReceived);

            //Check progress flags are reverted
            Assert.AreEqual(quests["0"].oldProgress, quests["0"].progress);
            Assert.AreEqual(quests["0"].sections[0].tasks[0].oldProgress, quests["0"].sections[0].tasks[0].progress);
            Assert.IsFalse(quests["0"].justProgressed);
            Assert.IsFalse(quests["0"].sections[0].tasks[0].justProgressed);
            Assert.IsFalse(quests["0"].sections[0].tasks[0].justUnlocked);
        }

        [Test]
        public void UpdateQuestProgressUpdateQuest_Completed()
        {
            quests.Add("0", new QuestModel
            {
                id = "0", status = QuestsLiterals.Status.NOT_STARTED,
                sections = new [] { new QuestSection { id = "0_0", tasks = new [] { new QuestTask { id = "0_0_0" } } } },
                rewards = new [] { new QuestReward { id = "reward0", status = QuestsLiterals.RewardStatus.NOT_GIVEN } }
            });

            QuestModel progressedQuest = new QuestModel
            {
                id = "0",
                status = QuestsLiterals.Status.COMPLETED,
                sections = new []
                {
                    new QuestSection
                    {
                        id = "0_0",
                        progress = 1f,
                        tasks = new []
                        {
                            new QuestTask { id = "0_0_0", progress = 1f, status = QuestsLiterals.Status.COMPLETED }
                        }
                    }
                },
                rewards = new [] { new QuestReward { id = "reward0", status = QuestsLiterals.RewardStatus.OK } }
            };

            bool newQuestReceived = false;
            bool questUpdatedReceived = false;
            bool rewardObtainedReceived = false;
            questsController.OnNewQuest += (questId) => newQuestReceived = true;
            questsController.OnQuestUpdated += (questId, hasProgressed) =>
            {
                questUpdatedReceived = true;
                //Check the progress flags are set propperly
                Assert.AreEqual(0, quests["0"].oldProgress);
                Assert.AreEqual(1f, quests["0"].progress);
                Assert.AreEqual(0, quests["0"].sections[0].tasks[0].oldProgress);
                Assert.AreEqual(1f, quests["0"].sections[0].tasks[0].progress);
            };
            questsController.OnRewardObtained += (questId, rewardId) =>
            {
                rewardObtainedReceived = true;
                // Check reward
                Assert.AreEqual(questId, "0");
                Assert.AreEqual(rewardId, "reward0");
            };

            questsController.UpdateQuestProgress(progressedQuest);

            Assert.IsFalse(newQuestReceived);
            Assert.IsTrue(questUpdatedReceived);
            Assert.IsTrue(rewardObtainedReceived);

            //Check progress flags are reverted
            Assert.AreEqual(quests["0"].oldProgress, quests["0"].progress);
            Assert.AreEqual(quests["0"].sections[0].tasks[0].oldProgress, quests["0"].sections[0].tasks[0].progress);
            Assert.IsFalse(quests["0"].justProgressed);
            Assert.IsFalse(quests["0"].sections[0].tasks[0].justProgressed);
            Assert.IsFalse(quests["0"].sections[0].tasks[0].justUnlocked);
        }

        [TearDown]
        public void TearDown()
        {
            Environment.Dispose();
            DataStore.Clear();
        }
    }
}