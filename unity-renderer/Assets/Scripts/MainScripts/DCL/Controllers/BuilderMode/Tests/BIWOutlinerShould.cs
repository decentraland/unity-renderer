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
    private BIWEntity entity;
    private BIWEntityHandler entityHandler;
    private BIWOutlinerController outlinerController;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();

        TestHelpers.CreateSceneEntity(scene, ENTITY_ID);

        TestHelpers.CreateAndSetShape(scene, ENTITY_ID, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
            new
            {
                src = TestAssetsUtils.GetPath() + "/GLB/Trunk/Trunk.glb"
            }));

        LoadWrapper gltfShape = GLTFShape.GetLoaderForEntity(scene.entities[ENTITY_ID]);
        yield return new DCL.WaitUntil(() => gltfShape.alreadyLoaded);

        outlinerController = new BIWOutlinerController();
        entityHandler = new BIWEntityHandler();

        var referencesController = BIWTestHelper.CreateReferencesControllerWithGenericMocks(
            outlinerController,
            entityHandler
        );

        outlinerController.Init(referencesController);
        entityHandler.Init(referencesController);

        entityHandler.EnterEditMode(scene);
        outlinerController.EnterEditMode(scene);

        entity = entityHandler.GetConvertedEntity(scene.entities[ENTITY_ID]);
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
        Assert.AreEqual(entity.rootEntity.meshesInfo.renderers[0].gameObject.layer, LayerMask.NameToLayer("Selection"));

        outlinerController.CancelEntityOutline(entity);
        Assert.AreNotEqual(entity.rootEntity.meshesInfo.renderers[0].gameObject.layer, LayerMask.NameToLayer("Selection"));
    }

    [Test]
    public void OutlineLockEntities()
    {
        entity.SetIsLockedValue(true);
        outlinerController.OutlineEntity(entity);
        Assert.IsFalse(outlinerController.IsEntityOutlined(entity));
    }

    [Test]
    public void CheckOutline()
    {
        //Arrange
        outlinerController.OutlineEntity(entity);

        //Act
        for (int i = 0; i <= BIWOutlinerController.OUTLINER_OPTIMIZATION_TIMES; i++)
        {
            outlinerController.CheckOutline();
        }

        //Assert
        Assert.IsFalse(outlinerController.IsEntityOutlined(entity));
    }

    [Test]
    public void CheckCameraComponetAdded()
    {
        //Act
        outlinerController.EnterEditMode(scene);

        //Assert
        Assert.IsTrue(Camera.main.GetComponent<BIWOutline>().enabled);
    }

    [Test]
    public void CheckCameraComponentRemoved()
    {
        //Act
        outlinerController.ExitEditMode();

        //Assert
        Assert.IsFalse(Camera.main.GetComponent<BIWOutline>().enabled);
    }

    protected override IEnumerator TearDown()
    {
        outlinerController.ExitEditMode();
        outlinerController.Dispose();
        entityHandler.Dispose();
        yield return base.TearDown();
    }
}