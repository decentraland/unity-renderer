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
            const string prefabAssetPath =
                "Assets/Scripts/MainScripts/DCL/Controllers/HUD/BuilderProjectsPanel/Resources/BuilderProjectsPanel.prefab";
            prefab = AssetDatabase.LoadAssetAtPath<BuilderMainPanelView>(prefabAssetPath);
        }

        [Test]
        public void SectionsContainerShouldBeEmpty() { Assert.AreEqual(0, prefab.sectionsContainer.transform.childCount); }
    }
}