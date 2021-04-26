using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class SectionSceneAdminsSettingsShould
    {
        private SectionSceneAdminsSettingsView view;
        private SectionSceneAdminsSettingsController controller;

        [SetUp]
        public void SetUp()
        {
            var prefab = Resources.Load<SectionSceneAdminsSettingsView>(SectionSceneAdminsSettingsController.VIEW_PREFAB_PATH);
            view = Object.Instantiate(prefab);
            controller = new SectionSceneAdminsSettingsController(view, new FriendsController_Mock());
        }

        [TearDown]
        public void TearDown() { controller.Dispose(); }

        [Test]
        public void HavePrefabSetupCorrectly()
        {
            Assert.AreEqual(1, view.adminsContainer.childCount);
            Assert.AreEqual(1, view.blockedContainer.childCount);
        }

        [Test]
        public void ShowEmptyListCorrectly()
        {
            view.SetAdminsEmptyList(true);
            view.SetBannedUsersEmptyList(true);

            Assert.IsTrue(view.adminsEmptyListContainer.activeSelf);
            Assert.IsTrue(view.blockedEmptyListContainer.activeSelf);
            Assert.IsFalse(view.adminsContainer.gameObject.activeSelf);
            Assert.IsFalse(view.blockedContainer.gameObject.activeSelf);
        }

        [Test]
        public void SetAdminCorrectly()
        {
            controller.SetAdmins(new [] { "1", "2", "3" });

            Assert.AreEqual(3, view.adminsElementViews.Count);
            Assert.AreEqual(3, GetVisibleChildrenAmount(view.adminsContainer));

            controller.SetAdmins(new [] { "1", "2" });
            Assert.AreEqual(2, view.adminsElementViews.Count);
            Assert.AreEqual(2, GetVisibleChildrenAmount(view.adminsContainer));
        }

        [Test]
        public void SetBannedUsersCorrectly()
        {
            controller.SetBannedUsers(new [] { "1", "2" });

            Assert.AreEqual(2, view.bannedUsersElementViews.Count);
            Assert.AreEqual(2, GetVisibleChildrenAmount(view.blockedContainer));

            controller.SetBannedUsers(new [] { "1", "2", "3", "4", "5" });
            Assert.AreEqual(5, view.bannedUsersElementViews.Count);
            Assert.AreEqual(5, GetVisibleChildrenAmount(view.blockedContainer));
        }

        [Test]
        public void TriggerUpdateAdminsCorrectly()
        {
            controller.SetAdmins(new [] { "1" });

            bool triggered = false;
            void UpdateAdmins(string sceneId, SceneAdminsUpdatePayload payload)
            {
                triggered = true;
            }

            controller.OnRequestUpdateSceneAdmins += UpdateAdmins;
            view.adminsElementViews["1"].removeButton.onClick.Invoke();
            Assert.IsTrue(triggered);
        }
        
        [Test]
        public void TriggerUpdateBannedUsersCorrectly()
        {
            controller.SetBannedUsers(new [] { "11" });

            bool triggered = false;
            void UpdateBanned(string sceneId, SceneBannedUsersUpdatePayload payload)
            {
                triggered = true;
            }

            controller.OnRequestUpdateSceneBannedUsers += UpdateBanned;
            view.bannedUsersElementViews["11"].removeButton.onClick.Invoke();
            Assert.IsTrue(triggered);
        }

        public int GetVisibleChildrenAmount(Transform parent) { return parent.Cast<Transform>().Count(child => child.gameObject.activeSelf); }
    }
}