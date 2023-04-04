using DCL;
using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using UnityEditor;
using UnityEngine;

public class EntityMaterialUpdateTestController : MonoBehaviour
{
    void Start()
    {
        var sceneController = Environment.i.world.sceneController;
        var scenesToLoad = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Scripts/Tests/TestJSON/SceneLoadingTest").text;

        sceneController.UnloadAllScenes();
        sceneController.LoadParcelScenes(scenesToLoad);

        var scene = Environment.i.world.state.GetScene(666) as ParcelScene;

        DCLTexture dclAtlasTexture = TestUtils.CreateDCLTexture(
            scene,
            TestAssetsUtils.GetPath() + "/Images/atlas.png",
            DCLTexture.BabylonWrapMode.CLAMP,
            FilterMode.Bilinear);

        DCLTexture dclAvatarTexture = TestUtils.CreateDCLTexture(
            scene,
            TestAssetsUtils.GetPath() + "/Images/avatar.png",
            DCLTexture.BabylonWrapMode.CLAMP,
            FilterMode.Bilinear);


        IDCLEntity entity;

        TestUtils.CreateEntityWithBasicMaterial(
            scene,
            new BasicMaterial.Model
            {
                texture = dclAtlasTexture.id,
            },
            out entity);

        TestUtils.CreateEntityWithPBRMaterial(scene,
            new PBRMaterial.Model
            {
                albedoTexture = dclAvatarTexture.id,
                metallic = 0,
                roughness = 1,
            },
            out entity);

        PBRMaterial mat = TestUtils.CreateEntityWithPBRMaterial(scene,
            new PBRMaterial.Model
            {
                albedoTexture = dclAvatarTexture.id,
                metallic = 1,
                roughness = 1,
                alphaTexture = dclAvatarTexture.id,
            },
            out entity);

        // Re-assign last PBR material to new entity
        BoxShape shape = TestUtils.CreateEntityWithBoxShape(scene, new Vector3(5, 1, 2));
        BasicMaterial m =
            TestUtils.SharedComponentCreate<BasicMaterial, BasicMaterial.Model>(scene, CLASS_ID.BASIC_MATERIAL);

        Color color1;
        ColorUtility.TryParseHtmlString("#FF9292", out color1);

        // Update material attached to 2 entities, adding albedoColor
        scene.componentsManagerLegacy.SceneSharedComponentUpdate(m.id, JsonUtility.ToJson(new DCL.Components.PBRMaterial.Model
        {
            albedoTexture = dclAvatarTexture.id,
            metallic = 1,
            roughness = 1,
            alphaTexture = dclAvatarTexture.id,
            albedoColor = color1
        }));
    }
}
