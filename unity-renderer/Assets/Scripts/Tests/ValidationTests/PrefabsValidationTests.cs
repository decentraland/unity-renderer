using DCL.HUD.Common;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Tests.ValidationTests
{
    public class PrefabsValidationTests
    {
        private static readonly string[] PREFAB_PATHS = { "Assets" };

        [TestCaseSource(nameof(AllPrefabPaths))]
        public void ValidateShowHideAnimators(string prefabPath)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            var unityAnimators =
                from showHideAnimator in prefab.GetComponentsInChildren<ShowHideAnimator>()
                where showHideAnimator.GetComponent<Animator>() != null
                select $"{showHideAnimator.gameObject.name}";

            Assert.That(unityAnimators, Is.Empty, "Unity animator is presented on several child objects that have DCL ShowHideAnimator component (DOTween-based)");
        }

        [TestCaseSource(nameof(AllPrefabPaths))]
        public void ScrollsIncludesSensibilityMultiplierByPlatform(string prefabPath)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            var scrollsWithoutSensibility = from scroll in prefab.GetComponentsInChildren<ScrollRect>(true)
                where scroll.GetComponent<ScrollRectSensitivityHandler>() == null && scroll.GetComponent<DynamicScrollSensitivity>() == null
                select $"{scroll.gameObject.name}";

            Assert.That(scrollsWithoutSensibility, Is.Empty);
        }

        private static IEnumerable<string> AllPrefabPaths() =>
            AssetDatabase
               .FindAssets("t:Prefab", PREFAB_PATHS)
               .Select(AssetDatabase.GUIDToAssetPath);
    }
}
