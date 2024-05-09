using DCL.Controllers;
using DCL.Models;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DCL.ExperiencesViewer.Tests
{
    public class ExperiencesViewerComponentControllerTests
    {
        private ExperiencesViewerController controller;
        private IExperiencesViewerComponentView view;
        private IWorldState worldState;
        private DataStore dataStore;
        private IPortableExperiencesBridge portableExperiencesBridge;

        [SetUp]
        public void SetUp()
        {
            view = Substitute.For<IExperiencesViewerComponentView>();
            view.GetAllAvailableExperiences().Returns(new List<ExperienceRowComponentView>());
            worldState = Substitute.For<IWorldState>();
            dataStore = new DataStore();
            portableExperiencesBridge = Substitute.For<IPortableExperiencesBridge>();
            controller = new ExperiencesViewerController(view, dataStore, worldState, portableExperiencesBridge);
            view.ClearReceivedCalls();
        }

        [TearDown]
        public void TearDown()
        {
            controller.Dispose();
        }

        [Test]
        public void InitializeCorrectly()
        {
            // Assert
            Assert.AreEqual(view.ExperienceViewerTransform, DataStore.i.experiencesViewer.isInitialized.Get());
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetVisibilityCorrectly(bool isVisible)
        {
            // Act
            dataStore.experiencesViewer.isOpen.Set(isVisible, true);

            // Assert
            view.Received(1).SetVisible(isVisible);
            Assert.AreEqual(isVisible, DataStore.i.experiencesViewer.isOpen.Get());
        }

        [Test]
        public void RaiseOnCloseButtonPressedCorrectly()
        {
            // Act
            view.OnCloseButtonPressed += Raise.Event<Action>();

            // Assert
            view.Received(1).SetVisible(false);
            Assert.IsFalse(DataStore.i.experiencesViewer.isOpen.Get());
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void RaiseIsOpenChangedCorrectly(bool isOpen)
        {
            // Act
            dataStore.experiencesViewer.isOpen.Set(isOpen, true);

            // Assert
            view.Received(1).SetVisible(isOpen);
            Assert.AreEqual(isOpen, DataStore.i.experiencesViewer.isOpen.Get());
        }

        [Test]
        public void HidePortableExperienceUiVisibility()
        {
            IParcelScene scene = Substitute.For<IParcelScene>();

            scene.sceneData.Returns(new LoadParcelScenesMessage.UnityParcelScene
            {
                sceneNumber = 7,
            });

            scene.GetSceneName().Returns("sceneName");
            worldState.GetPortableExperienceScene("pxId").Returns(scene);

            view.OnExperienceUiVisibilityChanged += Raise.Event<Action<string, bool>>("pxId", false);

            Assert.IsFalse(dataStore.HUDs.isSceneUiEnabled.Get(7));
            view.Received(1).ShowUiHiddenToast("sceneName");
        }

        [Test]
        public void ShowPortableExperienceUiVisibility()
        {
            IParcelScene scene = Substitute.For<IParcelScene>();

            scene.sceneData.Returns(new LoadParcelScenesMessage.UnityParcelScene
            {
                sceneNumber = 7,
            });

            scene.GetSceneName().Returns("sceneName");
            worldState.GetPortableExperienceScene("pxId").Returns(scene);

            view.OnExperienceUiVisibilityChanged += Raise.Event<Action<string, bool>>("pxId", true);

            Assert.IsTrue(dataStore.HUDs.isSceneUiEnabled.Get(7));
            view.Received(1).ShowUiShownToast("sceneName");
        }

        [Test]
        public void StartPortableExperienceExecution()
        {
            IParcelScene scene = Substitute.For<IParcelScene>();

            scene.sceneData.Returns(new LoadParcelScenesMessage.UnityParcelScene
            {
                sceneNumber = 7,
            });

            scene.GetSceneName().Returns("sceneName");
            worldState.GetPortableExperienceScene("pxId").Returns(scene);

            dataStore.world.disabledPortableExperienceIds.AddOrSet("pxId", ("sceneName", "desc", "icon"));
            dataStore.world.disabledPortableExperienceIds.AddOrSet("otherPxId", ("otherSceneName", "desc", "icon"));

            view.OnExperienceExecutionChanged += Raise.Event<Action<string, bool>>("pxId", true);

            Assert.AreEqual("pxId", dataStore.world.forcePortableExperience.Get());

            portableExperiencesBridge.Received(1).SetDisabledPortableExperiences(
                Arg.Is<IEnumerable<string>>(i => i.Count() == 1
                                                 && i.ElementAt(0) == "otherPxId"));

            view.Received(1).ShowEnabledToast("sceneName");
        }

        [Test]
        public void StopPortableExperienceExecution()
        {
            IParcelScene scene = Substitute.For<IParcelScene>();

            scene.sceneData.Returns(new LoadParcelScenesMessage.UnityParcelScene
            {
                sceneNumber = 7,
            });

            scene.GetSceneName().Returns("sceneName");
            worldState.GetPortableExperienceScene("pxId").Returns(scene);

            dataStore.world.disabledPortableExperienceIds.AddOrSet("otherPxId", ("otherSceneName", "desc", "icon"));

            view.OnExperienceExecutionChanged += Raise.Event<Action<string, bool>>("pxId", false);

            portableExperiencesBridge.Received(1).SetDisabledPortableExperiences(
                Arg.Is<IEnumerable<string>>(i => i.Count() == 2
                                                 && i.Contains("otherPxId")
                                                 && i.Contains("pxId")));

            view.Received(1).ShowDisabledToast("sceneName");
        }

        [Test]
        public void DisablePortableExperience()
        {
            view.GetAvailableExperienceById("pxId").Returns((ExperienceRowComponentView) null);

            dataStore.world.disabledPortableExperienceIds.AddOrSet("pxId", ("sceneName", "desc", "icon"));

            view.Received(1).AddAvailableExperience(Arg.Is<ExperienceRowComponentModel>(e =>
                e.id == "pxId"
                && !e.isPlaying
                && e.isUIVisible
                && e.name == "sceneName"
                && e.iconUri == "icon"
                && e.allowStartStop));
        }
    }
}
