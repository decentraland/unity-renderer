using DCL.ECSRuntime;
using NSubstitute;
using NUnit.Framework;
using System;

namespace DCL.ECSComponents.UIAbstractElements.Tests
{
    [TestFixture][Category("Flaky")]
    public class UIElementRegisterBaseShould : UIComponentsShouldBase
    {
        private ECSComponentsFactory factory;
        private IECSComponentHandler<PBUiBackground> handler;
        private UIElementRegisterBase<PBUiBackground, IECSComponentHandler<PBUiBackground>, PBUiInputResult>.HandlerBuilder handlerBuilder;
        private IECSComponentWriter writer;
        private UIElementRegisterBase<PBUiBackground, IECSComponentHandler<PBUiBackground>, PBUiInputResult> register;

        private const int COMPONENT_ID = 1001;
        private const int RESULT_COMPONENT_ID = 10010;

        [SetUp]
        public void RegisterSetUp()
        {
            factory = Substitute.For<ECSComponentsFactory>();
            handler = Substitute.For<IECSComponentHandler<PBUiBackground>>();
            writer = Substitute.For<IECSComponentWriter>();
            handlerBuilder = (c, i) => handler;

            register = Substitute.For<UIElementRegisterBase<PBUiBackground, IECSComponentHandler<PBUiBackground>, PBUiInputResult>>
                (COMPONENT_ID, RESULT_COMPONENT_ID, factory, writer, internalUiContainer, handlerBuilder);
        }

        [Test]
        public void AddDeserializer()
        {
            factory.Received(1)
                   .AddOrReplaceComponent(COMPONENT_ID,
                        Arg.Any<Func<object, PBUiBackground>>(), default);

            Assert.IsTrue(factory.componentBuilders.TryGetValue(COMPONENT_ID, out var componentBuilder));
            Assert.AreEqual(typeof(ECSComponent<PBUiBackground>), componentBuilder().GetType());
        }

        [Test]
        public void AddResultDeserializer()
        {
            Assert.IsTrue(factory.componentBuilders.TryGetValue(RESULT_COMPONENT_ID, out var componentBuilder));
            Assert.AreEqual(typeof(ECSComponent<PBUiInputResult>), componentBuilder().GetType());
        }

        [Test]
        public void AddSerializer()
        {
            writer.Received(1)
                  .AddOrReplaceComponentSerializer<PBUiBackground>(COMPONENT_ID, ProtoSerialization.Serialize);
        }

        [Test]
        public void AddResultSerializer()
        {
            writer.Received(1)
                  .AddOrReplaceComponentSerializer<PBUiInputResult>(RESULT_COMPONENT_ID, ProtoSerialization.Serialize);
        }

        [Test]
        public void RemoveBuilder()
        {
            register.Dispose();
            Assert.IsFalse(factory.componentBuilders.ContainsKey(COMPONENT_ID));
        }

        [Test]
        public void RemoveSerializer()
        {
            register.Dispose();
            writer.Received(1).RemoveComponentSerializer(COMPONENT_ID);
        }

        [Test]
        public void RemoveResultBuilder()
        {
            register.Dispose();
            Assert.IsFalse(factory.componentBuilders.ContainsKey(RESULT_COMPONENT_ID));
        }

        [Test]
        public void RemoveResultSerializer()
        {
            register.Dispose();
            writer.Received(1).RemoveComponentSerializer(RESULT_COMPONENT_ID);
        }
    }
}
