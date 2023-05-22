using NSubstitute;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DCL.Chat.HUD
{
    public class CollapsableChatSearchListComponentViewShould
    {
        private CollapsableChatSearchListComponentView view;
        private WorldChatWindowComponentView conversationList;

        [SetUp]
        public void SetUp()
        {
            conversationList = Object.Instantiate(
                AssetDatabase.LoadAssetAtPath<WorldChatWindowComponentView>(
                    "Assets/Scripts/MainScripts/DCL/Controllers/HUD/SocialBarPrefabs/SocialBarV1/Addressables/ConversationListHUD.prefab"));

            view = conversationList.searchResultsList;
            view.Initialize(Substitute.For<IChatController>(), new DataStore_Mentions());
        }

        [TearDown]
        public void TearDown()
        {
            conversationList.Dispose();
        }

        [Test]
        public void DisableEmptyStateWhenSetPrivateChat()
        {
            GivenPrivateChatEntry();

            Assert.IsFalse(view.emptyStateContainer.activeSelf);
        }

        [Test]
        public void DisableEmptyStateWhenSetPublicChat()
        {
            GivenPublicChatEntry();

            Assert.IsFalse(view.emptyStateContainer.activeSelf);
        }

        [Test]
        public void EnableEmptyStateWhenIsCleared()
        {
            GivenPrivateChatEntry();
            GivenPublicChatEntry();

            view.Clear();

            Assert.IsTrue(view.emptyStateContainer.activeSelf);
        }

        [Test]
        public void EnableEmptyStateWhenEntryIsRemovedAndIsEmpty()
        {
            GivenPrivateChatEntry();

            view.Remove("usr");

            Assert.IsTrue(view.emptyStateContainer.activeSelf);
        }

        [Test]
        public void EnableEmptyStateWhenFilterMatchesNoElement()
        {
            GivenPrivateChatEntry();
            GivenPublicChatEntry();

            view.Filter(entry => false);

            Assert.IsTrue(view.emptyStateContainer.activeSelf);
        }

        [Test]
        public void DisableEmptyStateWhenFilterMatchesAnyElement()
        {
            GivenPrivateChatEntry();
            GivenPublicChatEntry();

            view.Filter(entry => true);

            Assert.IsFalse(view.emptyStateContainer.activeSelf);
        }

        private void GivenPublicChatEntry()
        {
            view.Set(new PublicChatEntryModel("chan", "chan", true, 2, false, false));
        }

        private void GivenPrivateChatEntry()
        {
            view.Set(new PrivateChatEntryModel("usr", "usr", "hey", "", false, false, 0));
        }
    }
}
