using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DCL.Chat.HUD
{
    
    public class ChatDescriptionShould
    {

        private ChatChannelComponentView chatChannelComponentView;
        private ChatDescription chatDescription;
        private ChatHUDView chatHUDView;

        [SetUp]
        public void SetUp()
        {
            chatChannelComponentView = ChatChannelComponentView.Create();
            chatDescription = chatChannelComponentView.Transform.GetComponentInChildren<ChatDescription>();
            chatHUDView = chatChannelComponentView.Transform.GetComponentInChildren<ChatHUDView>();
        }

        [UnityTest]
        public IEnumerator CheckParentingOnMessageLimitReached()
        {
            ChatEntryModel modelChat = new ChatEntryModel();
            for (int i = 0; i < 20; i++)
            {
                chatHUDView.AddEntry(modelChat);
            }
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            Assert.True(chatDescription.isParentedToChatContainer);
            Assert.True(chatDescription.transform.IsChildOf(chatHUDView.chatEntriesContainer));
        }

        [UnityTest]
        public IEnumerator CheckRepositioningOnClearEntries()
        {
            ChatEntryModel modelChat = new ChatEntryModel();
            for (int i = 0; i < 20; i++)
            {
                chatHUDView.AddEntry(modelChat);
            }
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            chatHUDView.ClearAllEntries();
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            Assert.False(chatDescription.isParentedToChatContainer);
            Assert.False(chatDescription.transform.IsChildOf(chatHUDView.chatEntriesContainer));
            Assert.True(chatDescription.transform.IsChildOf(chatDescription.originalParent));
        }
        
        [TearDown]
        public void TearDown()
        {
            chatChannelComponentView.Dispose();
        }

    }
}