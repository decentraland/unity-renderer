using DCL.Social.Chat;
using NSubstitute;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DCL.Social.Chat
{
    public class PrivateChatEntryShould
    {
        private PrivateChatEntry view;
        private UserContextMenu userContextMenu;

        [SetUp]
        public void SetUp()
        {
            view = Object.Instantiate(
                AssetDatabase.LoadAssetAtPath<PrivateChatEntry>(
                    "Assets/Scripts/MainScripts/DCL/Controllers/HUD/SocialBarPrefabs/SocialBarV1/Prefabs/WhisperChannelElement.prefab"));

            userContextMenu = Substitute.ForPartsOf<UserContextMenu>();
            view.Initialize(Substitute.For<IChatController>(), userContextMenu, new DataStore_Mentions());
        }

        [TearDown]
        public void TearDown()
        {
            view.Dispose();
        }

        [TestCase(true, false)]
        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, true)]
        public void Configure(bool online, bool blocked)
        {
            var model = new PrivateChatEntryModel("userId", "name", "hello", "someUrl", blocked, online,
                0);
            model.imageFetchingEnabled = false;
            view.Configure(model);

            Assert.AreEqual("name", view.userNameLabel.text);
            Assert.AreEqual("hello", view.lastMessageLabel.text);
            Assert.AreEqual(blocked, view.blockedContainer.activeSelf);
            Assert.AreEqual(online && !blocked, view.onlineStatusContainer.activeSelf);
            Assert.AreEqual(!online && !blocked, view.offlineStatusContainer.activeSelf);
        }

        [Test]
        public void TriggerOpenChat()
        {
            var called = false;
            view.OnOpenChat += entry => called = true;

            view.openChatButton.onClick.Invoke();

            Assert.IsTrue(called);
        }

        [Test]
        public void EnableAvatarSnapshotFetching()
        {
            var picture = Substitute.ForPartsOf<ImageComponentView>();
            picture.WhenForAnyArgs(component => component.Configure(default)).DoNotCallBase();
            view.picture = picture;
            var model = new PrivateChatEntryModel("userId", "name", "hello", "someUrl", false, true, 0)
            {
                imageFetchingEnabled = false
            };
            view.Configure(model);

            view.EnableAvatarSnapshotFetching();

            picture.Received(1).Configure(Arg.Is<ImageComponentModel>(im => im.uri == "someUrl"));
        }

        [Test]
        public void DisableAvatarSnapshotFetching()
        {
            var picture = Substitute.ForPartsOf<ImageComponentView>();
            picture.WhenForAnyArgs(component => component.Configure(default)).DoNotCallBase();
            picture.WhenForAnyArgs(component => component.SetImage((string) default)).DoNotCallBase();
            view.picture = picture;
            var model = new PrivateChatEntryModel("userId", "name", "hello", "someUrl", false, true, 0)
            {
                imageFetchingEnabled = true
            };
            view.Configure(model);

            view.DisableAvatarSnapshotFetching();

            picture.Received(1).SetImage(Arg.Is<string>(s => string.IsNullOrEmpty(s)));
        }
    }
}
