using DCL;
using DCL.Components;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Helpers;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using DCL.Builder;
using Tests;
using UnityEngine;

public class BIWOutlinerShould : IntegrationTestSuite_Legacy
{
    private const long ENTITY_ID = 1;
    private BIWEntity entity;
    private BIWEntityHandler entityHandler;
    private BIWOutlinerController outlinerController;
    private IContext context;
    private IBuilderScene builderScene;
    private ParcelScene scene;
    private CoreComponentsPlugin coreComponentsPlugin;

    protected override List<GameObject> SetUp_LegacySystems()
    {
        List<GameObject> result = new List<GameObject>();
        result.Add(MainSceneFactory.CreateBridges());
        result.Add(MainSceneFactory.CreateEnvironment());
        result.AddRange(MainSceneFactory.CreatePlayerSystems());
        result.Add(MainSceneFactory.CreateNavMap());
        result.Add(MainSceneFactory.CreateAudioHandler());
        result.Add(MainSceneFactory.CreateHudController());
        result.Add(MainSceneFactory.CreateMouseCatcher());
        result.Add(MainSceneFactory.CreateEventSystem());
        return result;
    }

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();

        scene = TestUtils.CreateTestScene();
        coreComponentsPlugin = new CoreComponentsPlugin();
        BuilderInWorldPlugin.RegisterRuntimeComponents();

        TestUtils.CreateSceneEntity(scene, ENTITY_ID);

        TestUtils.CreateAndSetShape(scene, ENTITY_ID, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
            new
            {
                src = TestAssetsUtils.GetPath() + "/GLB/Trunk/Trunk.glb"
            }));

        LoadWrapper gltfShape = Environment.i.world.state.GetLoaderForEntity(scene.entities[ENTITY_ID]);
        yield return new DCL.WaitUntil(() => gltfShape.alreadyLoaded);

        outlinerController = new BIWOutlinerController();
        entityHandler = new BIWEntityHandler();

        context = BIWTestUtils.CreateContextWithGenericMocks(
            outlinerController,
            entityHandler
        );
        builderScene = BIWTestUtils.CreateBuilderSceneFromParcelScene(scene);

        outlinerController.Initialize(context);
        entityHandler.Initialize(context);

        entityHandler.EnterEditMode(builderScene);
        outlinerController.EnterEditMode(builderScene);

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
    public void CheckCameraComponentAdded()
    {
        //Act
        outlinerController.EnterEditMode(builderScene);

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
        BuilderInWorldPlugin.UnregisterRuntimeComponents();
        coreComponentsPlugin.Dispose();
        context.Dispose();
        yield return base.TearDown();
    }
}