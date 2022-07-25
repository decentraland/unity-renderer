using System.Collections.Generic;
using DCL;
using DCL.Configuration;
using DCL.Controllers;
using DCL.ECS7;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using ECSSystems.CameraSystem;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class ECSCameraTransformSystemShould
    {
        private Transform cameraTransform;
        private IECSComponentWriter componentsWriter;
        private IList<IParcelScene> scenes;

        [SetUp]
        public void SetUp()
        {
            scenes = new[] { Substitute.For<IParcelScene>() };
            scenes[0]
                .sceneData.Returns(new LoadParcelScenesMessage.UnityParcelScene()
                {
                    id = "temptation", basePosition = new Vector2Int(1, 0)
                });

            DataStore.i.ecs7.scenes.AddRange(scenes);
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
            var update = ECSCameraTransformSystem.CreateSystem(componentsWriter);

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
            var update = ECSCameraTransformSystem.CreateSystem(componentsWriter);

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
    }
}