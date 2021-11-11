using DCL.Builder;
using NUnit.Framework;
using UnityEditor;

namespace Tests
{
    public class BuilderProjectsPanelViewPrefabCheck
    {
        private BuilderMainPanelView prefab;

        [SetUp]
        public void SetUp()
        {
            prefab = TestAssets.Get<BuilderMainPanelView>("BuilderProjectsPanel.prefab");
        }

        [Test]
        public void SectionsContainerShouldBeEmpty() { Assert.AreEqual(0, prefab.sectionsContainer.transform.childCount); }
    }
}