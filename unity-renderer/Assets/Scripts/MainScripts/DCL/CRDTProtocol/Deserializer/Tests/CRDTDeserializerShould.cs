using System;
using System.Runtime.InteropServices;
using DCL.CRDT;
using KernelCommunication;
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

            TestInput(new ByteArrayReader(bytes), new[] { expectedComponentHeader });
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

            TestInput(new ByteArrayReader(bytes), new[] { expectedComponentHeader, expectedComponentHeader });
        }

        [Test]
        public void ParseUnmanagedMemory()
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

            IntPtr unmanagedArray = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, unmanagedArray, bytes.Length);

            TestInput(new UnmanagedMemoryReader(unmanagedArray, bytes.Length),
                new[] { expectedComponentHeader });

            Marshal.FreeHGlobal(unmanagedArray);
        }

        [Test]
        public void ParseTwoMessagesInSameUnmanagedMemory()
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

            IntPtr unmanagedArray = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, unmanagedArray, bytes.Length);

            TestInput(new UnmanagedMemoryReader(unmanagedArray, bytes.Length),
                new[] { expectedComponentHeader, expectedComponentHeader });

            Marshal.FreeHGlobal(unmanagedArray);
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

            using (var iterator = KernelBinaryMessageDeserializer.Deserialize(binaryMessage))
            {
                while (iterator.MoveNext())
                {
                    Assert.IsTrue(AreEqual(data, (byte[])((CRDTMessage)iterator.Current).data),
                        "messages data are not equal");
                }
            }
        }

        [Test]
        public void CopyUnmanagedDataCorrectly()
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

            IntPtr unmanagedArray = Marshal.AllocHGlobal(binaryMessage.Length);
            Marshal.Copy(binaryMessage, 0, unmanagedArray, binaryMessage.Length);

            using (var iterator =
                KernelBinaryMessageDeserializer.Deserialize(unmanagedArray, binaryMessage.Length))
            {
                while (iterator.MoveNext())
                {
                    Assert.IsTrue(AreEqual(data, (byte[])((CRDTMessage)iterator.Current).data),
                        "messages data are not equal");
                }
            }
            Marshal.FreeHGlobal(unmanagedArray);
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

            using (var iterator =
                KernelBinaryMessageDeserializer.Deserialize(message))
            {
                while (iterator.MoveNext())
                {
                    byte[] data = ((CRDTMessage)iterator.Current).data as byte[];
                    Assert.NotNull(data);
                    Assert.AreEqual(0, data.Length);
                }
            }
        }

        static void TestInput(IBinaryReader reader, CRDTComponentMessageHeader[] expectedComponentHeader)
        {
            int count = 0;
            using (var iterator = KernelBinaryMessageDeserializer.Deserialize(reader))
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