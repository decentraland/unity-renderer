using Cysharp.Threading.Tasks;
using DCL.Helpers;
using Decentraland.Quests;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
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
            client.Configure().Subscribe(Arg.Any<Empty>()).Returns((x) => channel.Reader.ReadAllAsync());

            client.Configure()
                  .GetAllQuests(Arg.Any<Empty>())
                  .Returns((x) =>
                       UniTask.FromResult(new GetAllQuestsResponse()
                       {
                           Quests = new Quests()
                           {
                               Instances = { },
                           },
                       }));

            questsService = new QuestsService(client, Substitute.For<IQuestRewardsResolver>());

            channel.Writer.TryWrite(new UserUpdate()
            {
                Subscribed = true,
            });
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
                QuestStateUpdate = new QuestStateUpdate
                {
                    QuestState = new QuestState(),
                    InstanceId = "0",
                },
            });

            channel.Writer.TryWrite(new UserUpdate()
            {
                QuestStateUpdate = new QuestStateUpdate
                {
                    QuestState = new QuestState(),
                    InstanceId = "0",
                },
            });

            Assert.IsFalse(anyQuestUpdated);
            Assert.AreEqual(0, questsService.QuestInstances.Count);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(10)]
        public void TriggerOnQuestUpdateEvents(int count)
        {
            for (int i = 0; i < count; i++)
            {
                questsService.questInstances.Add(i.ToString(), new QuestInstance()
                {
                    Id = i.ToString(),
                    State = new QuestState(),
                    Quest = new Quest { Id = $"questId{i}", },
                });
            }

            int questsUpdated = 0;
            questsService.QuestUpdated.AddListener(_ => questsUpdated++);

            for (int i = 0; i < count; i++)
            {
                channel.Writer.TryWrite(new UserUpdate()
                {
                    QuestStateUpdate = new QuestStateUpdate
                    {
                        QuestState = new QuestState(),
                        InstanceId = i.ToString(),
                    },
                });
            }

            Assert.AreEqual(count, questsUpdated);
            Assert.AreEqual(count, questsService.QuestInstances.Count);
        }

        [TestCase((uint)1)]
        [TestCase((uint)5)]
        [TestCase((uint)10)]
        public void UpdateCurrentStateIfSameQuestIsUpdated(uint updates)
        {
            questsService.questInstances.Add("questInstanceId", new QuestInstance()
            {
                Id = "questInstanceId",
                State = new QuestState()
                {
                    StepsLeft = updates
                },
                Quest = new Quest { Id = $"questId" },
            });

            QuestInstance latestUpdate = null;
            questsService.QuestUpdated.AddListener(x => { latestUpdate = x; });

            for (uint i = 0; i < updates; i++)
            {
                channel.Writer.TryWrite(new UserUpdate()
                {
                    QuestStateUpdate = new QuestStateUpdate
                    {
                        InstanceId = "questInstanceId",
                        QuestState = new QuestState()
                        {
                            StepsLeft = (updates - 1) - i,
                        },
                    },
                });
            }

            Assert.AreEqual(0, latestUpdate.State.StepsLeft);
            Assert.AreEqual(1, questsService.QuestInstances.Count);
            Assert.AreEqual(latestUpdate, questsService.QuestInstances["questInstanceId"]);
        }

        [Test]
        public void KeepCurrentStateUpdated()
        {
            questsService.questInstances.Add("questInstanceId", new QuestInstance()
            {
                Id = "questInstanceId",
                State = new QuestState(),
                Quest = new Quest { Id = "questId", },
            });

            var questStateUpdate = new QuestStateUpdate()
            {
                QuestState = new QuestState(),
                InstanceId = "questInstanceId",
            };

            channel.Writer.TryWrite(new UserUpdate
            {
                QuestStateUpdate = questStateUpdate,
            });

            Assert.AreEqual(1, questsService.QuestInstances.Count);
            Assert.AreEqual(questStateUpdate.QuestState, questsService.QuestInstances["questInstanceId"].State);
        }

        [Test]
        public async Task StartQuest()
        {
            await questsService.StartQuest("questId");

            client.Received().StartQuest(Arg.Is<StartQuestRequest>(s => s.QuestId == "questId"));
        }

        [Test]
        public async Task RemoveQuestInstanceWhenAbortQuestSuccess()
        {
            questsService.questInstances.Add("questInstance", new QuestInstance()
            {
                Id = "questInstance",
                Quest = new Quest(),
                State = new QuestState()
            });
            client.AbortQuest(Arg.Any<AbortQuestRequest>())
                  .Returns(
                       new UniTask<AbortQuestResponse>(
                           new AbortQuestResponse()
                           {
                               Accepted = new AbortQuestResponse.Types.Accepted(),
                           }
                       ));

            await questsService.AbortQuest("questInstance");

            client.Received().AbortQuest(Arg.Is<AbortQuestRequest>(a => a.QuestInstanceId == "questInstance"));
            Assert.AreEqual(0, questsService.QuestInstances.Count);
        }

        [Test]
        public async Task DontRemoveQuestInstanceWhenAbortQuestFailed()
        {
            questsService.questInstances.Add("questInstance", new QuestInstance()
            {
                Id = "questInstance",
                Quest = new Quest(),
                State = new QuestState()
            });

            client.AbortQuest(Arg.Any<AbortQuestRequest>())
                  .Returns(
                       new UniTask<AbortQuestResponse>(
                           new AbortQuestResponse()
                           {
                               NotFoundQuestInstance = new NotFoundQuestInstance(),
                           }
                       ));

            await questsService.AbortQuest("questInstance");
            client.AbortQuest(Arg.Any<AbortQuestRequest>())
                  .Returns(
                       new UniTask<AbortQuestResponse>(
                           new AbortQuestResponse()
                           {
                               InternalServerError = new InternalServerError()
                           }
                       ));

            await questsService.AbortQuest("questInstance");

            Assert.AreEqual(1, questsService.QuestInstances.Count);
        }

        [Test]
        public void StopReceivingUpdatesWhenDisposing()
        {
            bool questUpdated = false;

            var questInstance = new QuestInstance()
            {
                Id = "questInstanceId",
                State = new QuestState(),
                Quest = new Quest { Id = "questId", },
            };

            questsService.questInstances.Add("questInstanceId", questInstance);
            questsService.QuestUpdated.AddListener((x) => questUpdated = true);

            questsService.Dispose();

            channel.Writer.TryWrite(new UserUpdate()
            {
                QuestStateUpdate = new QuestStateUpdate()
                {
                    QuestState = new QuestState(),
                    InstanceId = "questInstanceId",
                },
            });

            Assert.IsFalse(questUpdated);
            Assert.AreEqual(questInstance.State, questsService.questInstances["questInstanceId"].State);
        }
    }
}
