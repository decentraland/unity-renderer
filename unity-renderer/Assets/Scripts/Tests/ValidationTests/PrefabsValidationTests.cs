using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

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

        private static IEnumerable<string> AllPrefabPaths() =>
            AssetDatabase
               .FindAssets("t:Prefab", PREFAB_PATHS)
               .Select(AssetDatabase.GUIDToAssetPath);
    }
}
