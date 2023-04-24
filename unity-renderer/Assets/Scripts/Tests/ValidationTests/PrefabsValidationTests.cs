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
        [TestCaseSource(nameof(AllPrefabInAssetsFolder))]
        public void ValidateShowHideAnimators(string prefabPath)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            var unityAnimators =
                from showHideAnimator in prefab.GetComponentsInChildren<ShowHideAnimator>()
                where showHideAnimator.GetComponent<Animator>() != null
                select $"{showHideAnimator.gameObject.name}";

            Assert.That(unityAnimators, Is.Empty, "Unity animator is presented on several child objects that have DCL ShowHideAnimator component (DOTween-based)");
        }

        [TestCaseSource(nameof(AllPrefabInAssetsFolder))]
        public void ScrollsIncludesSensibilityMultiplierByPlatform(string prefabPath)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            var scrollsWithoutSensibility = from scroll in prefab.GetComponentsInChildren<ScrollRect>(true)
                where scroll.GetComponent<ScrollRectSensitivityHandler>() == null && scroll.GetComponent<DynamicScrollSensitivity>() == null
                select $"{scroll.gameObject.name}";

            Assert.That(scrollsWithoutSensibility, Is.Empty);
        }

        private static IEnumerable<string> AllPrefabInAssetsFolder() =>
            AllAssetsAtPaths( assetTypes: "t: Prefab", ASSETS_FOLDER_PATH);
    }
}
