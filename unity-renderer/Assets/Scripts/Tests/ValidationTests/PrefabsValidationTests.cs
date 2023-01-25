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
        public void ValidateShowHideAnimator(string prefabPath)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            IEnumerable<ShowHideAnimator> oldAnimators =
                prefab.GetComponentsInChildren<ShowHideAnimator>()
                      .Where(HasShowHideAnimator);

            Assert.That(oldAnimators, Is.Empty);
        }

        private static bool HasShowHideAnimator(ShowHideAnimator showHideAnimator)
        {
            var unityAnimator = showHideAnimator.GetComponent<Animator>();
            return unityAnimator != null && unityAnimator.runtimeAnimatorController.name.Contains("ShowHide");
        }

        private static IEnumerable<string> AllPrefabPaths() =>
            AssetDatabase
               .FindAssets("t:Prefab", PREFAB_PATHS)
               .Select(AssetDatabase.GUIDToAssetPath);
    }
}
