using DCL.Controllers;
using DCL.Models;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCL.PortableExperiencesToggle
{
    public class PortableExperiencesToggleControllerShould
    {
        private const int CURRENT_SCENE_NUMBER = 3;
        private const string CURRENT_SCENE_ID = "currentScene";

        private PortableExperiencesToggleController controller;
        private IWorldState worldState;
        private IntVariable currentSceneVariable;
        private IPortableExperiencesBridge portableExperiencesBridge;
        private BaseHashSet<string> portableExperiencesIds;
        private BaseDictionary<string, (string name, string description, string icon)> disabledPortableExperiences;
        private BaseDictionary<int, bool> isSceneUiEnabled;

        [SetUp]
        public void SetUp()
        {
            worldState = Substitute.For<IWorldState>();
            GivenScene(CURRENT_SCENE_NUMBER, CURRENT_SCENE_ID, ScenePortableExperienceFeatureToggles.Enable, false);

            currentSceneVariable = ScriptableObject.CreateInstance<IntVariable>();

            portableExperiencesBridge = Substitute.For<IPortableExperiencesBridge>();

            portableExperiencesIds = new BaseHashSet<string>();

            disabledPortableExperiences = new BaseDictionary<string, (string name, string description, string icon)>();

            isSceneUiEnabled = new BaseDictionary<int, bool>();

            controller = new PortableExperiencesToggleController(currentSceneVariable,
                worldState,
                portableExperiencesBridge,
                portableExperiencesIds,
                disabledPortableExperiences,
                isSceneUiEnabled);
        }

        [TearDown]
        public void TearDown()
        {
            controller.Dispose();
        }

        [Test]
        public void DisableAllPxWhenTheCurrentSceneHasPxDisabled()
        {
            GivenScene(CURRENT_SCENE_NUMBER, CURRENT_SCENE_ID, ScenePortableExperienceFeatureToggles.Disable, false);
            GivenScene(6, "px6", ScenePortableExperienceFeatureToggles.Enable, true);
            GivenScene(7, "px7", ScenePortableExperienceFeatureToggles.Enable, true);

            portableExperiencesIds.Set(new[] { "px6", "px7" });

            currentSceneVariable.Set(CURRENT_SCENE_NUMBER);

            portableExperiencesBridge.Received(1).SetDisabledPortableExperiences(Arg.Is<IEnumerable<string>>(i =>
                i.ElementAt(0) == "px6"
                && i.ElementAt(1) == "px7"
                && i.Count() == 2));
        }

        [Test]
        public void HideAllUiWhenCurrentSceneHasPxUiDisable()
        {
            GivenScene(CURRENT_SCENE_NUMBER, CURRENT_SCENE_ID, ScenePortableExperienceFeatureToggles.HideUi, false);
            GivenScene(6, "px6", ScenePortableExperienceFeatureToggles.Enable, true);
            GivenScene(7, "px7", ScenePortableExperienceFeatureToggles.Enable, true);

            portableExperiencesIds.Set(new[] { "px6", "px7" });

            currentSceneVariable.Set(CURRENT_SCENE_NUMBER);

            CollectionAssert.AreEqual(new Dictionary<int, bool>
            {
                {6, false},
                {7, false},
            }, isSceneUiEnabled.Get());
        }

        [Test]
        public void DoNothingWhenCurrentSceneHasPxEnabled()
        {
            currentSceneVariable.Set(CURRENT_SCENE_NUMBER);

            portableExperiencesBridge.DidNotReceiveWithAnyArgs().SetDisabledPortableExperiences(default);
            CollectionAssert.AreEqual(new Dictionary<int, bool>(), isSceneUiEnabled.Get());
        }

        [Test]
        public void RestoreDisabledPortableExperiences()
        {
            GivenScene(CURRENT_SCENE_NUMBER, CURRENT_SCENE_ID, ScenePortableExperienceFeatureToggles.Disable, false);
            GivenScene(5, "px5", ScenePortableExperienceFeatureToggles.Enable, false);
            GivenScene(6, "px6", ScenePortableExperienceFeatureToggles.Enable, true);
            GivenScene(7, "px7", ScenePortableExperienceFeatureToggles.Enable, true);

            portableExperiencesIds.Set(new[] { "px6", "px7" });

            currentSceneVariable.Set(CURRENT_SCENE_NUMBER);
            portableExperiencesBridge.ClearReceivedCalls();
            currentSceneVariable.Set(5);

            portableExperiencesBridge.Received(1).SetDisabledPortableExperiences(
                Arg.Is<IEnumerable<string>>(i => !i.Any()));
        }

        [Test]
        public void RestorePortableExperienceVisibility()
        {
            GivenScene(CURRENT_SCENE_NUMBER, CURRENT_SCENE_ID, ScenePortableExperienceFeatureToggles.HideUi, false);
            GivenScene(5, "px5", ScenePortableExperienceFeatureToggles.Enable, false);
            GivenScene(6, "px6", ScenePortableExperienceFeatureToggles.Enable, true);
            GivenScene(7, "px7", ScenePortableExperienceFeatureToggles.Enable, true);

            portableExperiencesIds.Set(new[] { "px6", "px7" });

            currentSceneVariable.Set(CURRENT_SCENE_NUMBER);
            currentSceneVariable.Set(5);

            Assert.IsTrue(isSceneUiEnabled[6]);
            Assert.IsTrue(isSceneUiEnabled[7]);
            Assert.IsTrue(isSceneUiEnabled.Count() == 2);
        }

        private IParcelScene GivenScene(int number, string id, ScenePortableExperienceFeatureToggles pxFt, bool isPx)
        {
            IParcelScene scene = Substitute.For<IParcelScene>();

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene
            {
                sceneNumber = number,
                id = id,
                scenePortableExperienceFeatureToggles = pxFt,
            };

            scene.sceneData.Returns(sceneData);
            scene.isPortableExperience.Returns(isPx);
            worldState.GetScene(number).Returns(scene);

            if (isPx)
                worldState.GetPortableExperienceScene(id).Returns(scene);

            return scene;
        }
    }
}
