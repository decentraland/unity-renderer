using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCL.ExperiencesViewer.Tests
{
    public class ExperiencesViewerComponentViewTests
    {
        private ExperiencesViewerComponentView experiencesViewerComponent;

        [SetUp]
        public void SetUp()
        {
            experiencesViewerComponent = BaseComponentView.Create<ExperiencesViewerComponentView>("ExperiencesViewer");
        }

        [TearDown]
        public void TearDown()
        {
            experiencesViewerComponent.Dispose();
        }

        [Test]
        public void SetAvailableExperiencesCorrectly()
        {
            // Arrange
            List<ExperienceRowComponentModel> testExperiences = new List<ExperienceRowComponentModel>
        {
            new ExperienceRowComponentModel
            {
                backgroundColor = Color.white,
                iconUri = "",
                id = "testId1",
                isPlaying = true,
                isUIVisible = true,
                name = "Test Name 1",
                onHoverColor = Color.gray
            },
            new ExperienceRowComponentModel
            {
                backgroundColor = Color.white,
                iconUri = "",
                id = "testId2",
                isPlaying = false,
                isUIVisible = false,
                name = "Test Name 2",
                onHoverColor = Color.gray
            }
        };

            // Act
            experiencesViewerComponent.SetAvailableExperiences(testExperiences);

            // Assert
            Assert.AreEqual(2, experiencesViewerComponent.availableExperiences.instantiatedItems.Count, "The number of set experiences does not match.");
            Assert.IsTrue(experiencesViewerComponent.availableExperiences.instantiatedItems.Any(x => (x as ExperienceRowComponentView).model == testExperiences[0]), "The experience 1 is not contained in the experiences grid");
            Assert.IsTrue(experiencesViewerComponent.availableExperiences.instantiatedItems.Any(x => (x as ExperienceRowComponentView).model == testExperiences[1]), "The experience 2 is not contained in the experiences grid");
        }

        [Test]
        public void AddAvailableExperiencesCorrectly()
        {
            // Arrange
            SetAvailableExperiencesCorrectly();

            ExperienceRowComponentModel extraTestExperience = new ExperienceRowComponentModel
            {
                backgroundColor = Color.white,
                iconUri = "",
                id = "testId3",
                isPlaying = true,
                isUIVisible = false,
                name = "Test Name 3",
                onHoverColor = Color.gray
            };

            // Act
            experiencesViewerComponent.AddAvailableExperience(extraTestExperience);

            // Assert
            Assert.AreEqual(3, experiencesViewerComponent.availableExperiences.instantiatedItems.Count, "The number of set experiences does not match.");
            Assert.IsTrue(experiencesViewerComponent.availableExperiences.instantiatedItems.Any(x => (x as ExperienceRowComponentView).model == extraTestExperience), "The experience 3 is not contained in the experiences grid");
        }

        [Test]
        public void RemoveAvailableExperiencesCorrectly()
        {
            // Arrange
            SetAvailableExperiencesCorrectly();

            // Act
            experiencesViewerComponent.RemoveAvailableExperience("testId1");

            // Assert
            Assert.AreEqual(1, experiencesViewerComponent.availableExperiences.instantiatedItems.Count, "The number of set experiences does not match.");
            Assert.AreEqual("testId2", (experiencesViewerComponent.availableExperiences.instantiatedItems[0] as ExperienceRowComponentView).model.id);
        }

        [Test]
        public void GetAvailableExperienceByIdCorrectly()
        {
            // Arrange
            SetAvailableExperiencesCorrectly();

            // Act
            ExperienceRowComponentView receivedExperienceRowComponent = experiencesViewerComponent.GetAvailableExperienceById("testId2");

            // Assert
            Assert.AreEqual("testId2", receivedExperienceRowComponent.model.id);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetVisibleCorrectly(bool isVisible)
        {
            // Arrange
            experiencesViewerComponent.gameObject.SetActive(!isVisible);

            // Act
            experiencesViewerComponent.SetVisible(isVisible);

            // Assert
            Assert.AreEqual(isVisible, experiencesViewerComponent.gameObject.activeSelf);
        }

        [Test]
        public void ShowUIHiddenToastCorrectly()
        {
            // Act
            experiencesViewerComponent.ShowUiHiddenToast("pxName");

            // Assert
            Assert.IsTrue(experiencesViewerComponent.toastAnimator.isVisible);
        }

        [Test]
        public void RaiseOnSomeExperienceUIVisibilityChangedCorrectly()
        {
            // Arrange
            const string TEST_ID = "TestId";
            string receivedId = string.Empty;
            bool receivedUIVisibleFlag = false;
            experiencesViewerComponent.OnExperienceUiVisibilityChanged += (id, isUIVisible) =>
            {
                receivedId = id;
                receivedUIVisibleFlag = isUIVisible;
            };

            // Act
            experiencesViewerComponent.AddAvailableExperience(new ExperienceRowComponentModel
            {
                id = TEST_ID,
            });

            experiencesViewerComponent.availableExperiences
                                      .GetComponentInChildren<ExperienceRowComponentView>()
                                      .showPEXUIButton.onClick.Invoke();

            // Assert
            Assert.AreEqual(TEST_ID, receivedId);
            Assert.IsTrue(receivedUIVisibleFlag);
        }

        [Test]
        public void RaiseOnSomeExperienceExecutionChangedCorrectly()
        {
            // Arrange
            const string TEST_ID = "TestId";
            string receivedId = string.Empty;
            bool receivedPlayingFlag = false;
            experiencesViewerComponent.OnExperienceExecutionChanged += (id, isPlaying) =>
            {
                receivedId = id;
                receivedPlayingFlag = isPlaying;
            };

            // Act
            experiencesViewerComponent.AddAvailableExperience(new ExperienceRowComponentModel
            {
                id = TEST_ID,
            });

            experiencesViewerComponent.availableExperiences
                                      .GetComponentInChildren<ExperienceRowComponentView>()
                                      .startStopPEXToggle.isOn = true;

            // Assert
            Assert.AreEqual(TEST_ID, receivedId);
            Assert.IsTrue(receivedPlayingFlag);
        }
    }
}
