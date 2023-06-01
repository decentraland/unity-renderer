using AvatarShape_Tests;
using DCL;
using DCL.Helpers;
using NUnit.Framework;
using System.Collections;
using DCL.Components;
using DCL.Controllers;
using UnityEngine;
using UnityEngine.TestTools;
using DCLServices.WearablesCatalogService;
using MainScripts.DCL.Models.AvatarAssets.Tests.Helpers;

namespace Tests
{
    public class AvatarShapeTests : IntegrationTestSuite_Legacy
    {
        private ParcelScene scene;
        private CoreComponentsPlugin coreComponentsPlugin;
        private IWearablesCatalogService wearablesCatalogService;

        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();

            coreComponentsPlugin = new CoreComponentsPlugin();
            scene = TestUtils.CreateTestScene();
        }

        protected override ServiceLocator InitializeServiceLocator()
        {
            ServiceLocator serviceLocator = base.InitializeServiceLocator();
            wearablesCatalogService = AvatarAssetsTestHelpers.CreateTestCatalogLocal();
            serviceLocator.Register<IWearablesCatalogService>(() => wearablesCatalogService);
            return serviceLocator;
        }

        protected override IEnumerator TearDown()
        {
            DataStore.Clear();
            coreComponentsPlugin.Dispose();
            wearablesCatalogService.Dispose();
            yield return base.TearDown();
        }

        void AssertMaterialsAreCorrect(Transform root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>();

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];

                for (int i1 = 0; i1 < renderer.sharedMaterials.Length; i1++)
                {
                    Material material = renderer.sharedMaterials[i1];
                    Assert.IsTrue(material.shader.name.Contains("DCL"), $"Material must be from DCL. found {material.shader.name} instead!");
                }
            }
        }

        [Test]
        public void CreateStandardAvatar()
        {
            DataStore.i.avatarConfig.useHologramAvatar.Set(false);
            AvatarShape avatar = AvatarShapeTestHelpers.CreateAvatarShape(scene, "Abortit", "TestAvatar.json");
            Assert.IsTrue(avatar.avatar is AvatarSystem.Avatar);
            TestUtils.RemoveSceneEntity(scene, avatar.entity);
        }

        [Test]
        public void CreateAvatarWithHologram()
        {
            DataStore.i.avatarConfig.useHologramAvatar.Set(true);
            AvatarShape avatar = AvatarShapeTestHelpers.CreateAvatarShape(scene, "Abortit", "TestAvatar.json");
            Assert.IsTrue(avatar.avatar is AvatarSystem.AvatarWithHologram);
            TestUtils.RemoveSceneEntity(scene, avatar.entity);
        }

        [Test]
        public void SetLayersProperly()
        {
            AvatarShape avatar = AvatarShapeTestHelpers.CreateAvatarShape(scene, "Abortit", "TestAvatar.json");
            Assert.AreEqual(avatar.gameObject.layer, LayerMask.NameToLayer("ViewportCullingIgnored"));
            Assert.AreEqual(avatar.avatarCollider.gameObject.layer, LayerMask.NameToLayer("AvatarTriggerDetection"));
            Assert.AreEqual(avatar.onPointerDown.gameObject.layer, LayerMask.NameToLayer("OnPointerEvent"));
            TestUtils.RemoveSceneEntity(scene, avatar.entity);
        }

        [UnityTest]
        public IEnumerator DestroyWhileLoading()
        {
            AvatarShape avatar = AvatarShapeTestHelpers.CreateAvatarShape(scene, "Abortit", "TestAvatar.json");

            GameObject goEntity = avatar.entity.gameObject;

            TestUtils.RemoveSceneEntity(scene, avatar.entity);

            yield return null;

            bool destroyedOrPooled = goEntity == null || !goEntity.activeSelf;
            Assert.IsTrue(destroyedOrPooled);
        }

        [UnityTest]
        public IEnumerator InterpolatePosition()
        {
            AvatarShape avatar = AvatarShapeTestHelpers.CreateAvatarShape(scene, "Abortitus", "TestAvatar.json");

            // We must wait for the AvatarShape to finish or the OnTransformChanged event is not used
            yield return new DCL.WaitUntil(() => avatar.everythingIsLoaded, 20);

            Assert.AreEqual(0f, avatar.entity.gameObject.transform.position.x);
            Assert.AreEqual(0f, avatar.entity.gameObject.transform.position.z);

            // Update position to the other end of the parcel
            var transformModel = new DCLTransform.Model { position = new Vector3(15, 2, 15) };

            TestUtils.SetEntityTransform(scene, avatar.entity, transformModel);

            yield return null;

            Assert.AreNotEqual(15f, avatar.entity.gameObject.transform.position.x);
            Assert.AreNotEqual(15f, avatar.entity.gameObject.transform.position.z);
        }

        [UnityTest]
        public IEnumerator MaterialsSetCorrectly()
        {
            AvatarShape avatar = AvatarShapeTestHelpers.CreateAvatarShape(scene, "Joan Darteis", "TestAvatar.json");
            yield return new DCL.WaitUntil(() => avatar.everythingIsLoaded, 20);

            AssertMaterialsAreCorrect(avatar.transform);
        }

        [UnityTest]
        public IEnumerator WhenTwoAvatarsLoadAtTheSameTimeTheyHaveProperMaterials()
        {
            //NOTE(Brian): Avatars must be equal to share their meshes.
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
