using DCL.Helpers;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace DCL.ExperiencesViewer.Tests
{
    public class ExperienceRowComponentViewTests
    {
        private ExperienceRowComponentView experienceRowComponent;

        [SetUp]
        public void SetUp()
        {
            experienceRowComponent = BaseComponentView.Create<ExperienceRowComponentView>("ExperienceRow");
            experienceRowComponent.iconImage.imageObserver = Substitute.For<ILazyTextureObserver>();
        }

        [TearDown]
        public void TearDown()
        {
            experienceRowComponent.Dispose();
        }

        [Test]
        public void ExperienceRowCorrectly()
        {
            // Arrange
            ExperienceRowComponentModel testModel = new ExperienceRowComponentModel
            {
                backgroundColor = Color.white,
                iconUri = "testUri",
                id = "testId",
                isPlaying = true,
                isUIVisible = true,
                name = "Test Name",
                onHoverColor = Color.black
            };

            // Act
            experienceRowComponent.Configure(testModel);

            // Assert
            Assert.AreEqual(testModel, experienceRowComponent.model, "The model does not match after configuring the experience row.");
        }

        [Test]
        public void RaiseOnFocusCorrectly()
        {
            // Arrange
            Color testColor = Color.green;
            experienceRowComponent.onHoverColor = testColor;

            // Act
            experienceRowComponent.OnFocus();

            // Assert
            Assert.AreEqual(testColor, experienceRowComponent.backgroundImage.color);
        }

        [Test]
        public void RaiseOnLoseFocusCorrectly()
        {
            // Arrange
            Color testColor = Color.green;
            experienceRowComponent.originalBackgroundColor = testColor;

            // Act
            experienceRowComponent.OnLoseFocus();

            // Assert
            Assert.AreEqual(testColor, experienceRowComponent.backgroundImage.color);
        }

        [Test]
        public void SetIdCorrectly()
        {
            // Arrange
            string testId = "TestId";
            experienceRowComponent.model.id = string.Empty;

            // Act
            experienceRowComponent.SetId(testId);

            // Assert
            Assert.AreEqual(testId, experienceRowComponent.model.id);
        }

        [Test]
        public void SetIconCorrectly()
        {
            // Arrange
            string testUri = "testUri";
            experienceRowComponent.model.iconUri = null;

            // Act
            experienceRowComponent.SetIcon(testUri);

            // Assert
            Assert.AreEqual(testUri, experienceRowComponent.model.iconUri, "The experience row icon uri does not match in the model.");
            experienceRowComponent.iconImage.imageObserver.Received().RefreshWithUri(testUri);
        }

        [Test]
        public void SetNameCorrectly()
        {
            // Arrange
            string testName = "Test text";

            // Act
            experienceRowComponent.SetName(testName);

            // Assert
            Assert.AreEqual(testName, experienceRowComponent.model.name, "The experience row name does not match in the model.");
            Assert.AreEqual(testName, experienceRowComponent.nameText.text);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetUIVisibilityCorrectly(bool isVisible)
        {
            // Arrange
            experienceRowComponent.model.isUIVisible = !isVisible;

            if (isVisible)
            {
                experienceRowComponent.showPEXUIButton.Show(true);
                experienceRowComponent.hidePEXUIButton.Hide(true);
            }
            else
            {
                experienceRowComponent.showPEXUIButton.Hide(true);
                experienceRowComponent.hidePEXUIButton.Show(true);
            }


            // Act
            experienceRowComponent.SetUIVisibility(isVisible);

            // Assert
            Assert.AreEqual(isVisible, experienceRowComponent.model.isUIVisible);
            Assert.AreEqual(!isVisible, experienceRowComponent.showPEXUIButton.isVisible);
            Assert.AreEqual(isVisible, experienceRowComponent.hidePEXUIButton.isVisible);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetAsPlayingCorrectly(bool isPlaying)
        {
            // Arrange
            experienceRowComponent.model.isPlaying = !isPlaying;
            if (isPlaying)
                experienceRowComponent.showHideUIButtonsContainerAnimator.Hide(true);
            else
                experienceRowComponent.showHideUIButtonsContainerAnimator.Show(true);

            // Act
            experienceRowComponent.SetAsPlaying(isPlaying);

            // Assert
            Assert.AreEqual(isPlaying, experienceRowComponent.model.isPlaying);
            Assert.AreEqual(isPlaying, experienceRowComponent.showHideUIButtonsContainerAnimator.isVisible);
        }

        [Test]
        public void SetRowColorCorrectly()
        {
            // Arrange
            Color testColor = Color.blue;

            // Act
            experienceRowComponent.SetRowColor(testColor);

            // Assert
            Assert.AreEqual(testColor, experienceRowComponent.model.backgroundColor, "The experience row background color does not match in the model.");
            Assert.AreEqual(testColor, experienceRowComponent.backgroundImage.color);
            Assert.AreEqual(testColor, experienceRowComponent.originalBackgroundColor);
        }

        [Test]
        public void SetOnHoverColorCorrectly()
        {
            // Arrange
            Color testColor = Color.blue;

            // Act
            experienceRowComponent.SetOnHoverColor(testColor);

            // Assert
            Assert.AreEqual(testColor, experienceRowComponent.model.onHoverColor, "The experience row on hover color does not match in the model.");
            Assert.AreEqual(testColor, experienceRowComponent.onHoverColor);
        }

        [Test]
        public void ClickOnShowPEXUIButtonCorrectly()
        {
            // Arrange
            string testId = "TestId";
            experienceRowComponent.model.id = testId;
            bool showPEXUIButtonClicked = false;
            string receivedId = string.Empty;
            bool receivedVisibleFlag = false;
            experienceRowComponent.onShowPEXUI += (id, isVisible) =>
            {
                showPEXUIButtonClicked = true;
                receivedId = id;
                receivedVisibleFlag = isVisible;
            };

            // Act
            experienceRowComponent.showPEXUIButton.onClick.Invoke();

            // Assert
            Assert.IsTrue(showPEXUIButtonClicked);
            Assert.AreEqual(testId, receivedId);
            Assert.IsTrue(receivedVisibleFlag);
        }

        [Test]
        public void ClickOnHidePEXUIButtonCorrectly()
        {
            // Arrange
            string testId = "TestId";
            experienceRowComponent.model.id = testId;
            bool hidePEXUIButtonClicked = false;
            string receivedId = string.Empty;
            bool receivedVisibleFlag = true;
            experienceRowComponent.onShowPEXUI += (id, isVisible) =>
            {
                hidePEXUIButtonClicked = true;
                receivedId = id;
                receivedVisibleFlag = isVisible;
            };

            // Act
            experienceRowComponent.hidePEXUIButton.onClick.Invoke();

            // Assert
            Assert.IsTrue(hidePEXUIButtonClicked);
            Assert.AreEqual(testId, receivedId);
            Assert.IsFalse(receivedVisibleFlag);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ClickOnStartStopPEXToggleCorrectly(bool isOn)
        {
            // Arrange
            experienceRowComponent.startStopPEXToggle.toggle.SetIsOnWithoutNotify(!isOn);
            string testId = "TestId";
            experienceRowComponent.model.id = testId;
            bool startStopPEXToggleClicked = false;
            string receivedId = string.Empty;
            bool receivedPlayingFlag = !isOn;
            experienceRowComponent.onStartPEX += (id, isPlaying) =>
            {
                startStopPEXToggleClicked = true;
                receivedId = id;
                receivedPlayingFlag = isPlaying;
            };

            // Act
            experienceRowComponent.startStopPEXToggle.toggle.onValueChanged.Invoke(isOn);

            // Assert
            Assert.IsTrue(startStopPEXToggleClicked);
            Assert.AreEqual(testId, receivedId);
            Assert.AreEqual(isOn, receivedPlayingFlag);
        }
    }
}