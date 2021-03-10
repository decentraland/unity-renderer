using NUnit.Framework;
using UnityEditor;

namespace Tests
{
    public class BuilderProjectsPanelViewPrefabCheck
    {
        private BuilderProjectsPanelView prefab;

        [SetUp]
        public void SetUp()
        {
            const string prefabAssetPath =
                "Assets/Scripts/MainScripts/DCL/Controllers/HUD/BuilderProjectsPanel/Resources/BuilderProjectsPanel.prefab";
            prefab = AssetDatabase.LoadAssetAtPath<BuilderProjectsPanelView>(prefabAssetPath);
        }

        [Test]
        public void SectionsContainerShouldBeEmpty()
        {
            Assert.AreEqual(0, prefab.sectionsContainer.transform.childCount);
        }
    }
}