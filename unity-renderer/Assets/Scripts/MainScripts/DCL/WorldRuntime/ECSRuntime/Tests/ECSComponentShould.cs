using System.Collections.Generic;
using System.Linq;
using DCL.Controllers;
using DCL.ECSRuntime;
using DCL.ECSRuntime.Tests;
using DCL.Models;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class ECSComponentShould
    {
        private IParcelScene scene;
        private IECSComponentHandler<TestingComponent> componentHandler;
        private ECSComponent<TestingComponent> component;

        [SetUp]
        public void SetUp()
        {
            scene = Substitute.For<IParcelScene>();
            componentHandler = Substitute.For<IECSComponentHandler<TestingComponent>>();
            component = new ECSComponent<TestingComponent>(scene, TestingComponentSerialization.Deserialize, () => componentHandler);
        }

        [Test]
        public void CreateComponent()
        {
            IDCLEntity entity = Substitute.For<IDCLEntity>();
            entity.entityId.Returns("1");

            component.Create(entity);

            componentHandler.Received(1).OnComponentCreated(scene, entity);
            Assert.AreEqual(1, component.entities.Count);
            Assert.AreEqual(entity.entityId, component.entities.FirstOrDefault().Key);
            Assert.IsTrue(component.HasComponent(entity));
        }

        [Test]
        public void RemoveComponent()
        {
            IDCLEntity entity = Substitute.For<IDCLEntity>();
            entity.entityId.Returns("1");

            component.Create(entity);
            component.Remove(entity);

            componentHandler.Received(1).OnComponentRemoved(scene, entity);
            Assert.AreEqual(0, component.entities.Count);
            Assert.IsFalse(component.HasComponent(entity));
        }

        [Test]
        public void UpdateComponent()
        {
            IDCLEntity entity = Substitute.For<IDCLEntity>();
            entity.entityId.Returns("1");

            var newComponentModel = new TestingComponent()
            {
                someString = "temptation",
                someVector = new Vector3(70, 0, -135)
            };
            var serializedModel = TestingComponentSerialization.Serialize(newComponentModel);

            component.Create(entity);
            component.Deserialize(entity, serializedModel);

            componentHandler.Received(1).OnComponentModelUpdated(scene, entity, Arg.Any<TestingComponent>());
            var componentData = component.Get(entity);
            Assert.AreEqual(newComponentModel.someString, componentData.model.someString);
            Assert.AreEqual(newComponentModel.someVector, componentData.model.someVector);
        }

        [Test]
        public void HandleSeveralEntities()
        {
            const int entitiesCount = 31;
            List<IDCLEntity> trackedEntities = new List<IDCLEntity>();
            Dictionary<IDCLEntity, TestingComponent> trackedModels = new Dictionary<IDCLEntity, TestingComponent>();

            for (int i = 0; i < entitiesCount; i++)
            {
                IDCLEntity entity = Substitute.For<IDCLEntity>();
                entity.entityId.Returns($"{i}");

                var entityModel = new TestingComponent()
                {
                    someString = $"temptation{i}",
                    someVector = new Vector3(70 + i, i, 0)
                };
                trackedEntities.Add(entity);
                trackedModels.Add(entity, entityModel);

                component.Create(entity);
                component.SetModel(entity, entityModel);
            }

            componentHandler.Received(entitiesCount).OnComponentCreated(scene, Arg.Any<IDCLEntity>());
            componentHandler.Received(entitiesCount).OnComponentModelUpdated(scene, Arg.Any<IDCLEntity>(), Arg.Any<TestingComponent>());

            for (int i = entitiesCount - 2; i > 3; i--)
            {
                IDCLEntity entity = trackedEntities[i];
                component.Remove(entity);
                trackedEntities.RemoveAt(i);
                trackedModels.Remove(entity);
            }

            Assert.AreEqual(trackedEntities.Count, component.entities.Count);

            using (var iterator = component.Get())
            {
                IDCLEntity currentEntity = null;
                while (iterator.MoveNext())
                {
                    if (currentEntity != null)
                    {
                        Assert.AreNotEqual(currentEntity, iterator.Current.entity);
                    }
                    Assert.IsNotNull(iterator.Current.entity);

                    currentEntity = iterator.Current.entity;
                    Assert.AreEqual(trackedModels[currentEntity].someString, iterator.Current.model.someString);
                    Assert.AreEqual(trackedModels[currentEntity].someVector, iterator.Current.model.someVector);
                }
            }
        }
    }
}