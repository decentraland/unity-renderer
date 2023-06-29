using DCL.CRDT;
using DCL.ECS7;
using DCL.ECS7.ComponentWrapper.Generic;
using System;

namespace TestUtils
{
    public static class ComponentWriterTestExtension
    {
        public static void Put_Called<T>(
            this DualKeyValueSet<long, int, WriteData> msgs,
            long expectedEntityId,
            int expectedComponentId,
            Func<T, bool> expresionComparer) where T: class
        {
            if (!AssertWriteMessage(msgs, expectedEntityId, expectedComponentId, CrdtMessageType.PUT_COMPONENT, expresionComparer))
            {
                throw new Exception($"Expected to receive a call, but a no call was received");
            }
        }

        public static void Put_NotCalled(
            this DualKeyValueSet<long, int, WriteData> msgs,
            long expectedEntityId,
            int expectedComponentId)
        {
            var pairs = msgs.Pairs;

            for (int i = 0; i < pairs.Count; i++)
            {
                if (pairs[i].key2 == expectedComponentId
                    && pairs[i].key1 == expectedEntityId
                    && pairs[i].value.MessageType == CrdtMessageType.PUT_COMPONENT)
                {
                    throw new Exception($"Expected not to receive a call, but a call was received");
                }
            }
        }

        public static void Append_Called<T>(
            this DualKeyValueSet<long, int, WriteData> msgs,
            long expectedEntityId,
            int expectedComponentId,
            Func<T, bool> expresionComparer) where T: class
        {
            if (!AssertWriteMessage(msgs, expectedEntityId, expectedComponentId, CrdtMessageType.APPEND_COMPONENT, expresionComparer))
            {
                throw new Exception($"Expected to receive a call, but a no call was received");
            }
        }

        public static void Append_NotCalled(
            this DualKeyValueSet<long, int, WriteData> msgs,
            long expectedEntityId,
            int expectedComponentId)
        {
            var pairs = msgs.Pairs;

            for (int i = 0; i < pairs.Count; i++)
            {
                if (pairs[i].key2 == expectedComponentId
                    && pairs[i].key1 == expectedEntityId
                    && pairs[i].value.MessageType == CrdtMessageType.APPEND_COMPONENT)
                {
                    throw new Exception($"Expected not to receive a call, but a call was received");
                }
            }
        }

        public static void Remove_Called(
            this DualKeyValueSet<long, int, WriteData> msgs,
            long expectedEntityId,
            int expectedComponentId)
        {
            var pairs = msgs.Pairs;

            for (int i = 0; i < pairs.Count; i++)
            {
                if (pairs[i].key2 == expectedComponentId
                    && pairs[i].key1 == expectedEntityId
                    && pairs[i].value.MessageType == CrdtMessageType.DELETE_COMPONENT)
                {
                    return;
                }
            }

            throw new Exception($"Expected not to receive a call, but a call was received");
        }

        public static void Clear_Calls(this DualKeyValueSet<long, int, WriteData> msgs)
        {
            msgs.Clear();
        }

        private static bool AssertWriteMessage<T>(
            DualKeyValueSet<long, int, WriteData> msgs,
            long expectedEntityId,
            int expectedComponentId,
            CrdtMessageType expectedMessageType,
            Func<T, bool> expresionComparer) where T: class
        {
            var pairs = msgs.Pairs;

            for (int i = 0; i < pairs.Count; i++)
            {
                if (pairs[i].key2 == expectedComponentId && pairs[i].key1 == expectedEntityId)
                {
                    var value = pairs[i].value;

                    if (value.MessageType != expectedMessageType)
                        throw new Exception($"Expected messageType {expectedMessageType} but was {value.MessageType}");

                    IWrappedComponent<T> componentValue = (IWrappedComponent<T>)value.PooledWrappedComponent.WrappedComponentBase;

                    if (expresionComparer != null)
                    {
                        var result = expresionComparer(componentValue.Model);

                        if (!result)
                            throw new Exception($"Unexpected component value. was: {componentValue.Model}");
                    }

                    return true;
                }
            }

            return false;
        }
    }
}
