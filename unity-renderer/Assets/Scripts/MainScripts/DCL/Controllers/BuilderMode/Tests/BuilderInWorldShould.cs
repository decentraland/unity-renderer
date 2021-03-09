using Builder;
using Cinemachine;
using DCL;
using DCL.Components;
using DCL.Configuration;
using DCL.Helpers;
using DCL.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

public class BuilderInWorldShould : IntegrationTestSuite_Legacy
{

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        string entityId = "mockUpEntity";
        TestHelpers.CreateSceneEntity(scene, entityId);
    }

    [Test]
    public void GroundRaycast()
    {
        RaycastHit hit;
        BuilderInWorldController builderInWorldController = Resources.FindObjectsOfTypeAll<BuilderInWorldController>()[0];
        BuilderInWorldGodMode godMode = builderInWorldController.GetComponentInChildren<BuilderInWorldGodMode>(true);

        Vector3 fromPosition = new Vector3(0,10,0);
        Vector3 toPosition = Vector3.zero;
        Vector3 direction = toPosition - fromPosition;


        if (Physics.Raycast(fromPosition,direction, out hit, BuilderInWorldGodMode.RAYCAST_MAX_DISTANCE, godMode.groundLayer))
        {
            Assert.Pass();
            return;
        }

        UnityEngine.Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        if (Physics.Raycast(ray, out hit, BuilderInWorldGodMode.RAYCAST_MAX_DISTANCE, godMode.groundLayer))
        {
            Assert.Pass();
            return;
        }

        Assert.Fail("The ground layer is not set to Ground");
    }

    [Test]
    public void SceneReferences()
    {
        BuilderInWorldController builderInWorldController = Resources.FindObjectsOfTypeAll<BuilderInWorldController>()[0];

        Assert.IsNotNull(builderInWorldController.avatarRenderer,"References on the builder-in-world prefab are null, check them all!");
        Assert.IsNotNull(builderInWorldController.cursorGO, "References on the builder-in-world prefab are null, check them all!");
        Assert.IsNotNull(builderInWorldController.inputController, "References on the builder-in-world prefab are null, check them all!");
        Assert.IsNotNull(builderInWorldController.cameraParentGO, "References on the builder-in-world prefab are null, check them all!");


        BuilderInWorldGodMode godMode = builderInWorldController.GetComponentInChildren<BuilderInWorldGodMode>();


        Assert.IsNotNull(godMode.mouseCatcher, "References on the builder-in-world god mode are null, check them all!");
        Assert.IsNotNull(godMode.cameraController, "References on the builder-in-world god mode are null, check them all!");
        Assert.IsNotNull(godMode.freeCameraController, "References on the builder-in-world god mode are null, check them all!");

        DCLBuilderRaycast dCLBuilderRaycast =  godMode.GetComponentInChildren<DCLBuilderRaycast>();

        Assert.IsNotNull(dCLBuilderRaycast.builderCamera, "Camera reference on the builder-in-world god mode children are null, check them all!");

        VoxelController voxelController = godMode.GetComponent<VoxelController>();

        Assert.IsNotNull(voxelController.freeCameraMovement, "Camera reference on the builder-in-world voxel controller are null, check them all!");
    }

    [UnityTest]
    public IEnumerator SceneObjectFloorObject()
    {
        SceneObject sceneObject = BuilderInWorldUtils.CreateFloorSceneObject();
        LoadParcelScenesMessage.UnityParcelScene data = scene.sceneData;
        data.contents = new List<ContentServerUtils.MappingPair>();
        data.baseUrl = BuilderInWorldSettings.BASE_URL_CATALOG;

        foreach (KeyValuePair<string, string> content in sceneObject.contents)
        {
            ContentServerUtils.MappingPair mappingPair = new ContentServerUtils.MappingPair();
            mappingPair.file = content.Key;
            mappingPair.hash = content.Value;
            bool found = false;
            foreach (ContentServerUtils.MappingPair mappingPairToCheck in data.contents)
            {
                if (mappingPairToCheck.file == mappingPair.file)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
                data.contents.Add(mappingPair);
        }

        Environment.i.world.sceneController.UpdateParcelScenesExecute(data);


        string entityId = "1";
        TestHelpers.CreateSceneEntity(scene, entityId);

        TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
            new
            {
                assetId = BuilderInWorldSettings.FLOOR_TEXTURE_VALUE,
                src = BuilderInWorldSettings.FLOOR_MODEL
            })); ;

        LoadWrapper gltfShape = GLTFShape.GetLoaderForEntity(scene.entities[entityId]);
        yield return new WaitUntil(() => gltfShape.alreadyLoaded);

        Assert.IsTrue(
         scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() != null,
        "Floor should be loaded, is the SceneObject not working anymore?");
    }

    protected override IEnumerator TearDown()
    {
        AssetCatalogBridge.ClearCatalog();
        yield return base.TearDown();
    }
}
