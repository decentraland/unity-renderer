﻿using DCL.Builder;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Tests
{
    public class SceneCardViewShould
    {
        private SceneCardView cardView;

        [SetUp]
        public void SetUp()
        {
            var prefab = Resources.Load<SceneCardView>("SceneCardView");
            cardView = UnityEngine.Object.Instantiate(prefab);
        }

        [TearDown]
        public void TearDown() { UnityEngine.Object.Destroy(cardView.gameObject); }

        [Test]
        public void DisplayCorrectlyWhenSceneIsDeployed()
        {
            ((ISceneCardView)cardView).Setup(new SceneData()
            {
                id = "",
                isDeployed = true,
                name = "test",
                coords = Vector2Int.zero,
                size = Vector2Int.zero,
                isOwner = true,
                isEditable = true
            });

            //should show both jump-in and editor buttons
            Assert.IsTrue(cardView.jumpInButton.gameObject.activeSelf, "JumpIn button should be active");
            Assert.IsTrue(cardView.editorButton.gameObject.activeSelf, "Editor button should be active");

            //should show coords instead of size
            Assert.IsTrue(cardView.coordsContainer.activeSelf, "Coords should be displayed");
            Assert.IsFalse(cardView.sizeContainer.activeSelf, "Size should not be displayed");

            //should show role
            Assert.IsTrue(cardView.roleOwnerGO.activeSelf, "Owner role tag should be displayed");
            Assert.IsFalse(cardView.roleOperatorGO.activeSelf, "Operator role tag should not be displayed");
            Assert.IsFalse(cardView.roleContributorGO.activeSelf, "Contributor role tag should not be displayed");
        }

        [Test]
        public void DisplayCorrectlyWhenSceneIsNotDeployed()
        {
            ((ISceneCardView)cardView).Setup(new SceneData()
            {
                id = "",
                isDeployed = false,
                name = "test",
                coords = Vector2Int.zero,
                size = Vector2Int.zero,
                isContributor = true,
                isEditable = true
            });

            //should show only editor button, no jump-in
            Assert.IsFalse(cardView.jumpInButton.gameObject.activeSelf, "JumpIn button should not be active");
            Assert.IsTrue(cardView.editorButton.gameObject.activeSelf, "Editor button should be active");

            //should show size instead of coords
            Assert.IsFalse(cardView.coordsContainer.activeSelf, "Coords should not be displayed");
            Assert.IsTrue(cardView.sizeContainer.activeSelf, "Size should be displayed");

            //should show role
            Assert.IsTrue(cardView.roleContributorGO.activeSelf, "Contributor role tag should be displayed");
            Assert.IsFalse(cardView.roleOperatorGO.activeSelf, "Operator role tag should not be displayed");
            Assert.IsFalse(cardView.roleOwnerGO.activeSelf, "Owner role tag should not be displayed");
        }

        [Test]
        public void DisplayCorrectlyWhenSceneIsNotEditable()
        {
            ((ISceneCardView)cardView).Setup(new SceneData()
            {
                id = "",
                isDeployed = false,
                name = "test",
                coords = Vector2Int.zero,
                size = Vector2Int.zero,
                isContributor = true,
                isEditable = false
            });

            Assert.IsFalse(cardView.editorButton.gameObject.activeSelf, "Editor button should not be active");
            Assert.IsTrue(cardView.editorLockedGO.activeSelf, "Editor locked indicator should be active");
        }

        [Test]
        public void DisplayCorrectlyWhenSceneIsEditable()
        {
            ((ISceneCardView)cardView).Setup(new SceneData()
            {
                id = "",
                isDeployed = false,
                name = "test",
                coords = Vector2Int.zero,
                size = Vector2Int.zero,
                isContributor = true,
                isEditable = true
            });

            Assert.IsTrue(cardView.editorButton.gameObject.activeSelf, "Editor button should be active");
            Assert.IsFalse(cardView.editorLockedGO.activeSelf, "Editor locked indicator should not be active");
        }
    }
}