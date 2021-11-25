using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.BuildModeHUDViews
{
    public class SceneCatalogViewShould
    {
        private SceneCatalogView sceneCatalogView;

        [SetUp]
        public void SetUp() { sceneCatalogView = SceneCatalogView.Create(); }

        [TearDown]
        public void TearDown() { Object.Destroy(sceneCatalogView.gameObject); }

        [Test]
        public void HideCatalogClickCorrectly()
        {
            // Arrange
            bool hideCatalogClicked = false;
            sceneCatalogView.OnHideCatalogClicked += () => hideCatalogClicked = true;

            // Act
            sceneCatalogView.OnHideCatalogClick();

            // Assert
            Assert.IsTrue(hideCatalogClicked, "The hide catalog event has not been called!");
        }

        [Test]
        public void GoBackCorrectly()
        {
            // Arrange
            bool backClicked = false;
            sceneCatalogView.OnSceneCatalogBack += () => backClicked = true;

            // Act
            sceneCatalogView.Back();

            // Assert
            Assert.IsTrue(backClicked, "The back event has not been called!");
        }

        [Test]
        public void SetCatalogTitleCorrectly()
        {
            // Arrange
            string oldText = "Old text";
            string newText = "New text";
            sceneCatalogView.catalogTitleTxt.text = oldText;

            // Act
            sceneCatalogView.SetCatalogTitle(newText);

            // Assert
            Assert.AreEqual(newText, sceneCatalogView.catalogTitleTxt.text, "The catalog title does not match!");
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void CheckIfCatalogIsOpenCorrectly(bool isOpen)
        {
            // Arrange
            sceneCatalogView.gameObject.SetActive(isOpen);

            // Act
            bool isCatalogOpen = sceneCatalogView.IsCatalogOpen();

            // Assert
            Assert.AreEqual(sceneCatalogView.gameObject.activeSelf, isCatalogOpen, "The catalog activation property does not match!");
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void CheckIfCatalogIsExpandedCorrectly(bool isExpanded)
        {
            // Arrange
            sceneCatalogView.isCatalogExpanded = isExpanded;

            // Act
            bool isCatalogExpanded = sceneCatalogView.IsCatalogExpanded();

            // Assert
            Assert.AreEqual(sceneCatalogView.isCatalogExpanded, isCatalogExpanded, "The catalog expanded property does not match!");
        }

        [UnityTest]
        public IEnumerator CloseCatalogCorrectly()
        {
            // Arrange
            sceneCatalogView.gameObject.SetActive(true);

            // Act
            sceneCatalogView.CloseCatalog();

            // Assert
            Assert.IsTrue(sceneCatalogView.isClosing, "The isClosing flag should be activated!");
            yield return null;
            Assert.IsFalse(sceneCatalogView.gameObject.activeSelf, "The catalog is not deactivated!");
            Assert.IsFalse(sceneCatalogView.isClosing, "The isClosing flag should be deactivated!");
        }

        [UnityTest]
        [TestCase(true, true, ExpectedResult = null)]
        [TestCase(true, false, ExpectedResult = null)]
        [TestCase(false, true, ExpectedResult = null)]
        [TestCase(false, false, ExpectedResult = null)]
        public IEnumerator SetActiveCorrectly(bool isActive, bool isClosing)
        {
            // Arrange
            sceneCatalogView.isClosing = isClosing;
            sceneCatalogView.gameObject.SetActive(!isActive);

            // Act
            sceneCatalogView.SetActive(isActive);

            // Assert
            if (isActive && isClosing)
            {
                Assert.AreEqual(!isActive, sceneCatalogView.gameObject.activeSelf);
                sceneCatalogView.isClosing = false;
                yield return null;
            }

            Assert.AreEqual(isActive, sceneCatalogView.gameObject.activeSelf, "The catalog has not been activated properly!");

            yield return null;
            Assert.IsTrue(true);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ShowBackButton(bool isActive)
        {
            // Arrange
            sceneCatalogView.backgBtn.gameObject.SetActive(!isActive);

            // Act
            sceneCatalogView.ShowBackButton(isActive);

            // Assert
            Assert.AreEqual(isActive, sceneCatalogView.backgBtn.gameObject.activeSelf, "The back button has not been activated properly!");
        }
    }
}