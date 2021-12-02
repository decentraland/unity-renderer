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
            prefab = Resources.Load<BuilderMainPanelView>("BuilderProjectsPanel");
        }

        [Test]
        public void SectionsContainerShouldBeEmpty() { Assert.AreEqual(0, prefab.sectionsContainer.transform.childCount); }
    }
}