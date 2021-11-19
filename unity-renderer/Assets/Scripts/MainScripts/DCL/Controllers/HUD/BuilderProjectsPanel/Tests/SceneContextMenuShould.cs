﻿using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Tests
{
    public class SceneContextMenuShould
    {
        private SceneCardViewContextMenu contextMenu;

        [SetUp]
        public void SetUp()
        {
            var prefab = Resources.Load<SceneCardViewContextMenu>("SceneCardViewContextMenu");
            contextMenu = UnityEngine.Object.Instantiate(prefab);
        }

        [TearDown]
        public void TearDown() { UnityEngine.Object.Destroy(contextMenu.gameObject); }

        [Test]
        [Explicit("Not valid for MVP version")]
        [Category("Explicit")]
        public void ShowOptionsForOwnerDeployedScene()
        {
            contextMenu.Show("", isSceneDeployed: true, isOwnerOrOperator: true, isContributor: false);

            Assert.IsFalse(contextMenu.deleteButton.gameObject.activeSelf, "Option should not be displayed");
            Assert.IsFalse(contextMenu.shareButton.gameObject.activeSelf, "Option should not be displayed");
            Assert.IsFalse(contextMenu.duplicateButton.gameObject.activeSelf, "Option should not be displayed");
            Assert.IsFalse(contextMenu.quitContributorButton.gameObject.activeSelf, "Option should not be displayed");

            Assert.IsTrue(contextMenu.settingsButton.gameObject.activeSelf, "Option should be displayed");
            Assert.IsTrue(contextMenu.duplicateAsProjectButton.gameObject.activeSelf, "Option should be displayed");
            Assert.IsTrue(contextMenu.downloadButton.gameObject.activeSelf, "Option should be displayed");
            Assert.IsTrue(contextMenu.unpublishButton.gameObject.activeSelf, "Option should be displayed");
        }

        [Test]
        public void ShowOptionsForOwnerDeployedSceneMVP()
        {
            contextMenu.Show("", isSceneDeployed: true, isOwnerOrOperator: true, isContributor: false);

            Assert.IsFalse(contextMenu.deleteButton.gameObject.activeSelf, "Option should not be displayed");
            Assert.IsFalse(contextMenu.shareButton.gameObject.activeSelf, "Option should not be displayed");
            Assert.IsFalse(contextMenu.duplicateButton.gameObject.activeSelf, "Option should not be displayed");
            Assert.IsFalse(contextMenu.quitContributorButton.gameObject.activeSelf, "Option should not be displayed");
            Assert.IsFalse(contextMenu.settingsButton.gameObject.activeSelf, "Option should not be displayed");
            Assert.IsFalse(contextMenu.duplicateAsProjectButton.gameObject.activeSelf, "Option should not be displayed");
            Assert.IsFalse(contextMenu.downloadButton.gameObject.activeSelf, "Option should not be displayed");
            Assert.IsTrue(contextMenu.unpublishButton.gameObject.activeSelf, "Option should be displayed");
        }

        [Test]
        [Explicit("Not valid for MVP version")]
        [Category("Explicit")]
        public void ShowOptionsForContributorDeployedScene()
        {
            contextMenu.Show("", isSceneDeployed: true, isOwnerOrOperator: false, isContributor: true);

            Assert.IsFalse(contextMenu.deleteButton.gameObject.activeSelf, "Option should not be displayed");
            Assert.IsFalse(contextMenu.shareButton.gameObject.activeSelf, "Option should not be displayed");
            Assert.IsFalse(contextMenu.duplicateButton.gameObject.activeSelf, "Option should not be displayed");
            Assert.IsFalse(contextMenu.settingsButton.gameObject.activeSelf, "Option should not be displayed");
            Assert.IsFalse(contextMenu.unpublishButton.gameObject.activeSelf, "Option should not be displayed");

            Assert.IsTrue(contextMenu.duplicateAsProjectButton.gameObject.activeSelf, "Option should be displayed");
            Assert.IsTrue(contextMenu.downloadButton.gameObject.activeSelf, "Option should be displayed");
            Assert.IsTrue(contextMenu.quitContributorButton.gameObject.activeSelf, "Option should be displayed");
        }

        [Test]
        public void ShowOptionsForOwnerProjectScene()
        {
            contextMenu.Show("", isSceneDeployed: false, isOwnerOrOperator: true, isContributor: false);

            Assert.IsFalse(contextMenu.quitContributorButton.gameObject.activeSelf, "Option should not be displayed");
            Assert.IsFalse(contextMenu.duplicateAsProjectButton.gameObject.activeSelf, "Option should not be displayed");
            Assert.IsFalse(contextMenu.unpublishButton.gameObject.activeSelf, "Option should not be displayed");

            Assert.IsTrue(contextMenu.duplicateButton.gameObject.activeSelf, "Option should be displayed");
            Assert.IsTrue(contextMenu.settingsButton.gameObject.activeSelf, "Option should be displayed");
            Assert.IsTrue(contextMenu.downloadButton.gameObject.activeSelf, "Option should be displayed");
            Assert.IsTrue(contextMenu.deleteButton.gameObject.activeSelf, "Option should be displayed");
            Assert.IsTrue(contextMenu.shareButton.gameObject.activeSelf, "Option should be displayed");
        }

        [Test]
        public void ShowOptionsForContributorProjectScene()
        {
            contextMenu.Show("", isSceneDeployed: false, isOwnerOrOperator: false, isContributor: true);

            Assert.IsFalse(contextMenu.duplicateAsProjectButton.gameObject.activeSelf, "Option should not be displayed");
            Assert.IsFalse(contextMenu.unpublishButton.gameObject.activeSelf, "Option should not be displayed");
            Assert.IsFalse(contextMenu.settingsButton.gameObject.activeSelf, "Option should not be displayed");
            Assert.IsFalse(contextMenu.deleteButton.gameObject.activeSelf, "Option should not be displayed");

            Assert.IsTrue(contextMenu.duplicateButton.gameObject.activeSelf, "Option should be displayed");
            Assert.IsTrue(contextMenu.downloadButton.gameObject.activeSelf, "Option should be displayed");
            Assert.IsTrue(contextMenu.shareButton.gameObject.activeSelf, "Option should be displayed");
            Assert.IsTrue(contextMenu.quitContributorButton.gameObject.activeSelf, "Option should be displayed");
        }
    }
}