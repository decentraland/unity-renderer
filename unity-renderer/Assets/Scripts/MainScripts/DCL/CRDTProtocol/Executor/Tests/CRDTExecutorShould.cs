using System;
using System.Collections.Generic;
using DCL;
using DCL.Controllers;
using DCL.CRDT;
using DCL.ECSRuntime;
using DCL.Models;
using NSubstitute;
using NUnit.Framework;

namespace Tests
{
    public class CRDTExecutorShould
    {
        public class ComponentString
        {
            public string value;
        }

        public class ComponentInt
        {
            public int value;
        }

        enum ComponentIds
        {
            COMPONENT_STRING,
            COMPONENT_INT
        }

        IECSComponentHandler<ComponentString> stringCompHandler;
        IECSComponentHandler<ComponentInt> intCompHandler;

        [SetUp]
        public void SetUp()
        {
            stringCompHandler = Substitute.For<IECSComponentHandler<ComponentString>>();
            intCompHandler = Substitute.For<IECSComponentHandler<ComponentInt>>();

            DataStore.i.ecs7.componentsFactory.AddOrReplaceComponent(
                (int)ComponentIds.COMPONENT_STRING,
                data => new ComponentString() { value = (string)data },
                () => stringCompHandler);

            DataStore.i.ecs7.componentsFactory.AddOrReplaceComponent(
                (int)ComponentIds.COMPONENT_INT,
                data => new ComponentInt() { value = (int)data },
                () => intCompHandler);
        }

        [TearDown]
        public void TearDown()
        {
            DataStore.Clear();
        }

        [Test]
        public void CreateEntity()
        {
            const int ENTITY_ID = 42;
            IParcelScene scene = CreateScene("temptation");
            CRDTExecutor executor = new CRDTExecutor(scene);
            CRDTMessage addComponentMessage = new CRDTMessage()
            {
                key1 = ENTITY_ID,
                key2 = (int)ComponentIds.COMPONENT_STRING,
                data = "tigre"
            };

            executor.Execute(addComponentMessage);

            IDCLEntity createdEntity = scene.GetEntityById(ENTITY_ID);
            Assert.NotNull(createdEntity);
            stringCompHandler.Received(1).OnComponentCreated(scene, createdEntity);
            stringCompHandler.Received(1)
                             .OnComponentModelUpdated(scene, createdEntity,
                                 Arg.Do<ComponentString>(m => Assert.AreEqual("tigre", m.value)));
        }

        [Test]
        public void RemoveEntity()
        {
            const int ENTITY_ID = 42;
            IParcelScene scene = CreateScene("temptation");
            CRDTExecutor executor = new CRDTExecutor(scene);

            CRDTMessage addComponentMessage = new CRDTMessage()
            {
                key1 = ENTITY_ID,
                key2 = (int)ComponentIds.COMPONENT_STRING,
                data = "",
                timestamp = 0
            };
            CRDTMessage removeComponentMessage = new CRDTMessage()
            {
                key1 = ENTITY_ID,
                key2 = (int)ComponentIds.COMPONENT_STRING,
                data = null,
                timestamp = 1
            };

            executor.Execute(addComponentMessage);

            IDCLEntity createdEntity = scene.GetEntityById(ENTITY_ID);
            Assert.NotNull(createdEntity);
            var removeSubscriber = Substitute.For<IDummyEventSubscriber<IDCLEntity>>();
            createdEntity.OnRemoved += removeSubscriber.React;

            executor.Execute(removeComponentMessage);

            stringCompHandler.Received(1).OnComponentRemoved(scene, createdEntity);
            removeSubscriber.Received(1).React(createdEntity);
        }

        [Test]
        public void NotRemoveEntityIfHasComponentLeft()
        {
            const int ENTITY_ID = 42;
            IParcelScene scene = CreateScene("temptation");
            CRDTExecutor executor = new CRDTExecutor(scene);

            CRDTMessage addComponentString = new CRDTMessage()
            {
                key1 = ENTITY_ID,
                key2 = (int)ComponentIds.COMPONENT_STRING,
                data = "",
                timestamp = 0
            };
            CRDTMessage addComponentInt = new CRDTMessage()
            {
                key1 = ENTITY_ID,
                key2 = (int)ComponentIds.COMPONENT_INT,
                data = 1,
                timestamp = 0
            };

            executor.Execute(addComponentString);
            executor.Execute(addComponentInt);

            IDCLEntity createdEntity = scene.GetEntityById(ENTITY_ID);
            Assert.NotNull(createdEntity);
            var removeSubscriber = Substitute.For<IDummyEventSubscriber<IDCLEntity>>();
            createdEntity.OnRemoved += removeSubscriber.React;

            CRDTMessage removeComponentString = new CRDTMessage()
            {
                key1 = ENTITY_ID,
                key2 = (int)ComponentIds.COMPONENT_STRING,
                data = null,
                timestamp = 1
            };
            CRDTMessage removeComponentInt = new CRDTMessage()
            {
                key1 = ENTITY_ID,
                key2 = (int)ComponentIds.COMPONENT_INT,
                data = null,
                timestamp = 1
            };

            executor.Execute(removeComponentInt);

            intCompHandler.Received(1).OnComponentRemoved(scene, createdEntity);
            stringCompHandler.DidNotReceive().OnComponentRemoved(scene, createdEntity);
            removeSubscriber.DidNotReceive().React(createdEntity);

            executor.Execute(removeComponentString);

            stringCompHandler.Received(1).OnComponentRemoved(scene, createdEntity);
            removeSubscriber.Received(1).React(createdEntity);
        }

