using DCL;
using DCL.Components;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Helpers;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Tests;
using UnityEngine;

public class BIWOutlinerShould : IntegrationTestSuite_Legacy
{
    private const string ENTITY_ID = "1";
    private DCLBuilderInWorldEntity entity;
    private BIWOutlinerController outlinerController;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        BuilderInWorldController controller = Resources.FindObjectsOfTypeAll<BuilderInWorldController>()[0];
        outlinerController = controller.outlinerController;

        TestHelpers.CreateSceneEntity(scene, ENTITY_ID);

        TestHelpers.CreateAndSetShape(scene, ENTITY_ID, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
            new
            {
                src = Utils.GetTestsAssetsPath() + "/GLB/Trunk/Trunk.glb"
            }));

        LoadWrapper gltfShape = GLTFShape.GetLoaderForEntity(scene.entities[ENTITY_ID]);
        yield return new WaitUntil(() => gltfShape.alreadyLoaded);
        controller.builderInWorldEntityHandler.EnterEditMode(scene);
        entity = controller.builderInWorldEntityHandler.GetConvertedEntity(scene.entities[ENTITY_ID]);
    }

    [Test]
    public void OutlineEnity()
    {
        outlinerController.OutlineEntity(entity);
        Assert.IsTrue(outlinerController.IsEntityOutlined(entity));

        outlinerController.CancelEntityOutline(entity);
        Assert.IsFalse(outlinerController.IsEntityOutlined(entity));
    }

    [Test]
    public void OutlineLayer()
    {
        outlinerController.OutlineEntity(entity);
        Assert.AreEqual(entity.rootEntity.meshesInfo.renderers[0].gameObject.layer, BuilderInWorldSettings.SELECTION_LAYER);

        outlinerController.CancelEntityOutline(entity);
        Assert.AreNotEqual(entity.rootEntity.meshesInfo.renderers[0].gameObject.layer, BuilderInWorldSettings.SELECTION_LAYER);
    }

    [Test]
    public void OutlineLockEntities()
    {
        entity.SetIsLockedValue(true);
        outlinerController.OutlineEntity(entity);
        Assert.IsFalse(outlinerController.IsEntityOutlined(entity));
    }
}
