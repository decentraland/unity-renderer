using DCL.Social.Chat;
using DCL.Interface;
using NSubstitute;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Tests;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace DCL.Social.Chat
{
    public class ChatHUDViewShould : IntegrationTestSuite
    {
        private ChatHUDView view;

        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();

            view = Object.Instantiate(
                AssetDatabase.LoadAssetAtPath<ChatHUDView>(
                    "Assets/Scripts/MainScripts/DCL/Controllers/HUD/SocialBarPrefabs/SocialBarV1/Prefabs/ChatHUD.prefab"));

            IWebRequestAsyncOperation webRequestAsyncOperation = Substitute.For<IWebRequestAsyncOperation>();
            Environment.i.platform.webRequest.GetTexture(default).ReturnsForAnyArgs(webRequestAsyncOperation);
        }

        protected override IEnumerator TearDown()
        {
            yield return base.TearDown();
            view.Dispose();
        }

        [Test]
        public void ShowMentionSuggestions()
        {
            view.ShowMentionSuggestions();
            view.SetMentionSuggestions(new List<ChatMentionSuggestionModel>
            {
                new ()
                {
                    userId = "u1",
                    imageUrl = "image1",
                    userName = "user1",
                },
                new ()
                {
                    userId = "u2",
                    imageUrl = "image2",
                    userName = "user2",
                }
            });

            Assert.IsTrue(view.chatMentionSuggestions.IsVisible);

            ChatMentionSuggestionEntryComponentView[] entries = view.chatMentionSuggestions.entryContainer
                                                                    .GetComponentsInChildren<ChatMentionSuggestionEntryComponentView>();

            Assert.AreEqual("user1", entries[0].userNameLabel.text);
            Assert.AreEqual("user2", entries[1].userNameLabel.text);
            Assert.AreEqual(2, entries.Length);
        }

        [Test]
        public void HideMentionSuggestions()
        {
            view.ShowMentionSuggestions();
            view.HideMentionSuggestions();

            Assert.IsFalse(view.chatMentionSuggestions.IsVisible);
        }

        [TestCase("im @us", 3, 3, "user1", "im @user1 ")]
        [TestCase("@", 0, 1, "superwow", "@superwow ")]
        public void AddMentionToInputField(string originalText, int fromIndex, int length, string userName, string expectedText)
        {
            view.SetInputFieldText(originalText);
            view.AddMentionToInputField(fromIndex, length, "u1", userName);

            Assert.AreEqual(expectedText, view.inputField.text);
        }

        [Test]
        public void TriggerSuggestionSubmitEventWhenButtonIsClicked()
        {
            var suggestionSelected = "";
            view.OnMentionSuggestionSelected += s => suggestionSelected = s;

            view.ShowMentionSuggestions();
            view.SetMentionSuggestions(new List<ChatMentionSuggestionModel>
            {
                new ()
                {
                    userId = "u1",
                    imageUrl = "image1",
                    userName = "user1",
                },
                new ()
                {
                    userId = "u2",
                    imageUrl = "image2",
                    userName = "user2",
                }
            });

            ChatMentionSuggestionEntryComponentView[] entries = view.chatMentionSuggestions.entryContainer
                                                                    .GetComponentsInChildren<ChatMentionSuggestionEntryComponentView>();

            entries[0].selectButton.onClick.Invoke();

            Assert.AreEqual("u1", suggestionSelected);
        }

        [UnityTest]
        public IEnumerator SortEntriesByTimestamp()
        {
            view.SortingStrategy = new ChatEntrySortingByTimestamp();

            var newMsg = new ChatEntryModel
            {
                timestamp = 200,
                bodyText = "new",
                isChannelMessage = false,
                messageId = "msg1",
                messageType = ChatMessage.Type.PRIVATE,
                recipientName = "recipient",
                senderName = "sender",
                senderId = "senderId",
                subType = ChatEntryModel.SubType.SENT,
            };

            var oldMsg = new ChatEntryModel
            {
                timestamp = 100,
                bodyText = "old",
                isChannelMessage = false,
                messageId = "msg2",
                messageType = ChatMessage.Type.PRIVATE,
                recipientName = "recipient",
                senderName = "sender",
                senderId = "senderId",
                subType = ChatEntryModel.SubType.RECEIVED,
            };

            view.SetEntry(newMsg);
            view.SetEntry(oldMsg);

            yield return null;

            ChatEntry chatEntry = view.chatEntriesContainer.GetChild(0).GetComponent<ChatEntry>();
            Assert.AreEqual(oldMsg, chatEntry.Model);

            chatEntry = view.chatEntriesContainer.GetChild(1).GetComponent<ChatEntry>();
            Assert.AreEqual(newMsg, chatEntry.Model);
        }

        [UnityTest]
        public IEnumerator DontSortEntries()
        {
            view.SortingStrategy = new ChatEntrySortingSequentially();

            var msg1 = new ChatEntryModel
            {
                timestamp = 100,
                bodyText = "msg1",
                isChannelMessage = false,
                messageId = "msg1",
                messageType = ChatMessage.Type.PUBLIC,
                senderName = "sender",
                senderId = "senderId",
                subType = ChatEntryModel.SubType.SENT,
            };

            var msg2 = new ChatEntryModel
            {
                timestamp = 200,
                bodyText = "msg2",
                isChannelMessage = false,
                messageId = "msg2",
                messageType = ChatMessage.Type.PUBLIC,
                senderName = "sender",
                senderId = "senderId",
                subType = ChatEntryModel.SubType.RECEIVED,
            };

            var msg3 = new ChatEntryModel
            {
                timestamp = 300,
                bodyText = "msg3",
                isChannelMessage = false,
                messageId = "msg3",
                messageType = ChatMessage.Type.PUBLIC,
                senderName = "sender",
                senderId = "senderId",
                subType = ChatEntryModel.SubType.SENT,
            };

            view.SetEntry(msg1);
            view.SetEntry(msg2);
            view.SetEntry(msg3);

            yield return null;

            ChatEntry chatEntry = view.chatEntriesContainer.GetChild(0).GetComponent<ChatEntry>();
            Assert.AreEqual(msg1, chatEntry.Model);

            chatEntry = view.chatEntriesContainer.GetChild(1).GetComponent<ChatEntry>();
            Assert.AreEqual(msg2, chatEntry.Model);

            chatEntry = view.chatEntriesContainer.GetChild(2).GetComponent<ChatEntry>();
            Assert.AreEqual(msg3, chatEntry.Model);
        }
    }
}
