using DCL;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class AvatarVisibilityShould
    {

        private GameObject parentGameObject;
        private AvatarVisibility visibility;
        private GameObject toggledGameObject;

        [SetUp]
        protected void SetUp()
        {
            // Prepare both game objects
            parentGameObject = new GameObject();
            visibility = parentGameObject.AddComponent<AvatarVisibility>();
            toggledGameObject = new GameObject();
            toggledGameObject.transform.parent = parentGameObject.transform;
            visibility.gameObjectsToToggle = new[] { toggledGameObject };
        }

        [TearDown]
        protected void TearDown() { Object.Destroy(parentGameObject); }

        [Test]
        public void DeactivateGameObjectsWhenVisibilityIsSetToFalse()
        {

            Assert.IsTrue(toggledGameObject.activeSelf);

            visibility.SetVisibility("Caller1", true);
            visibility.SetVisibility("Caller2", false);
            Assert.IsFalse(toggledGameObject.activeSelf);
        }

        [Test]
        public void ReactivateGameObjectsWhenVisibilityIsSetToTrue()
        {
            visibility.SetVisibility("Caller1", false);
            Assert.IsFalse(toggledGameObject.activeSelf);

            visibility.SetVisibility("Caller1", true);
            Assert.True(toggledGameObject.activeSelf);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetVisibilityForGameObjectsCorrectly(bool isVisible)
        {
            // Arrange
            Object.Destroy(visibility.gameObjectsToToggle[0]);
            visibility.gameObjectsToToggle = new[] { toggledGameObject, null };
            visibility.gameObjectsToToggle[0].SetActive(!isVisible);

            // Act
            bool isFailed = false;
            try
            {
                visibility.SetVisibilityForGameObjects(isVisible);
            }
            catch
            {
                isFailed = true;
            }

            // Assert
            Assert.AreEqual(isVisible, visibility.gameObjectsToToggle[0].activeSelf);
            Assert.IsFalse(isFailed, "The function call failed!");
        }
    }
}