        [Test]
        public void RemoveComponentsIfEntityRemoved()
        {
            const int ENTITY_ID = 42;
            IParcelScene scene = CreateScene("temptation");
            CRDTExecutor executor = new CRDTExecutor(scene);

            CRDTMessage addComponentString = new CRDTMessage()
            {
                key1 = ENTITY_ID,
                key2 = (int)ComponentIds.COMPONENT_STRING,
                data = "",
                timestamp = 0
            };
            CRDTMessage addComponentInt = new CRDTMessage()
            {
                key1 = ENTITY_ID,
                key2 = (int)ComponentIds.COMPONENT_INT,
                data = 1,
                timestamp = 0
            };

            executor.Execute(addComponentString);
            executor.Execute(addComponentInt);

            IDCLEntity createdEntity = scene.GetEntityById(ENTITY_ID);

            scene.RemoveEntity(ENTITY_ID);

            stringCompHandler.Received(1).OnComponentRemoved(scene, createdEntity);
            intCompHandler.Received(1).OnComponentRemoved(scene, createdEntity);
        }

        [Test]
        public void TestSanityCheck()
        {
            IParcelScene scene = CreateScene("temptation");

            IDCLEntity entity = scene.GetEntityById(0);
            Assert.IsNull(entity);
            entity = scene.CreateEntity(0);
            Assert.NotNull(entity);
            var removeSubscriber = Substitute.For<IDummyEventSubscriber<IDCLEntity>>();
            entity.OnRemoved += removeSubscriber.React;
            removeSubscriber.DidNotReceive().React(Arg.Any<IDCLEntity>());
            scene.RemoveEntity(0);
            removeSubscriber.Received(1).React(entity);
            entity = scene.GetEntityById(0);
            Assert.IsNull(entity);

            scene.CreateEntity(4);
            scene.CreateEntity(2);
            IDCLEntity entity0 = scene.GetEntityById(4);
            IDCLEntity entity1 = scene.GetEntityById(2);
            Assert.NotNull(entity0);
            Assert.NotNull(entity1);
            Assert.AreNotEqual(entity0, entity1);
            var removeSubscriber0 = Substitute.For<IDummyEventSubscriber<IDCLEntity>>();
            var removeSubscriber1 = Substitute.For<IDummyEventSubscriber<IDCLEntity>>();
            entity0.OnRemoved += removeSubscriber0.React;
            entity1.OnRemoved += removeSubscriber1.React;
            scene.RemoveEntity(4);
            scene.RemoveEntity(2);
            removeSubscriber0.Received(1).React(entity0);
            removeSubscriber1.Received(1).React(entity1);
        }

        private static IParcelScene CreateScene(string id)
        {
            IParcelScene scene = Substitute.For<IParcelScene>();
            scene.sceneData.Returns(new LoadParcelScenesMessage.UnityParcelScene()
            {
                id = id
            });

            Dictionary<long, IDCLEntity> entities = new Dictionary<long, IDCLEntity>();

            // GetEntityById
            scene.GetEntityById(Arg.Any<long>())
                 .Returns(info =>
                 {
                     entities.TryGetValue(info.ArgAt<long>(0), out IDCLEntity entity);
                     return entity;
                 });

            // RemoveEntity
            scene.WhenForAnyArgs(x => x.RemoveEntity(Arg.Any<long>()))
                 .Do(info =>
                 {
                     long entityId = info.ArgAt<long>(0);
                     if (entities.TryGetValue(entityId, out IDCLEntity entity))
                     {
                         entities.Remove(entityId);
                         entity.OnRemoved?.Invoke(entity);
                     }
                 });

            // CreateEntity
            scene.CreateEntity(Arg.Any<long>())
                 .Returns(info =>
                 {
                     Action<IDCLEntity> onEntityRemoved = null;
                     long entityId = info.ArgAt<long>(0);
                     IDCLEntity entity = Substitute.For<IDCLEntity>();
                     entity.entityId.Returns(entityId);
                     entity.OnRemoved.Returns(callInfo => onEntityRemoved);
                     entities.Add(entityId, entity);
                     return entity;
                 });

            return scene;
        }
    }
}