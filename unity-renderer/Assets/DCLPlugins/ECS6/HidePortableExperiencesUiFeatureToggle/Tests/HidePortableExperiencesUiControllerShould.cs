using Cysharp.Threading.Tasks;
using DCL;
using DCL.Components;
using DCL.Controllers;
using DCL.Models;
using NSubstitute;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace DCLPlugins.ECS6.HidePortableExperiencesUiFeatureToggle
{
    public class HidePortableExperiencesUiControllerShould
    {
        private const int CURRENT_SCENE_NUMBER = 7;
        private const string CURRENT_SCENE_ID = "currentScene";

        private HidePortableExperiencesUiController controller;
        private BaseDictionary<int, bool> isSceneUiEnabled;
        private BaseHashSet<string> portableExperiencesIds;
        private IWorldState worldState;

        [SetUp]
        public void SetUp()
        {
            isSceneUiEnabled = new BaseDictionary<int, bool>();

            portableExperiencesIds = new BaseHashSet<string>();

            worldState = Substitute.For<IWorldState>();
            worldState.GetCurrentSceneNumber().Returns(CURRENT_SCENE_NUMBER);

            GivenScene(CURRENT_SCENE_NUMBER, CURRENT_SCENE_ID, ScenePortableExperienceFeatureToggles.Enable, false);

            controller = new HidePortableExperiencesUiController(worldState,
                isSceneUiEnabled,
                portableExperiencesIds);
        }

        [TearDown]
        public void TearDown()
        {
            controller.Dispose();
        }

        [UnityTest]
        public IEnumerator HidePortableExperienceUi() =>
            UniTask.ToCoroutine(async () =>
            {
                IParcelScene pxScene = GivenScene(9, "px9", ScenePortableExperienceFeatureToggles.Enable, true);
                Canvas canvas = await GivenCanvas(pxScene);

                isSceneUiEnabled.AddOrSet(9, false);

                Assert.IsFalse(canvas.enabled);
            });

        [UnityTest]
        public IEnumerator ShowPortableExperienceUi() =>
            UniTask.ToCoroutine(async () =>
            {
                IParcelScene pxScene = GivenScene(9, "px9", ScenePortableExperienceFeatureToggles.Enable, true);
                Canvas canvas = await GivenCanvas(pxScene);

                isSceneUiEnabled.AddOrSet(9, true);

                Assert.IsTrue(canvas.enabled);
            });

        [UnityTest]
        public IEnumerator KeepUiHiddenWhenSceneMustHidePxUi() =>
            UniTask.ToCoroutine(async () =>
            {
                GivenScene(CURRENT_SCENE_NUMBER, CURRENT_SCENE_ID, ScenePortableExperienceFeatureToggles.HideUi, false);
                IParcelScene pxScene = GivenScene(9, "px9", ScenePortableExperienceFeatureToggles.Enable, true);
                Canvas canvas = await GivenCanvas(pxScene);

                isSceneUiEnabled.AddOrSet(9, true);

                // wait until the visibility state is reset again
                await UniTask.NextFrame();
                await UniTask.NextFrame();

                Assert.IsFalse(canvas.enabled);
            });

        [UnityTest]
        public IEnumerator DisablePxVisibilityWhenIsAddedAndCurrentSceneMustHideUi() =>
            UniTask.ToCoroutine(async () =>
            {
                GivenScene(CURRENT_SCENE_NUMBER, CURRENT_SCENE_ID, ScenePortableExperienceFeatureToggles.HideUi, false);

                IParcelScene pxScene = GivenScene(9, "px9", ScenePortableExperienceFeatureToggles.Enable, true);
                Canvas canvas = await GivenCanvas(pxScene);

                portableExperiencesIds.Add("px9");

                // wait until the visibility state is reset
                await UniTask.NextFrame();
                await UniTask.NextFrame();

                Assert.IsFalse(canvas.enabled);
            });

        private async UniTask<Canvas> GivenCanvas(IParcelScene pxScene)
        {
            pxScene.GetSceneTransform().Returns(new GameObject("PxSceneTransform").transform);
            UIScreenSpace uiScreenSpace = new UIScreenSpace();
            uiScreenSpace.Initialize(pxScene, pxScene.sceneData.id);
            await uiScreenSpace.ApplyChanges(null).ToUniTask();
            IECSComponentsManagerLegacy ecsComponentsManagerLegacy = Substitute.For<IECSComponentsManagerLegacy>();
            pxScene.componentsManagerLegacy.Returns(ecsComponentsManagerLegacy);
            ecsComponentsManagerLegacy.GetSceneSharedComponent<UIScreenSpace>().Returns(uiScreenSpace);
            return uiScreenSpace.canvas;
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
