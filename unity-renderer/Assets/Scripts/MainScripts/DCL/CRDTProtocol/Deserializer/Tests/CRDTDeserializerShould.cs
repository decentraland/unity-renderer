using System;
using DCL.CRDT;
using NUnit.Framework;

namespace Tests
{
    public class CRDTDeserializerShould
    {

        [Test]
        public void ParseByteArray()
        {
            byte[] bytes =
            {
                0, 0, 0, 68, 0, 0, 0, 1, 0, 0, 2, 154,
                0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 29, 242,
                0, 0, 0, 40, 64, 73, 15, 219, 64, 73, 15, 219,
                64, 73, 15, 219, 64, 73, 15, 219, 64, 73, 15, 219,
                64, 73, 15, 219, 64, 73, 15, 219, 64, 73, 15, 219,
                64, 73, 15, 219, 64, 73, 15, 219
            };

            CRDTComponentMessageHeader expectedComponentHeader = new CRDTComponentMessageHeader()
            {
                entityId = 666,
                componentClassId = 1,
                timestamp = 7666,
                dataLength = 40
            };

            TestInput(new ReadOnlyMemory<byte>(bytes), new[] { expectedComponentHeader });
        }

        [Test]
        public void ParseTwoMessagesInSameByteArray()
        {
            byte[] bytes =
            {
                0, 0, 0, 68, 0, 0, 0, 1, 0, 0, 2, 154,
                0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 29, 242,
                0, 0, 0, 40, 64, 73, 15, 219, 64, 73, 15, 219,
                64, 73, 15, 219, 64, 73, 15, 219, 64, 73, 15, 219,
                64, 73, 15, 219, 64, 73, 15, 219, 64, 73, 15, 219,
                64, 73, 15, 219, 64, 73, 15, 219,
                0, 0, 0, 68, 0, 0, 0, 1, 0, 0, 2, 154,
                0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 29, 242,
                0, 0, 0, 40, 64, 73, 15, 219, 64, 73, 15, 219,
                64, 73, 15, 219, 64, 73, 15, 219, 64, 73, 15, 219,
                64, 73, 15, 219, 64, 73, 15, 219, 64, 73, 15, 219,
                64, 73, 15, 219, 64, 73, 15, 219
            };

            CRDTComponentMessageHeader expectedComponentHeader = new CRDTComponentMessageHeader()
            {
                entityId = 666,
                componentClassId = 1,
                timestamp = 7666,
                dataLength = 40
            };

            TestInput(new ReadOnlyMemory<byte>(bytes), new[] { expectedComponentHeader, expectedComponentHeader });
        }

        [Test]
        public void CopyByteArrayDataCorrectly()
        {
            byte[] binaryMessage =
            {
                0, 0, 0, 68, 0, 0, 0, 1, 0, 0, 2, 154,
                0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 29, 242,
                0, 0, 0, 40, 64, 73, 15, 219, 64, 73, 15, 219,
                64, 73, 15, 219, 64, 73, 15, 219, 64, 73, 15, 219,
                64, 73, 15, 219, 64, 73, 15, 219, 64, 73, 15, 219,
                64, 73, 15, 219, 64, 73, 15, 219
            };

            int dataStart = 8 + 20; //sizeof(CRDTMessageHeader) + sizeof(CRDTComponentMessageHeader)
            byte[] data = new byte[binaryMessage.Length - dataStart];
            Buffer.BlockCopy(binaryMessage, dataStart, data, 0, data.Length);

            using (var iterator = CRDTDeserializer.DeserializeBatch(binaryMessage))
            {
                while (iterator.MoveNext())
                {
                    Assert.IsTrue(AreEqual(data, (byte[])((CRDTMessage)iterator.Current).data),
                        "messages data are not equal");
                }
            }
        }

        [Test]
        public void SetDataOnPutComponentWithEmptyPayload()
        {
            byte[] message =
            {
                0, 0, 0, 32, //header: length = 32
                0, 0, 0, 1, //header: type = 1 (PUT_COMPONENT)
                0, 0, 0, 1, // component: entityId
                0, 0, 0, 1, // component: componentId
                0, 0, 0, 0, 0, 0, 0, 1, // component: timestamp (int64)
                0, 0, 0, 0, // component: data-lenght (0)
            };

            using (var iterator = CRDTDeserializer.DeserializeBatch(message))
            {
                while (iterator.MoveNext())
                {
                    byte[] data = ((CRDTMessage)iterator.Current).data as byte[];
                    Assert.NotNull(data);
                    Assert.AreEqual(0, data.Length);
                }
            }
        }

        [Test]
        public void SkipUnknownMessageType()
        {
            byte[] message =
            {
                0, 0, 0, 27, //header: length = 27
                0, 0, 0, 44, //header: type = 44 (unknown)
                0, 0, 0, 0, 0, 0, 0, //b: 11
                0, 0, 0, 0, 0, 0, 18, 139, //b:8
            };


            int parsedCount = 0;
            using (var iterator = CRDTDeserializer.DeserializeBatch(new ReadOnlyMemory<byte>(message)))
            {
                while (iterator.MoveNext())
                {
                    parsedCount++;
                }
            }
            Assert.AreEqual(0, parsedCount);
        }

        static void TestInput(ReadOnlyMemory<byte> memory, CRDTComponentMessageHeader[] expectedComponentHeader)
        {
            int count = 0;
            using (var iterator = CRDTDeserializer.DeserializeBatch(memory))
            {
                while (iterator.MoveNext())
                {
                    Assert.IsTrue(AreEqual(expectedComponentHeader[count], CRDTDeserializer.componentHeader),
                        "component header are not equal");

                    count++;
                }
            }
            Assert.AreEqual(expectedComponentHeader.Length, count);
        }

        static bool AreEqual(CRDTComponentMessageHeader a, CRDTComponentMessageHeader b)
        {
            return a.entityId == b.entityId
                   && a.componentClassId == b.componentClassId
                   && a.timestamp == b.timestamp
                   && a.dataLength == b.dataLength;
        }

        static bool AreEqual(byte[] a, byte[] b)
        {
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