using System;
using DCL.CameraTool;
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
                sceneNumber = 666
            });

            player = new GameObject("Player")
            {
                layer = LayerMask.NameToLayer("AvatarTriggerDetection")
            };

            playerCollider = player.AddComponent<BoxCollider>();
            playerCollider.center = Vector3.zero;
            playerCollider.size = Vector3.one;

            CommonScriptableObjects.sceneNumber.Set(scene.sceneData.sceneNumber);
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
            component.updateEventHandler.Received(1).AddListener(Arg.Any<DCL.IUpdateEventHandler.EventType>(), Arg.Any<Action>());

            Object.Destroy(component.areaEntity.gameObject);
            component.Dispose();
            component.updateEventHandler.Received(1).RemoveListener(Arg.Any<DCL.IUpdateEventHandler.EventType>(), Arg.Any<Action>());
        }

        [Test]
        public void ValidateCameraMode()
        {
            Vector3 areaPosition = new Vector3(100, 0, 100);
            Vector3 areaSize = new Vector3(10, 10, 10);
            const CameraMode.ModeId targetMode = CameraMode.ModeId.BuildingToolGodMode;

            CameraModeArea component = CreateArea(areaPosition, areaSize, targetMode, 1 << (int)CameraMode.ModeId.FirstPerson);

            CameraMode.ModeId initialMode = CommonScriptableObjects.cameraMode.Get();

            // player inside area
            SetPlayerPosition(areaPosition);
            component.Update();
            Assert.IsTrue(component.isPlayerInside);
            Assert.AreNotEqual(targetMode, CommonScriptableObjects.cameraMode.Get());

            // enable mode and update component
            component.validCameraModes = (1 << (int)CameraMode.ModeId.BuildingToolGodMode);
            component.OnModelUpdated(new CameraModeArea.Model()
            {
                area = new CameraModeArea.Model.Area()
                {
                    box = areaSize
                },
                cameraMode = targetMode
            });

            Assert.IsTrue(component.isPlayerInside);
            Assert.AreEqual(targetMode, CommonScriptableObjects.cameraMode.Get());

            Object.Destroy(component.areaEntity.gameObject);
            component.Dispose();
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
            CommonScriptableObjects.sceneNumber.Set(5);
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
            Vector3 mediumAreaPos = (bigAreaPos + Vector3.right * bigAreaSize.x * 0.5f) + Vector3.left * mediumAreaSize.x * 0.5f;
            const CameraMode.ModeId mediumAreaMode = CameraMode.ModeId.FirstPerson;

            Vector3 smallAreaSize = new Vector3(1, 1, 1);
            Vector3 smallAreaPos = (bigAreaPos + Vector3.right * bigAreaSize.x * 0.5f) + Vector3.left * smallAreaSize.x * 0.5f;
            const CameraMode.ModeId smallAreaMode = CameraMode.ModeId.ThirdPerson;

            CameraModeArea bigArea = CreateArea(bigAreaPos, bigAreaSize, bigAreaMode);
            CameraModeArea mediumArea = CreateArea(mediumAreaPos, mediumAreaSize, mediumAreaMode);
            CameraModeArea smallArea = CreateArea(smallAreaPos, smallAreaSize, smallAreaMode);

            CameraMode.ModeId initialMode = CommonScriptableObjects.cameraMode.Get();

            // move player to the beginning of big area
            SetPlayerPosition(bigAreaPos + Vector3.left * bigAreaSize.x * 0.5f);
            bigArea.Update();
            mediumArea.Update();
            smallArea.Update();

            Assert.IsTrue(bigArea.isPlayerInside);
            Assert.IsFalse(mediumArea.isPlayerInside);
            Assert.IsFalse(smallArea.isPlayerInside);
            Assert.AreEqual(bigAreaMode, CommonScriptableObjects.cameraMode.Get());

            // move player to the beginning of medium area
            SetPlayerPosition(mediumAreaPos + Vector3.left * mediumAreaSize.x * 0.5f);
            bigArea.Update();
            mediumArea.Update();
            smallArea.Update();

            Assert.IsTrue(bigArea.isPlayerInside);
            Assert.IsTrue(mediumArea.isPlayerInside);
            Assert.IsFalse(smallArea.isPlayerInside);
            Assert.AreEqual(mediumAreaMode, CommonScriptableObjects.cameraMode.Get());

            // move player to the beginning of small area
            SetPlayerPosition(smallAreaPos + Vector3.left * smallAreaSize.x * 0.5f);
            bigArea.Update();
            mediumArea.Update();
            smallArea.Update();

            Assert.IsTrue(bigArea.isPlayerInside);
            Assert.IsTrue(mediumArea.isPlayerInside);
            Assert.IsTrue(smallArea.isPlayerInside);
            Assert.AreEqual(smallAreaMode, CommonScriptableObjects.cameraMode.Get());

            // move player outside all areas
            SetPlayerPosition(smallAreaPos + Vector3.right * (playerCollider.size.x + 1 + smallAreaSize.x * 0.5f));
            bigArea.Update();
            mediumArea.Update();
            smallArea.Update();

            Assert.IsFalse(bigArea.isPlayerInside);
            Assert.IsFalse(mediumArea.isPlayerInside);
            Assert.IsFalse(smallArea.isPlayerInside);
            Assert.AreEqual(initialMode, CommonScriptableObjects.cameraMode.Get());

            // cleanup test
            Object.Destroy(bigArea.areaEntity.gameObject);
            Object.Destroy(mediumArea.areaEntity.gameObject);
            Object.Destroy(smallArea.areaEntity.gameObject);

            bigArea.Dispose();
            mediumArea.Dispose();
            smallArea.Dispose();
        }

        private CameraModeArea CreateArea(Vector3 position, Vector3 size, CameraMode.ModeId mode, int validModeMask = -1)
        {
            GameObject entityGO = new GameObject("Entity");
            entityGO.transform.position = position;

            IDCLEntity entity = Substitute.For<IDCLEntity>();
            entity.gameObject.Returns(entityGO);

            CameraModeArea component = new CameraModeArea();
            component.Initialize(scene, entity, Substitute.For<DCL.IUpdateEventHandler>(), playerCollider);
            component.validCameraModes = validModeMask;
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