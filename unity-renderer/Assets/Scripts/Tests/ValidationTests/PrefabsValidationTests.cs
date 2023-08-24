using DCL.HUD.Common;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static DefaultNamespace.TestsUtils;

namespace Tests.ValidationTests
{
    [Category("EditModeCI")]
    public class PrefabsValidationTests
    {
        private static readonly string[] SEARCH_IN_FOLDERS = { "Assets/DCLServices", "Assets/DCLPlugins", "Assets/Resources", "Assets/Scripts/MainScripts/DCL/Controllers/InputController" };

        [TestCaseSource(nameof(AllPrefabInAssetsFolder))]
        public void ValidateShowHideAnimators(string prefabPath)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            IEnumerable<string> unityAnimators =
                from showHideAnimator in prefab.GetComponentsInChildren<ShowHideAnimator>()
                where showHideAnimator.GetComponent<Animator>() != null
                select $"{showHideAnimator.gameObject.name}";

            Assert.That(unityAnimators, Is.Empty, "Unity animator is presented on several child objects that have DCL ShowHideAnimator component (DOTween-based)");
        }

        [TestCaseSource(nameof(AllPrefabInAssetsFolder))]
        public void ScrollsIncludesSensibilityMultiplierByPlatform(string prefabPath)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            IEnumerable<string> scrollsWithoutSensibility = from scroll in prefab.GetComponentsInChildren<ScrollRect>(true)
                where scroll.GetComponent<ScrollRectSensitivityHandler>() == null && scroll.GetComponent<DynamicScrollSensitivity>() == null
                select $"{scroll.gameObject.name}";

            Assert.That(scrollsWithoutSensibility, Is.Empty);
        }

        [TestCaseSource(nameof(PrefabPaths))]
        public void PrefabShouldNotHaveMissingReferences(string prefabPath)
        {
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (PrefabUtility.GetPrefabAssetType(prefabAsset) != PrefabAssetType.NotAPrefab)
            {
                Assert.IsFalse(HasMissingScripts(prefabAsset), $"Prefab at {prefabPath} has missing script!");
                Assert.IsFalse(HasBrokenReferences(prefabAsset, out string brokenReferenceMessage), $"Prefab at {prefabPath} {brokenReferenceMessage}");
            }
        }

        [TestCaseSource(nameof(ScriptableObjectPaths))]
        public void ScriptableObjectShouldNotHaveMissingReferences(string scriptableObjectPath)
        {
            ScriptableObject scriptableObjectAsset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(scriptableObjectPath);
            Assert.IsFalse(HasBrokenReferences(scriptableObjectAsset, out string brokenReferenceMessage), $"ScriptableObject at {scriptableObjectPath} {brokenReferenceMessage}");
        }

        private static IEnumerable<string> AllPrefabInAssetsFolder() =>
            AllAssetsAtPaths(assetTypes: "t: Prefab", ASSETS_FOLDER_PATH);

        public static IEnumerable<string> PrefabPaths() =>
            AssetDatabase.FindAssets("t:Prefab", SEARCH_IN_FOLDERS).Select(AssetDatabase.GUIDToAssetPath);

        public static IEnumerable<string> ScriptableObjectPaths() =>
            AssetDatabase.FindAssets("t:ScriptableObject", SEARCH_IN_FOLDERS).Select(AssetDatabase.GUIDToAssetPath);

        private static bool HasMissingScripts(GameObject obj)
        {
            Component[] components = obj.GetComponents<Component>();
            return components.Any(comp => comp == null) || obj.transform.Cast<Transform>().Any(child => HasMissingScripts(child.gameObject));
        }

        private static bool HasBrokenReferences(Object obj, out string brokenReferenceMessage)
        {
            var serializedObject = new SerializedObject(obj);
            SerializedProperty prop = serializedObject.GetIterator();

            while (prop.NextVisible(true))
                if (prop.propertyType == SerializedPropertyType.ObjectReference && prop.objectReferenceValue == null && prop.objectReferenceInstanceIDValue != 0)
                {
                    brokenReferenceMessage = $"Object: '{obj.name}', Field: {prop.displayName} has a broken reference!";
                    return true; // There's a broken reference!
                }

            if (obj is GameObject gameObject)
                foreach (Transform child in gameObject.transform)
                    if (HasBrokenReferences(child.gameObject, out brokenReferenceMessage))
                        return true;

            brokenReferenceMessage = "";
            return false;
        }
    }
}
