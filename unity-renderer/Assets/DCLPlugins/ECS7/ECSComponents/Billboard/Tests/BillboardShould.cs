﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DCL.Configuration;
using DCL.Controllers;
using DCL.ECS7;
using DCL.ECSRuntime;
using DCL.Models;
using ECSSystems.BillboardSystem;
using Google.Protobuf;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using Tests;
using UnityEngine;
using UnityEngine.TestTools;

namespace DCL.ECSComponents.Test
{
    public class BillboardShould
    {
        private IDCLEntity entity;
        private GameObject gameObject;
        
        private Transform cameraTransform;
        private IList<IParcelScene> scenes;
        private BillboardRegister billboardRegister;
        private ECSBillboardSystem systemUpdate;
        private DataStore_Camera dataStoreCamera;
        private ECSComponent<PBBillboard> billboards;

        [SetUp]
        protected void SetUp()
        {
            scenes = DataStore.i.ecs7.scenes;
            scenes.Add(Substitute.For<IParcelScene>());
            scenes[0]
                .sceneData.Returns(new LoadParcelScenesMessage.UnityParcelScene()
                {
                    id = "temptation", basePosition = new Vector2Int(1, 0)
                });

            cameraTransform = (new GameObject("GO")).transform;
            cameraTransform.position = new UnityEngine.Vector3(ParcelSettings.PARCEL_SIZE, 0, 0);
            
            CommonScriptableObjects.rendererState.Set(true);
            CommonScriptableObjects.worldOffset.Set(UnityEngine.Vector3.zero);
            
            gameObject = new GameObject();
            entity = Substitute.For<IDCLEntity>();

            entity.entityId.Returns(1); 
            entity.gameObject.Returns(gameObject);

            ECSComponentsFactory componentFactory = new ECSComponentsFactory();
            ECSComponentsManager componentsManager = new ECSComponentsManager(componentFactory.componentBuilders);
            var internalComponents = new InternalECSComponents(componentsManager, componentFactory);
            var componentsComposer = new ECS7ComponentsComposer(componentFactory,
                Substitute.For<IECSComponentWriter>(), internalComponents);
            
            billboards = (ECSComponent<PBBillboard>)componentsManager.GetOrCreateComponent(ComponentID.BILLBOARD);
            
            dataStoreCamera = new DataStore_Camera();
            dataStoreCamera.transform.Set(cameraTransform);
            
            systemUpdate = new ECSBillboardSystem(
                billboards,
                dataStoreCamera);
        }

        [TearDown]
        protected void TearDown()
        {
            // componentHandler.OnComponentRemoved(scene, entity);
            GameObject.Destroy(gameObject);
        }

        [Test]
        public void UpdateComponentCorrectly()
        {
            billboards.Create(scenes[0], entity);
            billboards.SetModel(scenes[0], entity, CreateModel());
            
            var currentRotation = gameObject.transform.rotation;
            cameraTransform.position = new UnityEngine.Vector3(30, 2, 15);
            systemUpdate.Update();
            
            // Assert
            Assert.AreNotEqual(currentRotation,  gameObject.transform.rotation);
        }

        [Test]
        public void SerializeAndDeserialzeCorrectly()
        {
            // Arrange
            var model = CreateModel();

            // Act
            var newModel = SerializaAndDeserialize(model);

            // Assert
            Assert.AreEqual(model.BillboardMode, newModel.BillboardMode);
            Assert.AreEqual(model.OppositeDirection, newModel.OppositeDirection);
        }

        private PBBillboard SerializaAndDeserialize(PBBillboard pb)
        {
            var serialized = BillboardSerializer.Serialize(pb);
            return BillboardSerializer.Deserialize(serialized);
        }

        private PBBillboard CreateModel()
        {
            PBBillboard model = new PBBillboard();
            model.BillboardMode = BillboardMode.BmYAxe;
            model.OppositeDirection = true;
            return model;
        }
    }
}
