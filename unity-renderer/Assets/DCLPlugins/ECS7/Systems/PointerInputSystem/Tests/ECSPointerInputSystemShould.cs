using DCL;
using DCL.CRDT;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Interface;
using ECSSystems.PointerInputSystem;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Environment = DCL.Environment;

namespace Tests
{
    public class ECSPointerInputSystemShould
    {
        private Action systemUpdate;

        private DataStore_ECS7 dataStoreEcs7;

        [SetUp]
        public void SetUp()
        {
            Environment.Setup(ServiceLocatorTestFactory.CreateMocked());

            var componentsFactory = new ECSComponentsFactory();
            var componentsManager = new ECSComponentsManager(componentsFactory.componentBuilders);
            var executors = new Dictionary<int, ICRDTExecutor>();
            var internalComponents = new InternalECSComponents(componentsManager, componentsFactory, executors);

            var componentsComposer = new ECS7ComponentsComposer(componentsFactory,
                Substitute.For<IECSComponentWriter>(), internalComponents);

            var worldState = Substitute.For<IWorldState>();
            worldState.ContainsScene(Arg.Any<int>()).Returns(true);

            dataStoreEcs7 = new DataStore_ECS7();

            ECSPointerInputSystem system = new ECSPointerInputSystem(
                internalComponents.onPointerColliderComponent,
                internalComponents.inputEventResultsComponent,
                internalComponents.PointerEventsComponent,
                Substitute.For<IECSInteractionHoverCanvas>(),
                worldState,
                dataStoreEcs7);

            systemUpdate = system.Update;
        }

        [Test]
        public void EnsureWebInterfaceAndProtobufInputEnumsMatch()
        {
            var inputActionsWebInterface = Enum.GetValues(typeof(WebInterface.ACTION_BUTTON)) as int[];
            var inputActionsProto = Enum.GetValues(typeof(InputAction)) as int[];
            Assert.AreEqual(inputActionsProto!.Length, inputActionsWebInterface!.Length);

            for (var i = 0; i < inputActionsWebInterface.Length; i++)
            {
                Assert.AreEqual(inputActionsWebInterface[i], inputActionsProto[i]);
            }
        }
    }
}
