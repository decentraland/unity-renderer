using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class SectionSceneContributorsSettingsShould
    {
        private SectionSceneContributorsSettingsView view;
        private SectionSceneContributorsSettingsController controller;

        [SetUp]
        public void SetUp()
        {
            var prefab = Resources.Load<SectionSceneContributorsSettingsView>(SectionSceneContributorsSettingsController.VIEW_PREFAB_PATH);
            view = Object.Instantiate(prefab);
            controller = new SectionSceneContributorsSettingsController(view, new FriendsController_Mock());
        }

        [TearDown]
        public void TearDown() { controller.Dispose(); }

        [Test]
        public void HavePrefabSetupCorrectly() { Assert.AreEqual(1, view.usersContainer.childCount); }

        [Test]
        public void ShowEmptyListCorrectly()
        {
            view.SetEmptyList(true);

            Assert.IsTrue(view.emptyListContainer.activeSelf);
            Assert.IsFalse(view.usersContainer.gameObject.activeSelf);
        }

        [Test]
        public void UpdateContributorsCorrectly()
        {
            controller.UpdateContributors(new [] { "1", "2", "3" });

            Assert.AreEqual(3, view.userElementViews.Count);
            Assert.AreEqual(3, GetVisibleChildrenAmount(view.usersContainer));

            controller.UpdateContributors(new [] { "1", "2" });

            Assert.AreEqual(2, view.userElementViews.Count);
            Assert.AreEqual(2, GetVisibleChildrenAmount(view.usersContainer));
        }

        [Test]
        public void TriggerUpdateContributorsCorrectly()
        {
            controller.UpdateContributors(new [] { "1" });

            bool triggered = false;
            void UpdateContributors(string sceneId, SceneContributorsUpdatePayload payload) { triggered = true; }

            controller.OnRequestUpdateSceneContributors += UpdateContributors;
            view.userElementViews["1"].removeButton.onClick.Invoke();
            Assert.IsTrue(triggered);
        }

        public int GetVisibleChildrenAmount(Transform parent) { return parent.Cast<Transform>().Count(child => child.gameObject.activeSelf); }
    }
}