using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Rendering.LoadingAvatar.Editor
{
    [TestFixture]
    [Category("EditModeCI")]
    public class BaseAvatarTest
    {
        private const string assetPath = "Assets/Rendering/LoadingAvatar/CrossSection/AvatarBase.prefab";

        [Test]
        public void BaseAvatarIsImportedCorrectly()
        {
            GameObject importedGameObject = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            Assert.IsNotNull(importedGameObject, "AvatarBase Asset");

            var skinnedMeshRenderer = importedGameObject.GetComponentInChildren<SkinnedMeshRenderer>();
            Assert.IsNotNull(skinnedMeshRenderer.sharedMesh, "AvatarBase renderer mesh");
        }
    }
}
