using Cysharp.Threading.Tasks;
using DCL.Helpers;
using Decentraland.Quests;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using Task = System.Threading.Tasks.Task;

namespace DCLServices.QuestsService.Tests
{
    [Category("EditModeCI")]
    public class QuestsServiceShould
    {
        private QuestsService questsService;
        private IClientQuestsService client;
        private Channel<UserUpdate> channel; // to simulate the asyncEnumerable

        [SetUp]
        public void SetUp()
        {
            channel = Channel.CreateSingleConsumerUnbounded<UserUpdate>();
            client = Substitute.For<IClientQuestsService>();
            client.Configure().Subscribe(Arg.Any<UserAddress>()).Returns((x) => channel.Reader.ReadAllAsync());
            questsService = new QuestsService(client);
        }

        [TearDown]
        public void TearDown()
        {
            questsService?.Dispose();
        }

        [Test]
        public void InitializeWithNoUserSubscription()
        {
            var anyQuestUpdated = false;
            questsService.QuestUpdated.AddListener(_ => anyQuestUpdated = true);

            channel.Writer.TryWrite(new UserUpdate()
            {
                EventIgnored = (int)UserUpdate.MessageOneofCase.QuestStateUpdate,
                QuestStateUpdate = new QuestStateUpdate
                {
                    QuestData = new QuestStateWithData()
                    {
                        QuestInstanceId = "0",
                    },
                },
            });

            channel.Writer.TryWrite(new UserUpdate()
            {
                EventIgnored = (int)UserUpdate.MessageOneofCase.QuestStateUpdate,
                QuestStateUpdate = new QuestStateUpdate
                {
                    QuestData = new QuestStateWithData()
                    {
                        QuestInstanceId = "1",
                    },
                },
            });

            Assert.IsFalse(anyQuestUpdated);
            Assert.AreEqual(0, questsService.CurrentState.Count);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(10)]
        public void TriggerOnQuestUpdateEvents(int count)
        {
            int questsUpdated = 0;
            questsService.QuestUpdated.AddListener(_ => questsUpdated++);

            questsService.SetUserId("user");

            for (int i = 0; i < count; i++)
            {
                channel.Writer.TryWrite(new UserUpdate()
                {
                    EventIgnored = (int)UserUpdate.MessageOneofCase.QuestStateUpdate,
                    QuestStateUpdate = new QuestStateUpdate
                    {
                        QuestData = new QuestStateWithData()
                        {
                            QuestInstanceId = i.ToString()
                        },
                    },
                });
            }

            Assert.AreEqual(count, questsUpdated);
            Assert.AreEqual(count, questsService.CurrentState.Count);
        }

        [TestCase((uint)1)]
        [TestCase((uint)5)]
        [TestCase((uint)10)]
        public void UpdateCurrentStateIfSameQuestIsUpdated(uint updates)
        {
            QuestStateWithData latestUpdate = null;
            questsService.QuestUpdated.AddListener(x => { latestUpdate = x; });

            questsService.SetUserId("user");

            for (uint i = 0; i < updates; i++)
            {
                channel.Writer.TryWrite(new UserUpdate()
                {
                    EventIgnored = (int)UserUpdate.MessageOneofCase.QuestStateUpdate,
                    QuestStateUpdate = new QuestStateUpdate
                    {
                        QuestData = new QuestStateWithData()
                        {
                            QuestInstanceId = "quest_id",
                            QuestState = new QuestState
                            {
                                StepsLeft = (updates - 1) - i,
                            },
                        }
                    },
                });
            }

            Assert.AreEqual(0, latestUpdate.QuestState.StepsLeft);
            Assert.AreEqual(1, questsService.CurrentState.Count);
            Assert.AreEqual(latestUpdate, questsService.CurrentState["quest_id"]);
        }

        [Test]
        public void KeepCurrentStateUpdated()
        {
            var questStateUpdate = new QuestStateUpdate()
            {
                QuestData = new QuestStateWithData
                {
                    QuestInstanceId = "questInstanceId",
                }
            };

            questsService.SetUserId("user");
            channel.Writer.TryWrite(new UserUpdate
            {
                QuestStateUpdate = questStateUpdate,
            });

            Assert.AreEqual(1, questsService.CurrentState.Count);
            Assert.AreEqual(questStateUpdate.QuestData, questsService.CurrentState["questInstanceId"]);
        }

        [Test]
        public async Task StartQuest()
        {
            questsService.SetUserId("userId");
            await questsService.StartQuest("questId");

            client.Received().StartQuest(Arg.Is<StartQuestRequest>(s => s.QuestId == "questId" && s.UserAddress == "userId"));
        }

        [Test]
        public async Task ThrowWhenStartingQuestWithNoUserIdSet()
        {
            await TestUtils.ThrowsAsync<UserIdNotSetException>(questsService.StartQuest("questId"));
        }

        [Test]
        public async Task ThrowWhenAbortingQuestWithNoUserIdSet()
        {
            await TestUtils.ThrowsAsync<UserIdNotSetException>(questsService.AbortQuest("questId"));
        }

        [Test]
        public void StopReceivingUpdatesWhenDisposing()
        {
            questsService.SetUserId("userId");
            bool questUpdated = false;
            questsService.QuestUpdated.AddListener((x) => questUpdated = true);

            questsService.Dispose();

            channel.Writer.TryWrite(new UserUpdate()
            {
                QuestStateUpdate = new QuestStateUpdate()
                {
                    QuestData = new QuestStateWithData()
                }
            });

            Assert.IsFalse(questUpdated);
            Assert.AreEqual(0, questsService.stateCache.Count);
        }
    }
}
