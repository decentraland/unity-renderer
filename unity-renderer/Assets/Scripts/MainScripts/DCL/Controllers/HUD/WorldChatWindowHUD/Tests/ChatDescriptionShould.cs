using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DCL.Chat.HUD
{
    
    public class ChatDescriptionShould
    {

        private ChatChannelComponentView chatChannelComponenetView;
        private ChatDescription chatDescription;

        [SetUp]
        public void SetUp()
        {
            chatChannelComponenetView = ChatChannelComponentView.Create();
            chatDescription = chatChannelComponenetView.Transform.GetComponentInChildren<ChatDescription>();
        }

        [UnityTest]
        public IEnumerator CheckParentingOnMessageLimitReached()
        {
            ChatEntryModel modelChat = new ChatEntryModel();
            for (int i = 0; i < 20; i++)
            {
                chatChannelComponenetView.chatView.AddEntry(modelChat);
            }
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            Assert.True(chatDescription.isParentedToChatContainer);
        }

        [UnityTest]
        public IEnumerator CheckRepositioningOnClearEntries()
        {
            chatChannelComponenetView.chatView.ClearAllEntries();
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            Assert.False(chatDescription.isParentedToChatContainer);
        }

    }
}