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
    public class ECSComponentsManagerShould
    {
        enum ComponentsID
        {
            Component0,
            Component1
        }

        IParcelScene scene;
        IECSComponentHandler<TestingComponent> componentHandler0;
        IECSComponentHandler<TestingComponent> componentHandler1;
        ECSComponentsManager componentsManager;

        [SetUp]
        public void SetUp()
        {
            scene = Substitute.For<IParcelScene>();
            componentHandler0 = Substitute.For<IECSComponentHandler<TestingComponent>>();
            componentHandler1 = Substitute.For<IECSComponentHandler<TestingComponent>>();

            Dictionary<int, ECSComponentsFactory.ECSComponentBuilder> components =
                new Dictionary<int, ECSComponentsFactory.ECSComponentBuilder>()
                {
                    {
                        (int)ComponentsID.Component0,
                        ECSComponentsFactory.CreateComponentBuilder(TestingComponentSerialization.Deserialize, () => componentHandler0)
                    },
                    {
                        (int)ComponentsID.Component1,
                        ECSComponentsFactory.CreateComponentBuilder(TestingComponentSerialization.Deserialize, () => componentHandler1)
                    },
                };

            componentsManager = new ECSComponentsManager(components);
        }

        [Test]
        public void GetOrCreateComponent()
        {
            IDCLEntity entity = Substitute.For<IDCLEntity>();
            entity.entityId.Returns(1);

            IECSComponent comp0 = componentsManager.GetOrCreateComponent((int)ComponentsID.Component0, scene, entity);
            IECSComponent comp1 = componentsManager.GetOrCreateComponent((int)ComponentsID.Component1, scene, entity);

            componentHandler0.Received(1).OnComponentCreated(scene, entity);
            componentHandler1.Received(1).OnComponentCreated(scene, entity);

            componentHandler0.ClearReceivedCalls();
            componentHandler1.ClearReceivedCalls();

            Assert.NotNull(comp0);
            Assert.NotNull(comp1);
            Assert.AreNotEqual(comp0, comp1);

            Assert.IsTrue(comp0.HasComponent(scene, entity));
            Assert.IsTrue(comp1.HasComponent(scene, entity));

            IECSComponent comp0II = componentsManager.GetOrCreateComponent((int)ComponentsID.Component0, scene, entity);
            IECSComponent comp1II = componentsManager.GetOrCreateComponent((int)ComponentsID.Component1, scene, entity);

            componentHandler0.DidNotReceive().OnComponentCreated(scene, entity);
            componentHandler1.DidNotReceive().OnComponentCreated(scene, entity);

            Assert.NotNull(comp0II);
            Assert.NotNull(comp1II);

            Assert.AreEqual(comp0, comp0II);
            Assert.AreEqual(comp1, comp1II);

            Assert.AreEqual(comp0, componentsManager.loadedComponents[(int)ComponentsID.Component0]);
            Assert.AreEqual(comp1, componentsManager.loadedComponents[(int)ComponentsID.Component1]);

            Assert.AreEqual(2, componentsManager.loadedComponents.Count);
        }

        [Test]
        public void GetCreatedComponent()
        {
            IDCLEntity entity = Substitute.For<IDCLEntity>();
            entity.entityId.Returns(1);

            IECSComponent comp0 = componentsManager.GetOrCreateComponent((int)ComponentsID.Component0, scene, entity);
            IECSComponent comp1 = componentsManager.GetComponent((int)ComponentsID.Component0);

            Assert.AreEqual(comp0, comp1);
        }

        [Test]
        public void DeserializeComponent()
        {
            IDCLEntity entity = Substitute.For<IDCLEntity>();
            entity.entityId.Returns(1);

            var newComponentModel = new TestingComponent()
            {
                someString = "temptation",
                someVector = new Vector3(70, 0, -135)
            };
            var serializedModel = TestingComponentSerialization.Serialize(newComponentModel);

            componentsManager.DeserializeComponent((int)ComponentsID.Component0, scene, entity, serializedModel);

            componentHandler0.Received(1).OnComponentCreated(scene, entity);
            componentHandler0.Received(1).OnComponentModelUpdated(scene, entity, Arg.Any<TestingComponent>());

            Assert.AreEqual(1, componentsManager.loadedComponents.Count);

            ECSComponent<TestingComponent> typedComponent = ((ECSComponent<TestingComponent>)componentsManager.loadedComponents[(int)ComponentsID.Component0]);
            Assert.AreEqual(newComponentModel.someString, typedComponent.Get(scene, entity).model.someString);
            Assert.AreEqual(newComponentModel.someVector, typedComponent.Get(scene, entity).model.someVector);
            Assert.IsTrue(typedComponent.HasComponent(scene, entity));
        }

        [Test]
        public void RemoveComponent()
        {
            IDCLEntity entity = Substitute.For<IDCLEntity>();
            entity.entityId.Returns(1);

            componentsManager.GetOrCreateComponent((int)ComponentsID.Component0, scene, entity);
            componentsManager.GetOrCreateComponent((int)ComponentsID.Component1, scene, entity);

            componentsManager.RemoveComponent((int)ComponentsID.Component0, scene, entity);
            componentsManager.RemoveComponent((int)ComponentsID.Component1, scene, entity);

            componentHandler0.Received(1).OnComponentRemoved(scene, entity);
            componentHandler1.Received(1).OnComponentRemoved(scene, entity);

            ECSComponent<TestingComponent> typedComponent0 = ((ECSComponent<TestingComponent>)componentsManager.loadedComponents[(int)ComponentsID.Component0]);
            ECSComponent<TestingComponent> typedComponent1 = ((ECSComponent<TestingComponent>)componentsManager.loadedComponents[(int)ComponentsID.Component1]);

            Assert.IsFalse(typedComponent0.HasComponent(scene, entity));
            Assert.IsFalse(typedComponent1.HasComponent(scene, entity));

            Assert.AreEqual(0, typedComponent0.componentData.Count);
            Assert.AreEqual(0, typedComponent1.componentData.Count);
        }
    }
}