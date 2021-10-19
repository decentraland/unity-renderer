using DCL.Builder;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class SectionSceneGeneralSettingsViewShould
    {
        private SectionSceneGeneralSettingsView view;
        private SectionPlaceGeneralSettingsController controller;

        [SetUp]
        public void SetUp()
        {
            var prefab = Resources.Load<SectionSceneGeneralSettingsView>(SectionPlaceGeneralSettingsController.VIEW_PREFAB_PATH);
            view = Object.Instantiate(prefab);
            controller = new SectionPlaceGeneralSettingsController(view);
        }

        [TearDown]
        public void TearDown()
        {
            controller.Dispose();
            Object.Destroy(view.gameObject);
        }

        [Test]
        public void DisplayCorrectlyForProjects()
        {
            var sceneData = new PlaceData() { isDeployed = false };
            controller.SetSceneData(sceneData);

            Assert.IsFalse(view.configurationContainer.activeInHierarchy);
            Assert.IsFalse(view.permissionsContainer.activeInHierarchy);
        }

        [Test]
        public void DisplayCorrectlyForDeployedScenes()
        {
            var sceneData = new PlaceData() { isDeployed = true };
            controller.SetSceneData(sceneData);

            Assert.IsTrue(view.configurationContainer.activeInHierarchy);
            Assert.IsTrue(view.permissionsContainer.activeInHierarchy);
        }

        [Test]
        public void UpdateCorrectlySceneData()
        {
            var sceneData = new PlaceData()
            {
                isDeployed = true,
                name = "TheRealPravus",
                description = "LookingForTemptation",
                allowVoiceChat = false,
                isMatureContent = true,
                requiredPermissions = new [] { SectionPlaceGeneralSettingsController.PERMISSION_MOVE_PLAYER }
            };
            controller.SetSceneData(sceneData);

            Assert.AreEqual(sceneData.name, view.nameInputField.text);
            Assert.AreEqual(sceneData.description, view.descriptionInputField.text);
            Assert.AreEqual(sceneData.allowVoiceChat, view.toggleVoiceChat.isOn);
            Assert.AreEqual(sceneData.isMatureContent, view.toggleMatureContent.isOn);
            Assert.IsTrue(view.toggleMovePlayer.isOn);
            Assert.IsFalse(view.toggleEmotes.isOn);
        }
    }
}