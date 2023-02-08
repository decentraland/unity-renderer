using System;
using System.Collections.Generic;
using System.Linq;
using DCL.Chat.Channels;
using DCL.Chat.WebApi;
using DCL.Interface;
using DCL.Social.Chat;
using NSubstitute;
using NUnit.Framework;

namespace DCL.Chat
{
    public class ChatControllerShould
    {
        private ChatController controller;
        private IChatApiBridge apiBridge;
        private DataStore dataStore;

        [SetUp]
        public void SetUp()
        {
            apiBridge = Substitute.For<IChatApiBridge>();
            dataStore = new DataStore();
            controller = new ChatController(apiBridge, dataStore);
            controller.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            controller.Dispose();
        }

        [Test]
        public void Initialize()
        {
            var called = false;
            controller.OnInitialized += () => called = true;

            apiBridge.OnInitialized += Raise.Event<Action<InitializeChatPayload>>(new InitializeChatPayload
            {
                totalUnseenMessages = 7
            });

            Assert.AreEqual(7, controller.TotalUnseenMessages);
            Assert.IsTrue(called);
        }

        [Test]
        public void AddMessage()
        {
            var totalUnseenMessagesCalled = 0;
            var messagesAddedCalled = false;

            controller.OnTotalUnseenMessagesUpdated += i => totalUnseenMessagesCalled = i;
            controller.OnAddMessage += messages => messagesAddedCalled = messages[0].messageId == "msg1"
                                                                         && messages[1].messageId == "msg2"
                                                                         && messages[2].messageId == "msg3";

            apiBridge.OnAddMessage += Raise.Event<Action<ChatMessage[]>>(new[]
            {
                new ChatMessage("msg1", ChatMessage.Type.PUBLIC, "sender", "hey", 100),
                new ChatMessage("msg2", ChatMessage.Type.PUBLIC, "sender", "bleh", 101),
                new ChatMessage("msg3", ChatMessage.Type.PRIVATE, "sender", "woah", 102)
                {
                    recipient = "me"
                }
            });

            Assert.AreEqual(2, controller.GetAllocatedUnseenChannelMessages(ChatUtils.NEARBY_CHANNEL_ID));
            Assert.AreEqual(2, totalUnseenMessagesCalled);
            Assert.IsTrue(messagesAddedCalled);
        }

        [TestCase(7)]
        [TestCase(4)]
        [TestCase(0)]
        public void UpdateTotalUnseenMessages(int expectedUnseenMessages)
        {
            var calledUnseenMessages = 0;
            controller.OnTotalUnseenMessagesUpdated += i => calledUnseenMessages = i;

            apiBridge.OnTotalUnseenMessagesChanged += Raise.Event<Action<UpdateTotalUnseenMessagesPayload>>(
                new UpdateTotalUnseenMessagesPayload
                {
                    total = expectedUnseenMessages
                });

            Assert.AreEqual(expectedUnseenMessages, controller.TotalUnseenMessages);
            Assert.AreEqual(expectedUnseenMessages, calledUnseenMessages);
        }

        [Test]
        public void UpdateUserTotalUnseenMessages()
        {
            var calls = new Dictionary<string, int>();
            controller.OnUserUnseenMessagesUpdated += (s, i) => calls[s] = i;

            apiBridge.OnUserUnseenMessagesChanged += Raise.Event<Action<(string userId, int count)[]>>(
                new[]
                {
                    ("usr1", 7),
                    ("usr2", 0),
                    ("usr3", 3),
                    ("usr4", 87)
                });

            Assert.AreEqual(7, controller.GetAllocatedUnseenMessages("usr1"));
            Assert.AreEqual(0, controller.GetAllocatedUnseenMessages("usr2"));
            Assert.AreEqual(3, controller.GetAllocatedUnseenMessages("usr3"));
            Assert.AreEqual(87, controller.GetAllocatedUnseenMessages("usr4"));
            Assert.AreEqual(7, calls["usr1"]);
            Assert.AreEqual(0, calls["usr2"]);
            Assert.AreEqual(3, calls["usr3"]);
            Assert.AreEqual(87, calls["usr4"]);
        }

        [Test]
        public void UpdateChannelTotalUnseenMessages()
        {
            var calls = new Dictionary<string, int>();
            controller.OnChannelUnseenMessagesUpdated += (s, i) => calls[s] = i;

            apiBridge.OnChannelUnseenMessagesChanged += Raise.Event<Action<(string channelId, int count)[]>>(
                new[]
                {
                    ("chn1", 6),
                    ("chn2", 2),
                    ("chn3", 102)
                });

            Assert.AreEqual(6, controller.GetAllocatedUnseenChannelMessages("chn1"));
            Assert.AreEqual(2, controller.GetAllocatedUnseenChannelMessages("chn2"));
            Assert.AreEqual(102, controller.GetAllocatedUnseenChannelMessages("chn3"));
            Assert.AreEqual(6, calls["chn1"]);
            Assert.AreEqual(2, calls["chn2"]);
            Assert.AreEqual(102, calls["chn3"]);
        }

