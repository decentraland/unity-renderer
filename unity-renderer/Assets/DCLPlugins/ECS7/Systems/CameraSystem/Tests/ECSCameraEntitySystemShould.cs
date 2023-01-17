using DCL;
using DCL.CameraTool;
using DCL.Configuration;
using DCL.Controllers;
using DCL.ECS7;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Helpers;
using DCL.Models;
using ECSSystems.CameraSystem;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using CameraType = DCL.ECSComponents.CameraType;

namespace Tests
{
    public class ECSCameraEntitySystemShould
    {
        private Transform cameraTransform;
        private IECSComponentWriter componentsWriter;
        private IList<IParcelScene> scenes;

        [SetUp]
        public void SetUp()
        {
            scenes = DataStore.i.ecs7.scenes;
            scenes.Add(Substitute.For<IParcelScene>());

            scenes[0]
               .sceneData.Returns(new LoadParcelScenesMessage.UnityParcelScene()
                {
                    sceneNumber = 666, basePosition = new Vector2Int(1, 0)
                });

            componentsWriter = Substitute.For<IECSComponentWriter>();
            cameraTransform = (new GameObject("GO")).transform;
            cameraTransform.position = new Vector3(ParcelSettings.PARCEL_SIZE, 0, 0);

            CommonScriptableObjects.rendererState.Set(true);
            CommonScriptableObjects.worldOffset.Set(Vector3.zero);
            DataStore.i.camera.transform.Set(cameraTransform);
        }

        [TearDown]
        public void TearDown()
        {
            DataStore.Clear();
            CommonScriptableObjects.UnloadAll();
            Object.Destroy(cameraTransform.gameObject);
        }

        [Test]
        public void NotSendTransformIfNoChange()
        {
            ECSCameraEntitySystem system = new ECSCameraEntitySystem(componentsWriter, new PBCameraMode(), new PBPointerLock(),
                DataStore.i.ecs7.scenes, DataStore.i.camera.transform, CommonScriptableObjects.worldOffset, CommonScriptableObjects.cameraMode);

            system.Update();

            componentsWriter.Received(1)
                            .PutComponent(
                                 scenes[0].sceneData.sceneNumber,
                                 SpecialEntityId.CAMERA_ENTITY,
                                 ComponentID.TRANSFORM,
                                 Arg.Any<ECSTransform>());

            componentsWriter.ClearReceivedCalls();

            system.Update();

            componentsWriter.DidNotReceive()
                            .PutComponent(
                                 scenes[0].sceneData.sceneNumber,
                                 SpecialEntityId.CAMERA_ENTITY,
                                 ComponentID.TRANSFORM,
                                 Arg.Any<ECSTransform>());

            system.Dispose();
        }

        [Test]
        public void SendTransformIfChanged()
        {
            ECSCameraEntitySystem system = new ECSCameraEntitySystem(componentsWriter, new PBCameraMode(), new PBPointerLock(),
                DataStore.i.ecs7.scenes, DataStore.i.camera.transform, CommonScriptableObjects.worldOffset, CommonScriptableObjects.cameraMode);

            system.Update();

            componentsWriter.Received(1)
                            .PutComponent(
                                 scenes[0].sceneData.sceneNumber,
                                 SpecialEntityId.CAMERA_ENTITY,
                                 ComponentID.TRANSFORM,
                                 Arg.Is<ECSTransform>(x => x.position == Vector3.zero));

            componentsWriter.ClearReceivedCalls();

            cameraTransform.position = new Vector3(0, 0, 0);

            system.Update();

            componentsWriter.Received(1)
                            .PutComponent(
                                 scenes[0].sceneData.sceneNumber,
                                 SpecialEntityId.CAMERA_ENTITY,
                                 ComponentID.TRANSFORM,
                                 Arg.Is<ECSTransform>(x =>
                                     x.position == new Vector3(-ParcelSettings.PARCEL_SIZE, 0, 0)));

            system.Dispose();
        }

        [Test]
        public void SendCameraMode()
        {
            CommonScriptableObjects.cameraMode.Set(CameraMode.ModeId.FirstPerson);

            ECSCameraEntitySystem system = new ECSCameraEntitySystem(componentsWriter, new PBCameraMode(), new PBPointerLock(),
                DataStore.i.ecs7.scenes, DataStore.i.camera.transform, CommonScriptableObjects.worldOffset, CommonScriptableObjects.cameraMode);

            system.Update();

            componentsWriter.Received(1)
                            .PutComponent(
                                 scenes[0].sceneData.sceneNumber,
                                 SpecialEntityId.CAMERA_ENTITY,
                                 ComponentID.CAMERA_MODE,
                                 Arg.Is<PBCameraMode>(x => x.Mode == CameraType.CtFirstPerson),
                                 ECSComponentWriteType.SEND_TO_SCENE);

            componentsWriter.ClearReceivedCalls();

            CommonScriptableObjects.cameraMode.Set(CameraMode.ModeId.ThirdPerson);

            system.Update();

            componentsWriter.Received(1)
                            .PutComponent(
                                 scenes[0].sceneData.sceneNumber,
                                 SpecialEntityId.CAMERA_ENTITY,
                                 ComponentID.CAMERA_MODE,
                                 Arg.Is<PBCameraMode>(x => x.Mode == CameraType.CtThirdPerson),
                                 ECSComponentWriteType.SEND_TO_SCENE);

            system.Dispose();
        }

        [Test]
        public void SendPointerLock()
        {
            Utils.LockCursor();

            ECSCameraEntitySystem system = new ECSCameraEntitySystem(componentsWriter, new PBCameraMode(), new PBPointerLock(),
                DataStore.i.ecs7.scenes, DataStore.i.camera.transform, CommonScriptableObjects.worldOffset, CommonScriptableObjects.cameraMode);

            system.Update();

            componentsWriter.Received(1)
                            .PutComponent(
                                 scenes[0].sceneData.sceneNumber,
                                 SpecialEntityId.CAMERA_ENTITY,
                                 ComponentID.POINTER_LOCK,
                                 Arg.Is<PBPointerLock>(x => x.IsPointerLocked),
                                 ECSComponentWriteType.SEND_TO_SCENE);

            componentsWriter.ClearReceivedCalls();

            Utils.UnlockCursor();

            system.Update();

            componentsWriter.Received(1)
                            .PutComponent(
                                 scenes[0].sceneData.sceneNumber,
                                 SpecialEntityId.CAMERA_ENTITY,
                                 ComponentID.POINTER_LOCK,
                                 Arg.Is<PBPointerLock>(x => !x.IsPointerLocked),
                                 ECSComponentWriteType.SEND_TO_SCENE);

            system.Dispose();
        }
    }
}
