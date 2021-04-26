using Builder;
using Cinemachine;
using DCL;
using DCL.Components;
using DCL.Configuration;
using DCL.Helpers;
using DCL.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

public class BuilderInWorldShould : IntegrationTestSuite_Legacy
{
    [Test]
    public void GroundRaycast()
    {
        RaycastHit hit;
        BuilderInWorldController builderInWorldController = Resources.FindObjectsOfTypeAll<BuilderInWorldController>()[0];
        BuilderInWorldGodMode godMode = builderInWorldController.GetComponentInChildren<BuilderInWorldGodMode>(true);

        Vector3 fromPosition = new Vector3(0, 10, 0);
        Vector3 toPosition = Vector3.zero;
        Vector3 direction = toPosition - fromPosition;

        bool groundLayerFound = Physics.Raycast(fromPosition, direction, out hit, BuilderInWorldGodMode.RAYCAST_MAX_DISTANCE, godMode.groundLayer);

        UnityEngine.Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        if (Physics.Raycast(ray, out hit, BuilderInWorldGodMode.RAYCAST_MAX_DISTANCE, godMode.groundLayer))
        {
            groundLayerFound = true;
        }

        Assert.IsTrue(groundLayerFound, "The ground layer is not set to Ground");
    }

    [Test]
    public void SceneReferences()
    {
        BuilderInWorldController builderInWorldController = Resources.FindObjectsOfTypeAll<BuilderInWorldController>()[0];

        Assert.IsNotNull(builderInWorldController.cursorGO, "References on the builder-in-world prefab are null, check them all!");
        Assert.IsNotNull(builderInWorldController.inputController, "References on the builder-in-world prefab are null, check them all!");
        Assert.IsNotNull(builderInWorldController.cameraParentGO, "References on the builder-in-world prefab are null, check them all!");


        BuilderInWorldGodMode godMode = builderInWorldController.GetComponentInChildren<BuilderInWorldGodMode>();

        Assert.IsNotNull(godMode.avatarRenderer, "References on the builder-in-world prefab are null, check them all!");
        Assert.IsNotNull(godMode.mouseCatcher, "References on the builder-in-world god mode are null, check them all!");
        Assert.IsNotNull(godMode.cameraController, "References on the builder-in-world god mode are null, check them all!");
        Assert.IsNotNull(godMode.freeCameraController, "References on the builder-in-world god mode are null, check them all!");

        DCLBuilderRaycast dCLBuilderRaycast = godMode.GetComponentInChildren<DCLBuilderRaycast>();

        Assert.IsNotNull(dCLBuilderRaycast.builderCamera, "Camera reference on the builder-in-world god mode children are null, check them all!");

        VoxelController voxelController = godMode.GetComponent<VoxelController>();

        Assert.IsNotNull(voxelController.freeCameraMovement, "Camera reference on the builder-in-world voxel controller are null, check them all!");
    }

    [Test]
    public void BuilderInWorldEntityComponents()
    {
        string entityId = "1";
        TestHelpers.CreateSceneEntity(scene, entityId);

        DCLBuilderInWorldEntity biwEntity = Utils.GetOrCreateComponent<DCLBuilderInWorldEntity>(scene.entities[entityId].gameObject);
        biwEntity.Init(scene.entities[entityId], null);

        Assert.IsTrue(biwEntity.entityUniqueId == scene.sceneData.id + scene.entities[entityId].entityId, "Entity id is not created correctly, this can lead to weird behaviour");

        SmartItemComponent.Model model = new SmartItemComponent.Model();

        scene.EntityComponentCreateOrUpdateWithModel(entityId, CLASS_ID_COMPONENT.SMART_ITEM, model);

        Assert.IsTrue(biwEntity.HasSmartItemComponent());

        DCLName name = (DCLName) scene.SharedComponentCreate(Guid.NewGuid().ToString(), Convert.ToInt32(CLASS_ID.NAME));
        scene.SharedComponentAttach(biwEntity.rootEntity.entityId, name.id);

        DCLName dclName = biwEntity.rootEntity.TryGetComponent<DCLName>();
        Assert.IsNotNull(dclName);

        string newName = "TestingName";
        dclName.SetNewName(newName);
        Assert.AreEqual(newName, biwEntity.GetDescriptiveName());


        DCLLockedOnEdit entityLocked = (DCLLockedOnEdit) scene.SharedComponentCreate(Guid.NewGuid().ToString(), Convert.ToInt32(CLASS_ID.LOCKED_ON_EDIT));
        scene.SharedComponentAttach(biwEntity.rootEntity.entityId, entityLocked.id);

        DCLLockedOnEdit dclLockedOnEdit = biwEntity.rootEntity.TryGetComponent<DCLLockedOnEdit>();
        Assert.IsNotNull(dclLockedOnEdit);

        bool isLocked = true;
        dclLockedOnEdit.SetIsLocked(isLocked);
        Assert.AreEqual(biwEntity.IsLocked, isLocked);
    }

    protected override IEnumerator TearDown()
    {
        AssetCatalogBridge.i.ClearCatalog();
        BIWCatalogManager.ClearCatalog();
        yield return base.TearDown();
    }
}