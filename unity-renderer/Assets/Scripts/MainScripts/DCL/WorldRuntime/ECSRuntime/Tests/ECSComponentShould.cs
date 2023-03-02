using System.Collections.Generic;
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
        private IParcelScene scene0;
        private IParcelScene scene1;
        private IECSComponentHandler<TestingComponent> componentHandler;
        private ECSComponent<TestingComponent> component;

        [SetUp]
        public void SetUp()
        {
            scene0 = Substitute.For<IParcelScene>();
            scene1 = Substitute.For<IParcelScene>();
            componentHandler = Substitute.For<IECSComponentHandler<TestingComponent>>();
            component = new ECSComponent<TestingComponent>(TestingComponentSerialization.Deserialize, () => componentHandler);
        }

        [Test]
        public void CreateComponent()
        {
            IDCLEntity entity0 = Substitute.For<IDCLEntity>();
            entity0.entityId.Returns(1);
            
            IDCLEntity entity1 = Substitute.For<IDCLEntity>();
            entity1.entityId.Returns(1);

            component.Create(scene0, entity0);
            component.Create(scene1, entity1);

            componentHandler.Received(1).OnComponentCreated(scene0, entity0);
            componentHandler.Received(1).OnComponentCreated(scene1, entity1);
            Assert.AreEqual(2, component.componentData.Count);
            Assert.IsTrue(component.HasComponent(scene0, entity0));
            Assert.IsTrue(component.HasComponent(scene1, entity1));
        }

        [Test]
        public void RemoveComponent()
        {
            IDCLEntity entity0 = Substitute.For<IDCLEntity>();
            entity0.entityId.Returns(1);
            
            IDCLEntity entity1 = Substitute.For<IDCLEntity>();
            entity1.entityId.Returns(1);

            component.Create(scene0, entity0);
            component.Remove(scene0, entity0);

            component.Create(scene1, entity1);

            componentHandler.Received(1).OnComponentRemoved(scene0, entity0);
            Assert.AreEqual(1, component.componentData.Count);
            Assert.IsFalse(component.HasComponent(scene0, entity0));
            
            component.Remove(scene1, entity1);
            componentHandler.Received(1).OnComponentRemoved(scene1, entity1);
            Assert.AreEqual(0, component.componentData.Count);
            Assert.IsFalse(component.HasComponent(scene1, entity1));
        }

        [Test]
        public void UpdateComponent()
        {
            IDCLEntity entity = Substitute.For<IDCLEntity>();
            entity.entityId.Returns(1);

            var newComponentModel = new TestingComponent()
            {
                someString = "temptation",
                someVector = new Vector3(70, 0, -135)
            };
            var serializedModel = TestingComponentSerialization.Serialize(newComponentModel);

            component.Create(scene0, entity);
            component.Deserialize(scene0, entity, serializedModel);

            componentHandler.Received(1).OnComponentModelUpdated(scene0, entity, Arg.Any<TestingComponent>());
            var componentData = component.Get(scene0, entity);
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
                entity.entityId.Returns(i);

                var entityModel = new TestingComponent()
                {
                    someString = $"temptation{i}",
                    someVector = new Vector3(70 + i, i, 0)
                };
                trackedEntities.Add(entity);
                trackedModels.Add(entity, entityModel);

                component.Create(scene0, entity);
                component.SetModel(scene0, entity, entityModel);
            }

            componentHandler.Received(entitiesCount).OnComponentCreated(scene0, Arg.Any<IDCLEntity>());
            componentHandler.Received(entitiesCount).OnComponentModelUpdated(scene0, Arg.Any<IDCLEntity>(), Arg.Any<TestingComponent>());

            for (int i = entitiesCount - 2; i > 3; i--)
            {
                IDCLEntity entity = trackedEntities[i];
                component.Remove(scene0, entity);
                trackedEntities.RemoveAt(i);
                trackedModels.Remove(entity);
            }

            Assert.AreEqual(trackedEntities.Count, component.componentData.Count);

            using (var iterator = component.componentData.GetEnumerator())
            {
                IDCLEntity currentEntity = null;
                while (iterator.MoveNext())
                {
                    if (currentEntity != null)
                    {
                        Assert.AreNotEqual(currentEntity, iterator.Current.value.entity);
                    }
                    Assert.IsNotNull(iterator.Current.value.entity);

                    currentEntity = iterator.Current.value.entity;
                    Assert.AreEqual(trackedModels[currentEntity].someString, iterator.Current.value.model.someString);
                    Assert.AreEqual(trackedModels[currentEntity].someVector, iterator.Current.value.model.someVector);
                }
            }
        }
    }
}