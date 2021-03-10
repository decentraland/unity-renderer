using AvatarShape_Tests;
using DCL;
using DCL.Helpers;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

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
        protected void TearDown()
        {
            Object.Destroy(parentGameObject);
        }

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
    }
}
