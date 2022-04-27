using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Builder;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Interface;
using NUnit.Framework;
using Tests;
using UnityEngine;
using UnityEngine.TestTools;

public class BIWKernelBridgeShould : IntegrationTestSuite_Legacy
{
    private BuilderInWorldBridge biwBridge;
    private BIWEntityHandler entityHandler;
    private ParcelScene scene;

    private bool messageReceived = false;

    private CoreComponentsPlugin coreComponentsPlugin;

    [UnitySetUp]
    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();

        BuilderInWorldPlugin.RegisterRuntimeComponents();
        coreComponentsPlugin = new CoreComponentsPlugin();
        scene = TestUtils.CreateTestScene();

        entityHandler = new BIWEntityHandler();
        entityHandler.Initialize(BIWTestUtils.CreateMockedContextForTestScene());
        var builderScene = BIWTestUtils.CreateBuilderSceneFromParcelScene(scene);

        entityHandler.EnterEditMode(builderScene);

        biwBridge = MainSceneFactory.CreateBuilderInWorldBridge();

        WebInterface.OnMessageFromEngine += MessageReceived;
        
    }

    protected override IEnumerator TearDown()
    {
        coreComponentsPlugin.Dispose();
        BuilderInWorldPlugin.UnregisterRuntimeComponents();

        Object.Destroy(biwBridge.gameObject);
        WebInterface.OnMessageFromEngine -= MessageReceived;
        yield return base.TearDown();
    }

    [Test]
    public void TestKernelPublishScene()
    {
        //Arrange
        var sceneJson = new CatalystSceneEntityMetadata();
        sceneJson.scene = new CatalystSceneEntityMetadata.Scene();
        sceneJson.scene.parcels = new [] { "0,0"};
        
        //Act
        biwBridge.PublishScene(new Dictionary<string, object>(),new Dictionary<string, object>(), sceneJson,new StatelessManifest(),false);

        //Assert
        CheckMessageReceived();
    }

    [Test]
    public void TestCreateEntityKernelUpdate()
    {
        //Arrange
        BIWEntity entity =  entityHandler.CreateEmptyEntity(scene, Vector3.zero, Vector3.zero);

        //Act
        biwBridge.AddEntityOnKernel(entity.rootEntity, scene);

        //Assert
        CheckMessageReceived();
    }

    [Test]
    public void TestRemoveEntityKernelUpdate()
    {
        //Arrange
        BIWEntity entity =  entityHandler.CreateEmptyEntity(scene, Vector3.zero, Vector3.zero);

        //Act
        biwBridge.RemoveEntityOnKernel(entity.rootEntity.entityId, scene);

        //Assert
        CheckMessageReceived();
    }

    [Test]
    public void TestTransformKernelUpdate()
    {
        //Arrange
        BIWEntity entity =  entityHandler.CreateEmptyEntity(scene, Vector3.zero, Vector3.zero);

        //Act
        biwBridge.EntityTransformReport(entity.rootEntity, scene);

        //Assert
        CheckMessageReceived();
    }

    [Test]
    public void TestLockComponentKernelUpdate()
    {
        //Arrange
        BIWEntity entity =  entityHandler.CreateEmptyEntity(scene, Vector3.zero, Vector3.zero);
        entity.ToggleLockStatus();

        //Act
        biwBridge.ChangeEntityLockStatus(entity, scene);

        //Assert
        CheckMessageReceived();
    }

    [Test]
    public void TestNameComponentKernelUpdate()
    {
        //Arrange
        BIWEntity entity =  entityHandler.CreateEmptyEntity(scene, Vector3.zero, Vector3.zero);
        entity.SetDescriptiveName("Test");

        //Act
        biwBridge.ChangedEntityName(entity, scene);

        //Assert
        CheckMessageReceived();
    }

    [Test]
    public void TestSmartItemComponentKernelUpdate()
    {
        //Arrange
        BIWEntity entity =  BIWTestUtils.CreateSmartItemEntity(entityHandler, scene, null);

        //Act
        biwBridge.UpdateSmartItemComponent(entity, scene);

        //Assert
        CheckMessageReceived();
    }

    [Test]
    public void TestStartStatefullScene()
    {
        //Arrange
        ILand land = new ILand();
        land.sceneId = "ds";

        //Act
        biwBridge.StartIsolatedMode(land);

        //Assert
        CheckMessageReceived();
    }

    [Test]
    public void TestEndStatefullScene()
    {
        //Act
        biwBridge.StopIsolatedMode();

        //Assert
        CheckMessageReceived();
    }

    private void MessageReceived(string arg1, string arg2) { messageReceived = true; }

    private void CheckMessageReceived()
    {
        Assert.IsTrue(messageReceived);
        messageReceived = false;
    }
}