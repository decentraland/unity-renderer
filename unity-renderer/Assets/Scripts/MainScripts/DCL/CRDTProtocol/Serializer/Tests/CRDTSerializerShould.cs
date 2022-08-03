using System.IO;
using DCL.CRDT;
using KernelCommunication;
using NUnit.Framework;
using BinaryWriter = KernelCommunication.BinaryWriter;

namespace Tests
{
    public class CRDTSerializerShould
    {
        [Test]
        [TestCase(0, 1, 100, null,
            ExpectedResult = new byte[] { 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 100, 0, 0, 0, 0 })]
        [TestCase(32424, 67867, 2423423423, new byte[] { 42, 33, 67, 22 },
            ExpectedResult = new byte[] { 0, 0, 126, 168, 0, 1, 9, 27, 0, 0, 0, 0, 144, 114, 129, 191, 0, 0, 0, 4, 42, 33, 67, 22 })]
        public byte[] SerializeCorrectly(int entityId, int componentId, long timestamp, byte[] data)
        {
            var message = new CRDTMessage()
            {
                key1 = entityId,
                key2 = componentId,
                timestamp = timestamp,
                data = data
            };

            var memoryStream = new MemoryStream();
            var binaryWriter = new BinaryWriter(memoryStream);
            CRDTSerializer.Serialize(binaryWriter, message);
            var bytes = memoryStream.ToArray();

            CRDTMessage result = CRDTDeserializer.Deserialize(new ByteArrayReader(bytes));

            Assert.AreEqual(message.key1, result.key1);
            Assert.AreEqual(message.timestamp, result.timestamp);
            Assert.IsTrue(AreEqual((byte[])message.data, (byte[])result.data));

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