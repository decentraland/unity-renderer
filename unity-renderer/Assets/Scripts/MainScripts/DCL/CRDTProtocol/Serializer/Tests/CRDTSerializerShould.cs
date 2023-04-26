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
            ExpectedResult = new byte[]
            {
                24, 0, 0, 0,    //msg_length
                1, 0, 0, 0,     //msg_type
                0, 0, 0, 0,     //entityId
                1, 0, 0, 0,     //componentId
                100, 0, 0, 0,   //timestamp
                0, 0, 0, 0      //data_length
            })]

        [TestCase(32424, 67867, 2138996092, new byte[] { 42, 33, 67, 22 },
            ExpectedResult = new byte[]
            {
                28, 0, 0, 0,        //msg_length
                1, 0, 0, 0,         //msg_type
                168, 126, 0, 0,     //entityId
                27, 9, 1, 0,        //componentId
                124, 125, 126, 127, //timestamp
                4, 0, 0, 0,         //data_length
                42, 33, 67, 22      //data
            })]

        public byte[] SerializeCorrectlyPutComponent(int entityId, int componentId, int timestamp, byte[] data)
        {
            var message = new CrdtMessage
            (
                type: CrdtMessageType.PUT_COMPONENT,
                entityId: entityId,
                componentId: componentId,
                timestamp: timestamp,
                data: data
            );

            var memoryStream = new MemoryStream();
            var binaryWriter = new BinaryWriter(memoryStream);
            CRDTSerializer.Serialize(binaryWriter, message);
            var bytes = memoryStream.ToArray();

            CrdtMessageType crdtMessageType = CrdtMessageType.PUT_COMPONENT;
            int memoryPosition = 8; // skip the CrdtMessageHeader
            CrdtMessage? result = CRDTDeserializer.DeserializePutComponent(bytes, ref memoryPosition);
            object expextedData = message.Data ?? new byte[0]; // NULL data for a PUT operation will be converted to byte[0]

            Assert.AreEqual(message.EntityId, result.Value.EntityId);
            Assert.AreEqual(message.Timestamp, result.Value.Timestamp);
            Assert.IsTrue(AreEqual((byte[])expextedData, (byte[])result.Value.Data));

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