        [Test]
        public void UpdateChannelMembers()
        {
            var called = false;
            controller.OnUpdateChannelMembers += (s, members) =>
            {
                called = s == "chn1"
                         && members[0].userId == "usr1"
                         && members[1].userId == "usr2";
            };

            apiBridge.OnChannelMembersUpdated += Raise.Event<Action<UpdateChannelMembersPayload>>(
                new UpdateChannelMembersPayload
                {
                    channelId = "chn1",
                    members = new[]
                    {
                        new ChannelMember
                        {
                            name = "woho",
                            isOnline = true,
                            userId = "usr1"
                        },
                        new ChannelMember
                        {
                            name = "hihi",
                            isOnline = false,
                            userId = "usr2"
                        },
                    }
                });

            Assert.IsTrue(called);
        }

        [Test]
        public void JoinChannel()
        {
            var calledJoinedChannels = new HashSet<string>();
            controller.OnChannelJoined += channel => calledJoinedChannels.Add(channel.ChannelId);
            var updatedChannels = new HashSet<string>();
            controller.OnChannelUpdated += channel => updatedChannels.Add(channel.ChannelId);

            apiBridge.OnChannelJoined += Raise.Event<Action<ChannelInfoPayloads>>(
                new ChannelInfoPayloads
                {
                    channelInfoPayload = new[]
                    {
                        new ChannelInfoPayload
                        {
                            description = "desc",
                            channelId = "chn1",
                            joined = true,
                            memberCount = 4,
                            muted = false,
                            name = "channel1",
                            unseenMessages = 6
                        },
                        new ChannelInfoPayload
                        {
                            description = "",
                            channelId = "chn2",
                            joined = true,
                            memberCount = 9,
                            muted = true,
                            name = "channel2",
                            unseenMessages = 3
                        }
                    }
                });

            var chn1 = controller.GetAllocatedChannel("chn1");
            var chn2 = controller.GetAllocatedChannel("chn2");
            Assert.AreEqual("chn1", chn1.ChannelId);
            Assert.AreEqual("desc", chn1.Description);
            Assert.AreEqual("channel1", chn1.Name);
            Assert.AreEqual(4, chn1.MemberCount);
            Assert.AreEqual(6, chn1.UnseenMessages);
            Assert.IsTrue(chn1.Joined);
            Assert.IsFalse(chn1.Muted);
            Assert.AreEqual("chn2", chn2.ChannelId);
            Assert.AreEqual("", chn2.Description);
            Assert.AreEqual("channel2", chn2.Name);
            Assert.AreEqual(9, chn2.MemberCount);
            Assert.AreEqual(3, chn2.UnseenMessages);
            Assert.IsTrue(chn2.Joined);
            Assert.IsTrue(chn2.Muted);
            Assert.AreEqual(2, calledJoinedChannels.Count);
            Assert.AreEqual("chn1", calledJoinedChannels.First());
            Assert.AreEqual("chn1", updatedChannels.First());
            Assert.AreEqual("chn2", updatedChannels.Skip(1).First());
        }

        [Test]
        public void JoinAutomaticChannel()
        {
            dataStore.HUDs.autoJoinChannelList.Set(new HashSet<string>(new[] {"chn1"}));
            var calledJoinedChannels = new HashSet<string>();
            controller.OnAutoChannelJoined += channel => calledJoinedChannels.Add(channel.ChannelId);
            var updatedChannels = new HashSet<string>();
            controller.OnChannelUpdated += channel => updatedChannels.Add(channel.ChannelId);

            apiBridge.OnChannelJoined += Raise.Event<Action<ChannelInfoPayloads>>(
                new ChannelInfoPayloads
                {
                    channelInfoPayload = new[]
                    {
                        new ChannelInfoPayload
                        {
                            description = "desc",
                            channelId = "chn1",
                            joined = true,
                            memberCount = 4,
                            muted = false,
                            name = "channel1",
                            unseenMessages = 6
                        }
                    }
                });

            var chn1 = controller.GetAllocatedChannel("chn1");
            Assert.AreEqual("chn1", chn1.ChannelId);
            Assert.AreEqual("desc", chn1.Description);
            Assert.AreEqual("channel1", chn1.Name);
            Assert.AreEqual(4, chn1.MemberCount);
            Assert.AreEqual(6, chn1.UnseenMessages);
            Assert.IsTrue(chn1.Joined);
            Assert.IsFalse(chn1.Muted);
            Assert.AreEqual(1, calledJoinedChannels.Count);
            Assert.AreEqual("chn1", calledJoinedChannels.First());
            Assert.AreEqual("chn1", updatedChannels.First());
        }

