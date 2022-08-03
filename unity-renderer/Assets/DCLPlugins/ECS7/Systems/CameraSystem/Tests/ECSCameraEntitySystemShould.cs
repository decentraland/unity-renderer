using System.Collections.Generic;
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
using UnityEngine;

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
                    id = "temptation", basePosition = new Vector2Int(1, 0)
                });

            componentsWriter = Substitute.For<IECSComponentWriter>();
            cameraTransform = (new GameObject("GO")).transform;
            cameraTransform.position = new UnityEngine.Vector3(ParcelSettings.PARCEL_SIZE, 0, 0);

            CommonScriptableObjects.rendererState.Set(true);
            CommonScriptableObjects.worldOffset.Set(UnityEngine.Vector3.zero);
            DataStore.i.camera.transform.Set(cameraTransform);
        }

        [TearDown]
        public void TearDown()
        {
            DataStore.Clear();
            Object.Destroy(cameraTransform.gameObject);
        }

        [Test]
        public void NotSendTransformIfNoChange()
        {
            var update = ECSCameraEntitySystem.CreateSystem(componentsWriter);

            update.Invoke();
            componentsWriter.Received(1)
                            .PutComponent(
                                scenes[0].sceneData.id,
                                SpecialEntityId.CAMERA_ENTITY,
                                ComponentID.TRANSFORM,
                                Arg.Any<ECSTransform>());

            componentsWriter.ClearReceivedCalls();

            update.Invoke();
            componentsWriter.DidNotReceive()
                            .PutComponent(
                                scenes[0].sceneData.id,
                                SpecialEntityId.CAMERA_ENTITY,
                                ComponentID.TRANSFORM,
                                Arg.Any<ECSTransform>());
        }

        [Test]
        public void SendTransformIfChanged()
        {
            var update = ECSCameraEntitySystem.CreateSystem(componentsWriter);

            update.Invoke();
            componentsWriter.Received(1)
                            .PutComponent(
                                scenes[0].sceneData.id,
                                SpecialEntityId.CAMERA_ENTITY,
                                ComponentID.TRANSFORM,
                                Arg.Is<ECSTransform>(x => x.position == UnityEngine.Vector3.zero));

            componentsWriter.ClearReceivedCalls();

            cameraTransform.position = new UnityEngine.Vector3(0, 0, 0);

            update.Invoke();
            componentsWriter.Received(1)
                            .PutComponent(
                                scenes[0].sceneData.id,
                                SpecialEntityId.CAMERA_ENTITY,
                                ComponentID.TRANSFORM,
                                Arg.Is<ECSTransform>(x =>
                                    x.position == new UnityEngine.Vector3(-ParcelSettings.PARCEL_SIZE, 0, 0)));
        }

        [Test]
        public void SendCameraMode()
        {
            CommonScriptableObjects.cameraMode.Set(CameraMode.ModeId.FirstPerson);

            var update = ECSCameraEntitySystem.CreateSystem(componentsWriter);

            update.Invoke();
            componentsWriter.Received(1)
                            .PutComponent(
                                scenes[0].sceneData.id,
                                SpecialEntityId.CAMERA_ENTITY,
                                ComponentID.CAMERA_MODE,
                                Arg.Is<PBCameraMode>(x => x.Mode == CameraModeValue.FirstPerson),
                                ECSComponentWriteType.SEND_TO_SCENE);

            componentsWriter.ClearReceivedCalls();

            CommonScriptableObjects.cameraMode.Set(CameraMode.ModeId.ThirdPerson);

            update.Invoke();
            componentsWriter.Received(1)
                            .PutComponent(
                                scenes[0].sceneData.id,
                                SpecialEntityId.CAMERA_ENTITY,
                                ComponentID.CAMERA_MODE,
                                Arg.Is<PBCameraMode>(x => x.Mode == CameraModeValue.ThirdPerson),
                                ECSComponentWriteType.SEND_TO_SCENE);
        }

        [Test]
        public void SendPointerLock()
        {
            Utils.LockCursor();

            var update = ECSCameraEntitySystem.CreateSystem(componentsWriter);

            update.Invoke();
            componentsWriter.Received(1)
                            .PutComponent(
                                scenes[0].sceneData.id,
                                SpecialEntityId.CAMERA_ENTITY,
                                ComponentID.POINTER_LOCK,
                                Arg.Is<PBPointerLock>(x => x.IsPointerLocked),
                                ECSComponentWriteType.SEND_TO_SCENE);

            componentsWriter.ClearReceivedCalls();

            Utils.UnlockCursor();

            update.Invoke();
            componentsWriter.Received(1)
                            .PutComponent(
                                scenes[0].sceneData.id,
                                SpecialEntityId.CAMERA_ENTITY,
                                ComponentID.POINTER_LOCK,
                                Arg.Is<PBPointerLock>(x => !x.IsPointerLocked),
                                ECSComponentWriteType.SEND_TO_SCENE);
        }
    }
}