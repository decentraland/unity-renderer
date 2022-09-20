using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Tests
{
    public class UsersSearchPromptShould
    {
        private UsersSearchPromptView promptView;

        [SetUp]
        public void SetUp()
        {
            var viewPrefab = Resources.Load<UsersSearchPromptView>("UsersSearchPrompt/UsersSearchPromptView");
            promptView = Object.Instantiate(viewPrefab);
        }

        [TearDown]
        public void TearDown() { Object.Destroy(promptView.gameObject); }

        [Test]
        public void HavePrefabSetupCorrectly() { Assert.AreEqual(1, promptView.friendListParent.childCount); }

        [Test]
        public void ShowFriendsCorrectly()
        {
            FriendsController_Mock friendsController = new FriendsController_Mock();
            friendsController.AddFriend(new UserStatus() { userId = "1", friendshipStatus = FriendshipStatus.FRIEND });
            friendsController.AddFriend(new UserStatus() { userId = "2", friendshipStatus = FriendshipStatus.FRIEND });
            friendsController.AddFriend(new UserStatus() { userId = "3", friendshipStatus = FriendshipStatus.FRIEND });

            var profile = ScriptableObject.CreateInstance<UserProfile>();
            profile.UpdateData(new UserProfileModel() { name = "Temp", userId = "1" });
            UserProfileController.userProfilesCatalog.Add(profile.userId, profile);
            profile = ScriptableObject.CreateInstance<UserProfile>();
            profile.UpdateData(new UserProfileModel() { name = "ta", userId = "2" });
            UserProfileController.userProfilesCatalog.Add(profile.userId, profile);
            profile = ScriptableObject.CreateInstance<UserProfile>();
            profile.UpdateData(new UserProfileModel() { name = "tion", userId = "3" });
            UserProfileController.userProfilesCatalog.Add(profile.userId, profile);

            FriendsSearchPromptController controller = new FriendsSearchPromptController(promptView, friendsController);
            controller.Show();

            Assert.AreEqual(3, promptView.friendListParent.childCount);
            Assert.AreEqual("Temp", controller.userViewsHandler.userElementViews["1"].textUserName.text);
            Assert.AreEqual("ta", controller.userViewsHandler.userElementViews["2"].textUserName.text);
            Assert.AreEqual("tion", controller.userViewsHandler.userElementViews["3"].textUserName.text);

            controller.Dispose();
        }

        [Test]
        public void SearchFriendsCorrectly()
        {
            FriendsController_Mock friendsController = new FriendsController_Mock();
            friendsController.AddFriend(new UserStatus() { userId = "1", friendshipStatus = FriendshipStatus.FRIEND });
            friendsController.AddFriend(new UserStatus() { userId = "2", friendshipStatus = FriendshipStatus.FRIEND });
            friendsController.AddFriend(new UserStatus() { userId = "3", friendshipStatus = FriendshipStatus.FRIEND });

            var profile = ScriptableObject.CreateInstance<UserProfile>();
            profile.UpdateData(new UserProfileModel() { name = "Temp", userId = "1" });
            UserProfileController.userProfilesCatalog.Add(profile.userId, profile);
            profile = ScriptableObject.CreateInstance<UserProfile>();
            profile.UpdateData(new UserProfileModel() { name = "ta", userId = "2" });
            UserProfileController.userProfilesCatalog.Add(profile.userId, profile);
            profile = ScriptableObject.CreateInstance<UserProfile>();
            profile.UpdateData(new UserProfileModel() { name = "tion", userId = "3" });
            UserProfileController.userProfilesCatalog.Add(profile.userId, profile);

            FriendsSearchPromptController controller = new FriendsSearchPromptController(promptView, friendsController);
            controller.Show();
            promptView.searchInputField.OnSubmit("Temp");

            Assert.IsTrue(controller.userViewsHandler.userElementViews["1"].gameObject.activeSelf);
            Assert.IsFalse(controller.userViewsHandler.userElementViews["2"].gameObject.activeSelf);
            Assert.IsFalse(controller.userViewsHandler.userElementViews["3"].gameObject.activeSelf);

            controller.Dispose();
        }

        [Test]
        public void ShowIfFriendsIsAddedToRolCorrectly()
        {
            FriendsController_Mock friendsController = new FriendsController_Mock();
            friendsController.AddFriend(new UserStatus() { userId = "1", friendshipStatus = FriendshipStatus.FRIEND });
            friendsController.AddFriend(new UserStatus() { userId = "2", friendshipStatus = FriendshipStatus.FRIEND });
            friendsController.AddFriend(new UserStatus() { userId = "3", friendshipStatus = FriendshipStatus.FRIEND });

            var profile = ScriptableObject.CreateInstance<UserProfile>();
            profile.UpdateData(new UserProfileModel() { name = "Temp", userId = "1" });
            UserProfileController.userProfilesCatalog.Add(profile.userId, profile);
            profile = ScriptableObject.CreateInstance<UserProfile>();
            profile.UpdateData(new UserProfileModel() { name = "ta", userId = "2" });
            UserProfileController.userProfilesCatalog.Add(profile.userId, profile);
            profile = ScriptableObject.CreateInstance<UserProfile>();
            profile.UpdateData(new UserProfileModel() { name = "tion", userId = "3" });
            UserProfileController.userProfilesCatalog.Add(profile.userId, profile);

            FriendsSearchPromptController controller = new FriendsSearchPromptController(promptView, friendsController);
            controller.Show();
            controller.SetUsersInRolList(new List<string>() { "1" });

            Assert.IsTrue(controller.userViewsHandler.userElementViews["1"].removeButton.gameObject.activeSelf);
            Assert.IsTrue(controller.userViewsHandler.userElementViews["2"].addButton.gameObject.activeSelf);
            Assert.IsTrue(controller.userViewsHandler.userElementViews["3"].addButton.gameObject.activeSelf);

            controller.Dispose();
        }

        [Test]
        public void TriggerAddButtonCorrectlyOnClick()
        {
            FriendsController_Mock friendsController = new FriendsController_Mock();
            friendsController.AddFriend(new UserStatus() { userId = "1", friendshipStatus = FriendshipStatus.FRIEND });

            var profile = ScriptableObject.CreateInstance<UserProfile>();
            profile.UpdateData(new UserProfileModel() { name = "Temptation", userId = "1" });
            UserProfileController.userProfilesCatalog.Add(profile.userId, profile);

            FriendsSearchPromptController controller = new FriendsSearchPromptController(promptView, friendsController);
            controller.Show();

            bool triggered = false;

            void OnButtonPressed(string id) { triggered = id == "1"; }

            controller.userViewsHandler.userElementViews["1"].OnAddPressed += OnButtonPressed;
            controller.userViewsHandler.userElementViews["1"].addButton.onClick.Invoke();

            Assert.IsTrue(triggered);
            controller.Dispose();
        }

        [Test]
        public void TriggerRemoveButtonCorrectlyOnClick()
        {
            FriendsController_Mock friendsController = new FriendsController_Mock();
            friendsController.AddFriend(new UserStatus() { userId = "1", friendshipStatus = FriendshipStatus.FRIEND });

            var profile = ScriptableObject.CreateInstance<UserProfile>();
            profile.UpdateData(new UserProfileModel() { name = "Temptation", userId = "1" });
            UserProfileController.userProfilesCatalog.Add(profile.userId, profile);

            FriendsSearchPromptController controller = new FriendsSearchPromptController(promptView, friendsController);
            controller.Show();

            bool triggered = false;

            void OnButtonPressed(string id) { triggered = id == "1"; }

            controller.userViewsHandler.userElementViews["1"].OnRemovePressed += OnButtonPressed;
            controller.userViewsHandler.userElementViews["1"].removeButton.onClick.Invoke();

            Assert.IsTrue(triggered);
            controller.Dispose();
        }

        [Test]
        public void SearchUserCorrectly()
        {
            UsersSearchPromptController controller = new UsersSearchPromptController(promptView);
            controller.Show();
            promptView.searchInputField.OnSubmit("Temp");
            controller.usersSearchPromise.Resolve(new []
            {
                new UserProfileModel() { userId = "Temp" },
                new UserProfileModel() { userId = "ta" },
                new UserProfileModel() { userId = "tion" },
            });

            Assert.AreEqual(3, promptView.friendListParent.childCount);
            Assert.IsTrue(controller.userViewsHandler.userElementViews["Temp"].gameObject.activeSelf);
            Assert.IsTrue(controller.userViewsHandler.userElementViews["ta"].gameObject.activeSelf);
            Assert.IsTrue(controller.userViewsHandler.userElementViews["tion"].gameObject.activeSelf);

            controller.Dispose();
        }
    }
}