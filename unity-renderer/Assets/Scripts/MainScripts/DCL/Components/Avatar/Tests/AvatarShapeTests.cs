using AvatarShape_Tests;
using DCL;
using DCL.Helpers;
using NUnit.Framework;
using System.Collections;
using DCL.Components;
using DCL.Interface;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class AvatarShapeTests : TestsBase
    {

        void AssertMaterialsAreCorrect(Transform root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>();

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];

                for (int i1 = 0; i1 < renderer.sharedMaterials.Length; i1++)
                {
                    Material material = renderer.sharedMaterials[i1];
                    Assert.IsTrue(!material.shader.name.Contains("Lit"), $"Material must not be LWRP Lit. found {material.shader.name} instead!");
                }
            }
        }

        [UnityTest]
        public IEnumerator DestroyWhileLoading()
        {
            AvatarAssetsTestHelpers.CreateTestCatalogLocal();
            AvatarShape avatar = AvatarShapeTestHelpers.CreateAvatarShape(scene, "Abortit", "TestAvatar.json");

            GameObject goEntity = avatar.entity.gameObject;

            TestHelpers.RemoveSceneEntity(scene, avatar.entity);

            yield return null;

            bool destroyedOrPooled = goEntity == null || !goEntity.activeSelf;
            Assert.IsTrue(destroyedOrPooled);
        }

        [UnityTest]
        [Category("Explicit")]
        [Explicit("Test too slow")]
        public IEnumerator InterpolatePosition()
        {
            AvatarAssetsTestHelpers.CreateTestCatalogLocal();
            AvatarShape avatar = AvatarShapeTestHelpers.CreateAvatarShape(scene, "Abortitus", "TestAvatar.json");

            // We must wait for the AvatarShape to finish or the OnTransformChanged event is not used
            yield return new DCL.WaitUntil(() => avatar.everythingIsLoaded, 20);

            Assert.AreEqual(0f, avatar.entity.gameObject.transform.position.x);
            Assert.AreEqual(0f, avatar.entity.gameObject.transform.position.z);

            // Update position to the other end of the parcel
            var transformModel = new DCLTransform.Model { position = new Vector3(15, 2, 15) };

            TestHelpers.SetEntityTransform(scene, avatar.entity, transformModel);

            yield return null;

            Assert.AreNotEqual(15f, avatar.entity.gameObject.transform.position.x);
            Assert.AreNotEqual(15f, avatar.entity.gameObject.transform.position.z);
        }

        [UnityTest]
        [Category("Explicit")]
        [Explicit("Test too slow")]
        public IEnumerator MaterialsSetCorrectly()
        {
            AvatarAssetsTestHelpers.CreateTestCatalogLocal();
            AvatarShape avatar = AvatarShapeTestHelpers.CreateAvatarShape(scene, "Joan Darteis", "TestAvatar.json");
            yield return new DCL.WaitUntil(() => avatar.everythingIsLoaded, 20);

            AssertMaterialsAreCorrect(avatar.transform);
        }


        [UnityTest]
        [Category("Explicit")]
        [Explicit("Test too slow")]
        public IEnumerator NameBackgroundHasCorrectSize()
        {
            AvatarAssetsTestHelpers.CreateTestCatalogLocal();
            AvatarShape avatar = AvatarShapeTestHelpers.CreateAvatarShape(scene, "Maiqel Yacson", "TestAvatar.json");
            yield return new DCL.WaitUntil(() => avatar.everythingIsLoaded, 20);

            avatar.transform.position = new Vector3(13, 0, 4);

            RectTransform rt = avatar.avatarName.uiContainer.GetComponent<RectTransform>();

            Assert.IsTrue((int)Mathf.Abs(rt.sizeDelta.x) == 190 && (int)Mathf.Abs(rt.sizeDelta.y) == 40, $"Avatar name dimensions are incorrect!. Current: {rt.sizeDelta}");
        }

        [UnityTest]
        [Category("Explicit")]
        [Explicit("Test too slow")]
        public IEnumerator WhenTwoAvatarsLoadAtTheSameTimeTheyHaveProperMaterials()
        {
            //NOTE(Brian): Avatars must be equal to share their meshes.
            AvatarAssetsTestHelpers.CreateTestCatalogLocal();
            AvatarShape avatar = AvatarShapeTestHelpers.CreateAvatarShape(scene, "Naicholas Keig", "TestAvatar.json");
            AvatarShape avatar2 = AvatarShapeTestHelpers.CreateAvatarShape(scene, "Naicholas Keig", "TestAvatar2.json");

            avatar.transform.position = new Vector3(-5, 0, 0);
            avatar2.transform.position = new Vector3(5, 0, 0);

            yield return new DCL.WaitUntil(() => avatar.everythingIsLoaded && avatar2.everythingIsLoaded, 25);

            AssertMaterialsAreCorrect(avatar.transform);
            AssertMaterialsAreCorrect(avatar2.transform);
        }
    }
}
