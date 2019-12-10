using DCL;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;
using UnityGLTF;

namespace Tests
{
    public static class AvatarTestHelpers
    {
        public static WearableDictionary CreateTestCatalog(string file = "TestCatalog.json")
        {
            var catalogJson = File.ReadAllText(Application.dataPath + $"/../TestResources/Avatar/{file}"); //Utils.GetTestAssetPath returns an URI not compatible with the really convenient File.ReadAllText
            var wearables = Newtonsoft.Json.JsonConvert.DeserializeObject<WearableItem[]>(catalogJson); // JsonUtility cannot deserialize jsons whose root is an array
            CatalogController.wearableCatalog.Clear();
            CatalogController.wearableCatalog.Add(wearables.Select(x => new KeyValuePair<string, WearableItem>(x.id, x)).ToArray());

            return CatalogController.wearableCatalog;
        }

        public static AvatarShape CreateAvatar(ParcelScene scene, string name, string fileName)
        {
            var model = GetTestAvatarModel(name, fileName);

            return CreateAvatar(scene, model);
        }

        public static AvatarShape CreateAvatar(ParcelScene scene, AvatarModel model)
        {
            GLTFSceneImporter.budgetPerFrameInMilliseconds = float.MaxValue;
            DecentralandEntity entity = TestHelpers.CreateSceneEntity(scene);
            AvatarShape shape = TestHelpers.EntityComponentCreate<AvatarShape, AvatarModel>(scene, entity, model, CLASS_ID_COMPONENT.AVATAR_SHAPE);
            TestHelpers.SetEntityTransform(scene, entity, new Vector3(0, 0, 0), Quaternion.identity, Vector3.one);
            return shape;
        }

        public static AvatarModel GetTestAvatarModel(string name, string fileName)
        {
            var avatarjson = File.ReadAllText(Application.dataPath + "/../TestResources/Avatar/" + fileName);
            AvatarModel model = JsonUtility.FromJson<AvatarModel>(avatarjson);
            model.name = name;
            return model;
        }
    }

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
            yield return InitScene();

            AvatarTestHelpers.CreateTestCatalog();
            AvatarShape avatar = AvatarTestHelpers.CreateAvatar(scene, "Abortit", "TestAvatar.json");

            yield return new WaitForSeconds(3.0f);

            GameObject goEntity = avatar.entity.gameObject;

            TestHelpers.RemoveSceneEntity(scene, avatar.entity);

            yield return null;

            bool destroyedOrPooled = goEntity == null || !goEntity.activeSelf;
            Assert.IsTrue(destroyedOrPooled);
        }

        [UnityTest]
        public IEnumerator MaterialsSetCorrectly()
        {
            yield return InitScene();

            AvatarTestHelpers.CreateTestCatalog();
            AvatarShape avatar = AvatarTestHelpers.CreateAvatar(scene, "Joan Darteis", "TestAvatar.json");
            yield return new DCL.WaitUntil(() => avatar.everythingIsLoaded, 20);

            AssertMaterialsAreCorrect(avatar.transform);
        }


        [UnityTest]
        public IEnumerator NameBackgroundHasCorrectSize()
        {
            yield return InitScene();
            AvatarTestHelpers.CreateTestCatalog();
            AvatarShape avatar = AvatarTestHelpers.CreateAvatar(scene, "Maiqel Yacson", "TestAvatar.json");
            yield return new DCL.WaitUntil(() => avatar.everythingIsLoaded, 20);

            avatar.transform.position = new Vector3(13, 0, 4);

            RectTransform rt = avatar.avatarName.uiContainer.GetComponent<RectTransform>();

            Assert.IsTrue((int)Mathf.Abs(rt.sizeDelta.x) == 190 && (int)Mathf.Abs(rt.sizeDelta.y) == 40, $"Avatar name dimensions are incorrect!. Current: {rt.sizeDelta}");
        }

        [UnityTest]
        public IEnumerator WhenTwoAvatarsLoadAtTheSameTimeTheyHaveProperMaterials()
        {
            yield return InitScene();

            //NOTE(Brian): Avatars must be equal to share their meshes.
            AvatarTestHelpers.CreateTestCatalog();
            AvatarShape avatar = AvatarTestHelpers.CreateAvatar(scene, "Naicholas Keig", "TestAvatar.json");
            AvatarShape avatar2 = AvatarTestHelpers.CreateAvatar(scene, "Naicholas Keig", "TestAvatar2.json");

            avatar.transform.position = new Vector3(-5, 0, 0);
            avatar2.transform.position = new Vector3(5, 0, 0);

            yield return new DCL.WaitUntil(() => avatar.everythingIsLoaded && avatar2.everythingIsLoaded, 25);

            AssertMaterialsAreCorrect(avatar.transform);
            AssertMaterialsAreCorrect(avatar2.transform);
        }
    }
}