        [Test]
        public void FailToJoinChannel()
        {
            var called = false;
            controller.OnJoinChannelError +=
                (s, code) => called = s == "chn1" && code == ChannelErrorCode.LimitExceeded;

            apiBridge.OnChannelJoinFailed += Raise.Event<Action<JoinChannelErrorPayload>>(
                new JoinChannelErrorPayload
                {
                    channelId = "chn1",
                    errorCode = 1
                });

            Assert.IsTrue(called);
        }

        [Test]
        public void FailToLeaveChannel()
        {
            var called = false;
            controller.OnChannelLeaveError +=
                (s, code) => called = s == "chn1" && code == ChannelErrorCode.Unknown;

            apiBridge.OnChannelLeaveFailed += Raise.Event<Action<JoinChannelErrorPayload>>(
                new JoinChannelErrorPayload
                {
                    channelId = "chn1",
                    errorCode = 0
                });

            Assert.IsTrue(called);
        }

        [Test]
        public void UpdateNewChannel()
        {
            var updatedChannels = new HashSet<string>();
            controller.OnChannelUpdated += channel => updatedChannels.Add(channel.ChannelId);

            apiBridge.OnChannelsUpdated += Raise.Event<Action<ChannelInfoPayloads>>(
                new ChannelInfoPayloads
                {
                    channelInfoPayload = new[]
                    {
                        new ChannelInfoPayload
                        {
                            description = "desc",
                            channelId = "chn1",
                            joined = true,
                            memberCount = 4,
                            muted = false,
                            name = "channel1",
                            unseenMessages = 6
                        }
                    }
                });

            var chn1 = controller.GetAllocatedChannel("chn1");
            Assert.AreEqual("chn1", chn1.ChannelId);
            Assert.AreEqual("desc", chn1.Description);
            Assert.AreEqual("channel1", chn1.Name);
            Assert.AreEqual(4, chn1.MemberCount);
            Assert.AreEqual(6, chn1.UnseenMessages);
            Assert.IsTrue(chn1.Joined);
            Assert.IsFalse(chn1.Muted);
            Assert.AreEqual("chn1", updatedChannels.First());
        }

        [Test]
        public void OverrideChannelData()
        {
            apiBridge.OnChannelsUpdated += Raise.Event<Action<ChannelInfoPayloads>>(
                new ChannelInfoPayloads
                {
                    channelInfoPayload = new[]
                    {
                        new ChannelInfoPayload
                        {
                            description = "desc",
                            channelId = "chn1",
                            joined = true,
                            memberCount = 4,
                            muted = false,
                            name = "channel1",
                            unseenMessages = 6
                        }
                    }
                });

            apiBridge.OnChannelsUpdated += Raise.Event<Action<ChannelInfoPayloads>>(
                new ChannelInfoPayloads
                {
                    channelInfoPayload = new[]
                    {
                        new ChannelInfoPayload
                        {
                            description = "bleh",
                            channelId = "chn1",
                            joined = false,
                            memberCount = 2,
                            muted = true,
                            name = "channelWoah",
                            unseenMessages = 1
                        }
                    }
                });

            var chn1 = controller.GetAllocatedChannel("chn1");
            Assert.AreEqual("chn1", chn1.ChannelId);
            Assert.AreEqual("bleh", chn1.Description);
            Assert.AreEqual("channelWoah", chn1.Name);
            Assert.AreEqual(2, chn1.MemberCount);
            Assert.AreEqual(1, chn1.UnseenMessages);
            Assert.IsFalse(chn1.Joined);
            Assert.IsTrue(chn1.Muted);
        }

        [Test]
        public void FailMuteChannel()
        {
            var called = false;
            controller.OnMuteChannelError += (s, code) => called = s == "chn" && code == ChannelErrorCode.Unknown;

            apiBridge.OnMuteChannelFailed += Raise.Event<Action<MuteChannelErrorPayload>>(
                new MuteChannelErrorPayload
                {
                    channelId = "chn",
                    errorCode = 0
                });

            Assert.IsTrue(called);
        }

        [Test]
        public void UpdateChannelSearchResults()
        {
            var called = false;
            controller.OnChannelSearchResult += (s, channels) => called = s == "page1"
                                                                          && channels[0].ChannelId == "chn1" &&
                                                                          channels[1].ChannelId == "chn2";

            apiBridge.OnChannelSearchResults += Raise.Event<Action<ChannelSearchResultsPayload>>(
                new ChannelSearchResultsPayload
                {
                    since = "page1",
                    channels = new[]
                    {
                        new ChannelInfoPayload
                        {
                            description = "desc",
                            channelId = "chn1",
                            joined = true,
                            memberCount = 4,
                            muted = false,
                            name = "channel1",
                            unseenMessages = 6
                        },
                        new ChannelInfoPayload
                        {
                            description = "",
                            channelId = "chn2",
                            joined = true,
                            memberCount = 9,
                            muted = true,
                            name = "channel2",
                            unseenMessages = 3
                        }
                    }
                });

            Assert.IsTrue(called);
        }
    }
}
