using DCL.Chat.HUD;
using NUnit.Framework;
using System.Collections.Generic;

namespace DCL.Social.Chat
{
    public class ChatHUDViewShould
    {
        private ChatHUDView view;

        [SetUp]
        public void SetUp()
        {
            view = ChatHUDView.Create();
        }

        [TearDown]
        public void TearDown()
        {
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
    }
}
