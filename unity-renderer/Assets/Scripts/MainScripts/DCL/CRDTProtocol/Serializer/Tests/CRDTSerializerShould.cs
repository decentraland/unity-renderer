using System.IO;
using DCL.CRDT;
using NUnit.Framework;
using BinaryWriter = KernelCommunication.BinaryWriter;

namespace Tests
{
    public class CRDTSerializerShould
    {
        [Test]
        [TestCase(0, 1, 100, null,
            ExpectedResult = new byte[] { 0,0,0,24,0,0,0,1,0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 100, 0, 0, 0, 0 })]
        [TestCase(32424, 67867, 2138996092, new byte[] { 42, 33, 67, 22 },
            ExpectedResult = new byte[] { 0, 0, 0, 28, 0, 0, 0, 1, 0, 0, 126, 168, 0, 1, 9, 27, 127, 126, 125, 124, 0, 0, 0, 4, 42, 33, 67, 22 })]
                                    //    msg_length |  msg_type |    entityId   | componentId|     timestamp     |data_length| data
        public byte[] SerializeCorrectlyPutComponent(int entityId, int componentId, int timestamp, byte[] data)
        {
            var message = new CRDTMessage()
            {
                type = CrdtMessageType.PUT_COMPONENT,
                entityId = entityId,
                componentId = componentId,
                timestamp = timestamp,
                data = data
            };

            var memoryStream = new MemoryStream();
            var binaryWriter = new BinaryWriter(memoryStream);
            CRDTSerializer.Serialize(binaryWriter, message);
            var bytes = memoryStream.ToArray();

            CrdtMessageType crdtMessageType = CrdtMessageType.PUT_COMPONENT;
            int memoryPosition = 8; // skip the CrdtMessageHeader
            CRDTMessage result = CRDTDeserializer.DeserializePutComponent(bytes, crdtMessageType, ref memoryPosition);
            object expextedData = message.data ?? new byte[0]; // NULL data for a PUT operation will be converted to byte[0]

            Assert.AreEqual(message.entityId, result.entityId);
            Assert.AreEqual(message.timestamp, result.timestamp);
            Assert.IsTrue(AreEqual((byte[])expextedData, (byte[])result.data));

            return bytes;
        }

        static bool AreEqual(byte[] a, byte[] b)
        {
            if (a == null && b == null)
                return true;

            if (a == null || b == null)
                return false;

            if (a.Length != b.Length)
                return false;

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                    return false;
            }

            return true;
        }
    }
}
