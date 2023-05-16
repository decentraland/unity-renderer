using DCL;
using DCL.CRDT;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Interface;
using ECSSystems.PointerInputSystem;
using NSubstitute;
using NUnit.Framework;
using RPC.Context;
using System;
using System.Collections.Generic;
using UnityEngine;
using Environment = DCL.Environment;

namespace Tests
{
    public class ECSPointerInputSystemShould
    {
        private Action systemUpdate;

        private DataStore_ECS7 dataStoreEcs7;
        private RestrictedActionsContext restrictedActionsRpcContext;

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

            restrictedActionsRpcContext = new RestrictedActionsContext();
            restrictedActionsRpcContext.LastFrameWithInput = -1;

            ECSPointerInputSystem system = new ECSPointerInputSystem(
                internalComponents.onPointerColliderComponent,
                internalComponents.inputEventResultsComponent,
                internalComponents.PointerEventsComponent,
                Substitute.For<IECSInteractionHoverCanvas>(),
                worldState,
                dataStoreEcs7,
                restrictedActionsRpcContext);

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

        [Test]
        [TestCase(InputAction.IaAction3, ExpectedResult = true)]
        [TestCase(InputAction.IaAction4, ExpectedResult = true)]
        [TestCase(InputAction.IaAction5, ExpectedResult = true)]
        [TestCase(InputAction.IaAction6, ExpectedResult = true)]
        [TestCase(InputAction.IaPrimary, ExpectedResult = true)]
        [TestCase(InputAction.IaSecondary, ExpectedResult = true)]
        [TestCase(InputAction.IaPointer, ExpectedResult = true)]
        [TestCase(InputAction.IaAny, ExpectedResult = false)]
        [TestCase(InputAction.IaForward, ExpectedResult = false)]
        [TestCase(InputAction.IaBackward, ExpectedResult = false)]
        [TestCase(InputAction.IaRight, ExpectedResult = false)]
        [TestCase(InputAction.IaLeft, ExpectedResult = false)]
        [TestCase(InputAction.IaJump, ExpectedResult = false)]
        [TestCase(InputAction.IaWalk, ExpectedResult = false)]
        public bool SetCurrentFrameWhenInputActionIsValid(InputAction inputAction)
        {
            int currentFrame = Time.frameCount;
            dataStoreEcs7.inputActionState[(int)inputAction] = true;
            systemUpdate();
            return currentFrame == restrictedActionsRpcContext.LastFrameWithInput;
        }
    }
}
