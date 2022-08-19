using DCL;
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
        private ECSComponentsManager componentsManager;
        private ECS7TestUtilsScenesAndEntities testUtils;

        [SetUp]
        public void SetUp()
        {
            stringCompHandler = Substitute.For<IECSComponentHandler<ComponentString>>();
            intCompHandler = Substitute.For<IECSComponentHandler<ComponentInt>>();

            ECSComponentsFactory componentsFactory = new ECSComponentsFactory();
            componentsFactory.AddOrReplaceComponent(
                (int)ComponentIds.COMPONENT_STRING,
                data => new ComponentString() { value = (string)data },
                () => stringCompHandler);

            componentsFactory.AddOrReplaceComponent(
                (int)ComponentIds.COMPONENT_INT,
                data => new ComponentInt() { value = (int)data },
                () => intCompHandler);

            componentsManager = new ECSComponentsManager(componentsFactory.componentBuilders);
            testUtils = new ECS7TestUtilsScenesAndEntities();
        }

        [TearDown]
        public void TearDown()
        {
            DataStore.Clear();
            testUtils.Dispose();
        }

        [Test]
        public void CreateEntity()
        {
            const int ENTITY_ID = 42;
            ECS7TestScene scene = testUtils.CreateScene("temptation");
            CRDTExecutor executor = new CRDTExecutor(scene, componentsManager);
            CRDTMessage addComponentMessage = new CRDTMessage()
            {
                key1 = ENTITY_ID,
                key2 = (int)ComponentIds.COMPONENT_STRING,
                data = "tigre"
            };

            executor.Execute(addComponentMessage);

            scene.entities.TryGetValue(ENTITY_ID, out IDCLEntity createdEntity);
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
            ECS7TestScene scene = testUtils.CreateScene("temptation");
            CRDTExecutor executor = new CRDTExecutor(scene, componentsManager);

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

            scene.entities.TryGetValue(ENTITY_ID, out IDCLEntity createdEntity);
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
            ECS7TestScene scene = testUtils.CreateScene("temptation");
            CRDTExecutor executor = new CRDTExecutor(scene, componentsManager);

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

            scene.entities.TryGetValue(ENTITY_ID, out IDCLEntity createdEntity);
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
            ECS7TestScene scene = testUtils.CreateScene("temptation");
            CRDTExecutor executor = new CRDTExecutor(scene, componentsManager);

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

            scene.entities.TryGetValue(ENTITY_ID, out IDCLEntity createdEntity);

            scene.RemoveEntity(ENTITY_ID);

            stringCompHandler.Received(1).OnComponentRemoved(scene, createdEntity);
            intCompHandler.Received(1).OnComponentRemoved(scene, createdEntity);
        }

        [Test]
        public void TestSanityCheck()
        {
            ECS7TestScene scene = testUtils.CreateScene("temptation");

            IDCLEntity entity = null;
            scene.entities.TryGetValue(0, out entity);
            Assert.IsNull(entity);
            entity = scene.CreateEntity(0);
            Assert.NotNull(entity);
            var removeSubscriber = Substitute.For<IDummyEventSubscriber<IDCLEntity>>();
            entity.OnRemoved += removeSubscriber.React;
            removeSubscriber.DidNotReceive().React(Arg.Any<IDCLEntity>());
            scene.RemoveEntity(0);
            removeSubscriber.Received(1).React(entity);
            scene.entities.TryGetValue(0, out entity);
            Assert.IsNull(entity);

            scene.CreateEntity(4);
            scene.CreateEntity(2);
            IDCLEntity entity0 = null;
            IDCLEntity entity1 = null;
            scene.entities.TryGetValue(4, out entity0);
            scene.entities.TryGetValue(2, out entity1);
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
    }
}