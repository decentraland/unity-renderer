using System;
using DCL.Components;
using DCL.Controllers;
using DCL.Models;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Tests
{
    public class CameraModeAreaShould
    {
        private IParcelScene scene;
        private GameObject player;
        private BoxCollider playerCollider;

        [SetUp]
        public void SetUp()
        {
            scene = Substitute.For<IParcelScene>();
            scene.sceneData.Returns(new LoadParcelScenesMessage.UnityParcelScene()
            {
                id = "temptation"
            });

            player = new GameObject("Player")
            {
                layer = LayerMask.NameToLayer("AvatarTriggerDetection")
            };

            playerCollider = player.AddComponent<BoxCollider>();
            playerCollider.center = Vector3.zero;
            playerCollider.size = Vector3.one;

            CommonScriptableObjects.sceneID.Set(scene.sceneData.id);
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(player);
        }

        [Test]
        public void AddAndRemoveListenerFromUpdateEventHandler()
        {
            CameraModeArea component = CreateArea(Vector3.zero, new Vector3(10, 10, 10), CameraMode.ModeId.BuildingToolGodMode);
            component.updateEventHandler.Received(1).AddListener(Arg.Any<IUpdateEventHandler.EventType>(), Arg.Any<Action>());

            Object.Destroy(component.areaEntity.gameObject);
            component.Dispose();
            component.updateEventHandler.Received(1).RemoveListener(Arg.Any<IUpdateEventHandler.EventType>(), Arg.Any<Action>());
        }

        [Test]
        public void ChangeModeOnAreaEnterAndExit()
        {
            Vector3 areaPosition = new Vector3(100, 0, 100);
            const CameraMode.ModeId targetMode = CameraMode.ModeId.BuildingToolGodMode;

            CameraModeArea component = CreateArea(areaPosition, new Vector3(10, 10, 10), targetMode);

            CameraMode.ModeId initialMode = CommonScriptableObjects.cameraMode.Get();

            // player outside area
            component.Update();
            Assert.IsFalse(component.isPlayerInside);
            Assert.AreEqual(initialMode, CommonScriptableObjects.cameraMode.Get());

            // player inside area
            SetPlayerPosition(areaPosition);
            component.Update();
            Assert.IsTrue(component.isPlayerInside);
            Assert.AreEqual(targetMode, CommonScriptableObjects.cameraMode.Get());

            // player outside area
            SetPlayerPosition(Vector3.zero);
            component.Update();
            Assert.IsFalse(component.isPlayerInside);
            Assert.AreEqual(initialMode, CommonScriptableObjects.cameraMode.Get());

            Object.Destroy(component.areaEntity.gameObject);
            component.Dispose();
        }

        [Test]
        public void ChangeModeWhenExitScene()
        {
            Vector3 areaPosition = new Vector3(100, 0, 100);
            const CameraMode.ModeId targetMode = CameraMode.ModeId.BuildingToolGodMode;

            CameraModeArea component = CreateArea(areaPosition, new Vector3(10, 10, 10), targetMode);

            CameraMode.ModeId initialMode = CommonScriptableObjects.cameraMode.Get();

            // player inside area
            SetPlayerPosition(areaPosition);
            component.Update();
            Assert.IsTrue(component.isPlayerInside);
            Assert.AreEqual(targetMode, CommonScriptableObjects.cameraMode.Get());

            // player outside scene
            CommonScriptableObjects.sceneID.Set("not-temptation");
            component.Update();
            Assert.IsFalse(component.isPlayerInside);
            Assert.AreEqual(initialMode, CommonScriptableObjects.cameraMode.Get());

            Object.Destroy(component.areaEntity.gameObject);
            component.Dispose();
        }

        [Test]
        public void WorkCorrectlyOnSizeChanged()
        {
            Vector3 areaPosition = new Vector3(100, 0, 100);
            const CameraMode.ModeId targetMode = CameraMode.ModeId.BuildingToolGodMode;

            CameraModeArea component = CreateArea(areaPosition, new Vector3(10, 10, 10), targetMode);

            CameraMode.ModeId initialMode = CommonScriptableObjects.cameraMode.Get();

            // player inside area
            SetPlayerPosition(areaPosition - new Vector3(component.areaModel.area.box.x * 0.5f, 0, 0));
            component.Update();
            Assert.IsTrue(component.isPlayerInside);
            Assert.AreEqual(targetMode, CommonScriptableObjects.cameraMode.Get());

            //update size to leave player outside area
            component.OnModelUpdated(new CameraModeArea.Model()
            {
                area = new CameraModeArea.Model.Area() { box = new Vector3(8, 10, 10) },
                cameraMode = CameraMode.ModeId.BuildingToolGodMode
            });
            component.Update();
            Assert.IsFalse(component.isPlayerInside);
            Assert.AreEqual(initialMode, CommonScriptableObjects.cameraMode.Get());

            Object.Destroy(component.areaEntity.gameObject);
            component.Dispose();
        }

        [Test]
        public void WorkCorrectlyOnAreaTransformChanged()
        {
            Vector3 areaPosition = new Vector3(100, 0, 100);
            const CameraMode.ModeId targetMode = CameraMode.ModeId.BuildingToolGodMode;

            CameraModeArea component = CreateArea(areaPosition, new Vector3(10, 10, 10), targetMode);

            CameraMode.ModeId initialMode = CommonScriptableObjects.cameraMode.Get();

            // player inside area
            SetPlayerPosition(areaPosition - new Vector3(component.areaModel.area.box.x * 0.5f, 0, 0));
            component.Update();
            Assert.IsTrue(component.isPlayerInside);
            Assert.AreEqual(targetMode, CommonScriptableObjects.cameraMode.Get());

            //move area to leave player outside
            Transform areaT = component.areaEntity.gameObject.transform;
            areaT.position += Vector3.right * 2;
            component.Update();
            Assert.IsFalse(component.isPlayerInside);
            Assert.AreEqual(initialMode, CommonScriptableObjects.cameraMode.Get());

            Object.Destroy(component.areaEntity.gameObject);
            component.Dispose();
        }

        [Test]
        public void WorkCorrectlyWithAreasInsideAreas()
        {
            Vector3 bigAreaSize = new Vector3(10, 1, 1);
            Vector3 bigAreaPos = Vector3.zero;
            const CameraMode.ModeId bigAreaMode = CameraMode.ModeId.BuildingToolGodMode;

            Vector3 mediumAreaSize = new Vector3(5, 1, 1);
            Vector3 mediumAreaPos = new Vector3(2, 0, 0);
            const CameraMode.ModeId mediumAreaMode = CameraMode.ModeId.FirstPerson;

            Vector3 smallAreaSize = new Vector3(1, 1, 1);
            Vector3 smallAreaPos = new Vector3(4, 0, 0);
            const CameraMode.ModeId smallAreaMode = CameraMode.ModeId.ThirdPerson;

            CameraModeArea bigArea = CreateArea(bigAreaPos, bigAreaSize, bigAreaMode);
            CameraModeArea mediumArea = CreateArea(mediumAreaPos, mediumAreaSize, mediumAreaMode);
            CameraModeArea smallArea = CreateArea(smallAreaPos, smallAreaSize, smallAreaMode);

            CameraMode.ModeId initialMode = CommonScriptableObjects.cameraMode.Get();

            Object.Destroy(bigArea.areaEntity.gameObject);
            Object.Destroy(mediumArea.areaEntity.gameObject);
            Object.Destroy(smallArea.areaEntity.gameObject);

            bigArea.Dispose();
            mediumArea.Dispose();
            smallArea.Dispose();
        }

        private CameraModeArea CreateArea(Vector3 position, Vector3 size, CameraMode.ModeId mode)
        {
            GameObject entityGO = new GameObject("Entity");
            entityGO.transform.position = position;

            IDCLEntity entity = Substitute.For<IDCLEntity>();
            entity.gameObject.Returns(entityGO);

            CameraModeArea component = new CameraModeArea();
            component.Initialize(scene, entity, Substitute.For<IUpdateEventHandler>(), playerCollider);
            component.OnModelUpdated(new CameraModeArea.Model()
            {
                cameraMode = mode,
                area = new CameraModeArea.Model.Area()
                {
                    box = size
                }
            });

            return component;
        }

        private void SetPlayerPosition(Vector3 position)
        {
            player.transform.position = position;
            Physics.SyncTransforms();
        }
    }
}