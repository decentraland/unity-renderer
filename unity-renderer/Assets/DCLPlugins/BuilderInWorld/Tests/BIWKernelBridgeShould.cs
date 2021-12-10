using System.Collections;
using System.Collections.Generic;
using DCL;
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

    [UnitySetUp]
    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();

        scene = TestUtils.CreateTestScene();

        entityHandler = new BIWEntityHandler();
        entityHandler.Initialize(BIWTestUtils.CreateMockedContextForTestScene());
        entityHandler.EnterEditMode(scene);

        biwBridge = MainSceneFactory.CreateBuilderInWorldBridge();

        WebInterface.OnMessageFromEngine += MessageReceived;
    }

    protected override IEnumerator TearDown()
    {
        Object.Destroy(biwBridge.gameObject);
        WebInterface.OnMessageFromEngine -= MessageReceived;
        yield return base.TearDown();
    }

    [Test]
    public void TestKernelPublishScene()
    {
        //Act
        biwBridge.PublishScene(scene, "Test title", "Test description", "test_screenshot");

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
        //Act
        biwBridge.StartKernelEditMode(scene);

        //Assert
        CheckMessageReceived();
    }

    [Test]
    public void TestEndStatefullScene()
    {
        //Act
        biwBridge.ExitKernelEditMode(scene);

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