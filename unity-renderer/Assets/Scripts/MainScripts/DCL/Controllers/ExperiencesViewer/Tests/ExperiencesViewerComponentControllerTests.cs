using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using System.Collections.Generic;

namespace DCL.ExperiencesViewer.Tests
{
    public class ExperiencesViewerComponentControllerTests
    {
        private ExperiencesViewerComponentController experiencesViewerComponentController;
        private IExperiencesViewerComponentView experiencesViewerComponentView;
        private ISceneController sceneController;

        [SetUp]
        public void SetUp()
        {
            experiencesViewerComponentView = Substitute.For<IExperiencesViewerComponentView>();
            experiencesViewerComponentView.Configure().GetAllAvailableExperiences().Returns(info => new List<ExperienceRowComponentView>());
            sceneController = Substitute.For<ISceneController>();
            experiencesViewerComponentController = Substitute.ForPartsOf<ExperiencesViewerComponentController>();
            experiencesViewerComponentController.Configure().CreateView().Returns(info => experiencesViewerComponentView);
            experiencesViewerComponentController.Initialize(sceneController);
        }

        [TearDown]
        public void TearDown() { experiencesViewerComponentController.Dispose(); }

        [Test]
        public void InitializeCorrectly()
        {
            // Assert
            Assert.AreEqual(experiencesViewerComponentView, experiencesViewerComponentController.view);
            Assert.AreEqual(experiencesViewerComponentView.experienceViewerTransform, DataStore.i.experiencesViewer.isInitialized.Get());
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetVisibilityCorrectly(bool isVisible)
        {
            // Act
            experiencesViewerComponentController.SetVisibility(isVisible);

            // Assert
            experiencesViewerComponentView.Received().SetVisible(isVisible);
            Assert.AreEqual(isVisible, DataStore.i.experiencesViewer.isOpen.Get());
        }

        [Test]
        public void RaiseOnCloseButtonPressedCorrectly()
        {
            // Act
            experiencesViewerComponentController.OnCloseButtonPressed();

            // Assert
            experiencesViewerComponentView.Received().SetVisible(false);
            Assert.IsFalse(DataStore.i.experiencesViewer.isOpen.Get());
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void RaiseIsOpenChangedCorrectly(bool isOpen)
        {
            // Act
            experiencesViewerComponentController.ShowOrHide(isOpen, false);

            // Assert
            experiencesViewerComponentView.Received().SetVisible(isOpen);
            Assert.AreEqual(isOpen, DataStore.i.experiencesViewer.isOpen.Get());
        }

        // [Test]
        // public void CheckCurrentActivePortableExperiencesCorrectly()
        // {
        //     // Act
        //     experiencesViewerComponentController.CheckCurrentActivePortableExperiences();
        //
        //     // Assert
        //     Assert.AreEqual(experiencesViewerComponentController.activePEXScenes.Count, DataStore.i.experiencesViewer.numOfLoadedExperiences.Get());
        // }
    }
}
