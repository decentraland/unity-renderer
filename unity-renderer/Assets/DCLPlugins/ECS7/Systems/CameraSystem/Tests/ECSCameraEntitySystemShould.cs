using DCL;
using DCL.CameraTool;
using DCL.Configuration;
using DCL.Controllers;
using DCL.ECS7;
using DCL.ECS7.ComponentWrapper;
using DCL.ECS7.ComponentWrapper.Generic;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Helpers;
using DCL.Models;
using ECSSystems.CameraSystem;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;
using TestUtils;
using UnityEngine;
using CameraType = DCL.ECSComponents.CameraType;

namespace Tests
{
    public class ECSCameraEntitySystemShould
    {
        private Transform cameraTransform;
        private BaseList<IParcelScene> scenes;

        private WrappedComponentPool<IWrappedComponent<PBCameraMode>> cameraModePool;
        private WrappedComponentPool<IWrappedComponent<PBPointerLock>> pointerLockPool;
        private WrappedComponentPool<IWrappedComponent<ECSTransform>> transformPool;
        private ECSComponent<ECSTransform> transformComponent;
        private IReadOnlyDictionary<int, ComponentWriter> componentsWriter;
        private DualKeyValueSet<long, int, WriteData> outgoingMessages;

        [SetUp]
        public void SetUp()
        {
            scenes = new BaseList<IParcelScene>();
            scenes.Add(Substitute.For<IParcelScene>());

            scenes[0]
               .sceneData.Returns(new LoadParcelScenesMessage.UnityParcelScene()
                {
                    sceneNumber = 666, basePosition = new Vector2Int(1, 0)
                });

            var entities = new Dictionary<long, IDCLEntity>();
            scenes[0].entities.Returns(entities);

            outgoingMessages = new DualKeyValueSet<long, int, WriteData>();

            componentsWriter = new Dictionary<int, ComponentWriter>()
            {
                { 666, new ComponentWriter(outgoingMessages) }
            };

            cameraTransform = (new GameObject("GO")).transform;
            cameraTransform.position = new Vector3(ParcelSettings.PARCEL_SIZE, 0, 0);

            cameraModePool = new WrappedComponentPool<IWrappedComponent<PBCameraMode>>(1, () => new ProtobufWrappedComponent<PBCameraMode>(new PBCameraMode()));
            pointerLockPool = new WrappedComponentPool<IWrappedComponent<PBPointerLock>>(1, () => new ProtobufWrappedComponent<PBPointerLock>(new PBPointerLock()));
            transformPool = new WrappedComponentPool<IWrappedComponent<ECSTransform>>(1 * 2, () => new TransformWrappedComponent(new ECSTransform()));

            transformComponent = new ECSComponent<ECSTransform>(null, null);

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
            ECSCameraEntitySystem system = new ECSCameraEntitySystem(
                componentsWriter,
                cameraModePool,
                pointerLockPool,
                transformPool,
                transformComponent,
                scenes,
                DataStore.i.camera.transform,
                CommonScriptableObjects.worldOffset,
                CommonScriptableObjects.cameraMode);

            system.Update();

            outgoingMessages.Put_Called<ECSTransform>(
                SpecialEntityId.CAMERA_ENTITY,
                ComponentID.TRANSFORM,
                null);

            outgoingMessages.Clear_Calls();

            system.Update();

            outgoingMessages.Put_NotCalled(
                SpecialEntityId.CAMERA_ENTITY,
                ComponentID.TRANSFORM);

            system.Dispose();
        }

        [Test]
        public void SendTransformIfChanged()
        {
            ECSCameraEntitySystem system = new ECSCameraEntitySystem(
                componentsWriter,
                cameraModePool,
                pointerLockPool,
                transformPool,
                transformComponent,
                scenes,
                DataStore.i.camera.transform,
                CommonScriptableObjects.worldOffset,
                CommonScriptableObjects.cameraMode);

            system.Update();

            outgoingMessages.Put_Called<ECSTransform>(
                SpecialEntityId.CAMERA_ENTITY,
                ComponentID.TRANSFORM,
                x => x.position == Vector3.zero
            );

            outgoingMessages.Clear_Calls();

            cameraTransform.position = new Vector3(0, 0, 0);

            system.Update();

            outgoingMessages.Put_Called<ECSTransform>(SpecialEntityId.CAMERA_ENTITY,
                ComponentID.TRANSFORM,
                x => x.position == new Vector3(-ParcelSettings.PARCEL_SIZE, 0, 0)
            );

            system.Dispose();
        }

        [Test]
        public void SendCameraMode()
        {
            CommonScriptableObjects.cameraMode.Set(CameraMode.ModeId.FirstPerson);

            ECSCameraEntitySystem system = new ECSCameraEntitySystem(
                componentsWriter,
                cameraModePool,
                pointerLockPool,
                transformPool,
                transformComponent,
                scenes,
                DataStore.i.camera.transform,
                CommonScriptableObjects.worldOffset,
                CommonScriptableObjects.cameraMode);

            system.Update();

            outgoingMessages.Put_Called<PBCameraMode>(
                SpecialEntityId.CAMERA_ENTITY,
                ComponentID.CAMERA_MODE,
                x => x.Mode == CameraType.CtFirstPerson
            );

            outgoingMessages.Clear_Calls();

            CommonScriptableObjects.cameraMode.Set(CameraMode.ModeId.ThirdPerson);

            system.Update();

            outgoingMessages.Put_Called<PBCameraMode>(
                SpecialEntityId.CAMERA_ENTITY,
                ComponentID.CAMERA_MODE,
                x => x.Mode == CameraType.CtThirdPerson
            );

            system.Dispose();
        }

        [Test]
        public void SendPointerLock()
        {
            Utils.LockCursor();

            ECSCameraEntitySystem system = new ECSCameraEntitySystem(
                componentsWriter,
                cameraModePool,
                pointerLockPool,
                transformPool,
                transformComponent,
                scenes,
                DataStore.i.camera.transform,
                CommonScriptableObjects.worldOffset,
                CommonScriptableObjects.cameraMode);

            system.Update();

            outgoingMessages.Put_Called<PBPointerLock>(
                SpecialEntityId.CAMERA_ENTITY,
                ComponentID.POINTER_LOCK,
                x => x.IsPointerLocked
            );

            outgoingMessages.Clear_Calls();

            Utils.UnlockCursor();

            system.Update();

            outgoingMessages.Put_Called<PBPointerLock>(
                SpecialEntityId.CAMERA_ENTITY,
                ComponentID.POINTER_LOCK,
                x => !x.IsPointerLocked
            );

            system.Dispose();
        }
    }
}
