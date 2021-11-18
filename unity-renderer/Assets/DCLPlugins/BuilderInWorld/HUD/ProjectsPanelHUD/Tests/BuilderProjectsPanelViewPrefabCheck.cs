using DCL.Builder;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Tests
{
    public class BuilderProjectsPanelViewPrefabCheck
    {
        private BuilderMainPanelView prefab;

        [SetUp]
        public void SetUp()
        {
            const string prefabAssetPath ="BuilderProjectsPanel";
            prefab =  Resources.Load<BuilderMainPanelView>(prefabAssetPath);
        }

        [Test]
        public void SectionsContainerShouldBeEmpty() { Assert.AreEqual(0, prefab.sectionsContainer.transform.childCount); }
    }
}