using DCL.CRDT;
using KernelCommunication;
using NUnit.Framework;
using System.IO;
using BinaryWriter = KernelCommunication.BinaryWriter;

namespace Tests
{
    public class KernelBinaryMessageShould
    {
        [Test]
        public void SerializeCRDTCorrectly()
        {
            var msgs = new[]
            {
                new CrdtMessage
                (
                    type: CrdtMessageType.DELETE_COMPONENT,
                    entityId: 34465673,
                    componentId: 5858585,
                    timestamp: 95987474,
                    data: null
                ),
                new CrdtMessage
                (
                    type: CrdtMessageType.PUT_COMPONENT,
                    entityId: 7693,
                    componentId: 6,
                    timestamp: 799,
                    data: new byte[] { 0, 4, 7, 9, 1, 55, 89, 54 }
                ),
                new CrdtMessage(
                    type: CrdtMessageType.PUT_COMPONENT,
                    entityId: 0,
                    componentId: 1,
                    timestamp: 0,
                    data: new byte[] { 1 }
                ),
            };

            MemoryStream allMsgsMemoryStream = new MemoryStream();
            BinaryWriter allMsgsBinaryWriter = new BinaryWriter(allMsgsMemoryStream);

            for (int i = 0; i < msgs.Length; i++)
            {
                CRDTSerializer.Serialize(allMsgsBinaryWriter, msgs[i]);

                MemoryStream memoryStream = new MemoryStream();
                BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
                CRDTSerializer.Serialize(binaryWriter, msgs[i]);
                var bytes = memoryStream.ToArray();

                IBinaryReader reader = new ByteArrayReader(bytes);

                // check message header values
                Assert.AreEqual(bytes.Length, reader.ReadInt32());

                int expectedType = msgs[i].Data != null ? (int)CrdtMessageType.PUT_COMPONENT : (int)CrdtMessageType.DELETE_COMPONENT;
                Assert.AreEqual(expectedType, reader.ReadInt32());

                binaryWriter.Dispose();
                memoryStream.Dispose();
            }

            // compare messages
            using (var iterator = CRDTDeserializer.DeserializeBatch(allMsgsMemoryStream.ToArray()))
            {
                int index = 0;

                while (iterator.MoveNext())
                {
                    CrdtMessage result = (CrdtMessage)iterator.Current;
                    Assert.AreEqual(msgs[index].EntityId, result.EntityId);
                    Assert.AreEqual(msgs[index].Timestamp, result.Timestamp);
                    Assert.IsTrue(AreEqual((byte[])msgs[index].Data, (byte[])result.Data));
                    index++;
                }
            }

            allMsgsBinaryWriter.Dispose();
            allMsgsMemoryStream.Dispose();
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
