using System;
using System.IO;
using System.Runtime.InteropServices;
using DCL.CRDT;
using KernelCommunication;
using NUnit.Framework;
using BinaryWriter = KernelCommunication.BinaryWriter;

namespace Tests
{
    public class KernelBinaryMessageShould
    {
        [Test]
        public void ReadUnmanagedMemory()
        {
            byte[] bytes =
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //b: 11
                0, 0, 0, 0, 0, 0, 18, 139, //b:8 v:4747
                0, 0, 0, 42, //b:4 v:42
                1, 1, 1, 1 //b:4
            };

            IntPtr unmanagedArray = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, unmanagedArray, bytes.Length);

            int loopCount = 0;
            IBinaryReader reader = new UnmanagedMemoryReader(unmanagedArray, bytes.Length);
            while (reader.CanRead())
            {
                reader.Skip(11);
                Assert.AreEqual(4747, reader.ReadInt64());
                Assert.AreEqual(42, reader.ReadInt32());
                byte[] data = reader.ReadBytes(4);
                Assert.IsTrue(AreEqual(new byte[] { 1, 1, 1, 1 }, data));
                loopCount++;
            }
            Assert.AreEqual(1, loopCount);

            Marshal.FreeHGlobal(unmanagedArray);
        }

        [Test]
        public void ReadByteArray()
        {
            byte[] bytes =
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //b: 11
                0, 0, 0, 0, 0, 0, 18, 139, //b:8 v:4747
                0, 0, 0, 42, //b:4 v:42
                1, 1, 1, 1 //b:4
            };

            int loopCount = 0;
            IBinaryReader reader = new ByteArrayReader(bytes);
            while (reader.CanRead())
            {
                reader.Skip(11);
                Assert.AreEqual(4747, reader.ReadInt64());
                Assert.AreEqual(42, reader.ReadInt32());
                byte[] data = reader.ReadBytes(4);
                Assert.IsTrue(AreEqual(new byte[] { 1, 1, 1, 1 }, data));
                loopCount++;
            }
            Assert.AreEqual(1, loopCount);
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

            IntPtr unmanagedArray = Marshal.AllocHGlobal(message.Length);
            Marshal.Copy(message, 0, unmanagedArray, message.Length);

            int parsedCount = 0;
            using (var iterator = KernelBinaryMessageDeserializer.Deserialize(unmanagedArray, message.Length))
            {
                while (iterator.MoveNext())
                {
                    parsedCount++;
                }
            }
            Assert.AreEqual(0, parsedCount);
            Marshal.FreeHGlobal(unmanagedArray);

            parsedCount = 0;
            using (var iterator = KernelBinaryMessageDeserializer.Deserialize(message))
            {
                while (iterator.MoveNext())
                {
                    parsedCount++;
                }
            }
            Assert.AreEqual(0, parsedCount);
        }

        [Test]
        public void ParseMessage()
        {
            byte[] message =
            {
                0, 0, 0, 32, //header: length = 32
                0, 0, 0, 1, //header: type = 1 (PUT_COMPONENT)
                0, 0, 0, 1, // component: entityId
                0, 0, 0, 1, // component: componentId
                0, 0, 0, 0, 0, 0, 0, 1, // component: timestamp (int64)
                0, 0, 0, 4, // component: data-lenght
                0, 0, 0, 0, // component: data

                0, 0, 0, 20, //header: length = 20
                0, 0, 0, 44, //header: type = 44 (unknown)
                0, 0, 0, 1, // skip read
                0, 0, 0, 1, // skip read
                0, 0, 0, 1, // skip read

                0, 0, 0, 28, //header: length = 28
                0, 0, 0, 1, //header: type = 1 (PUT_COMPONENT)
                0, 0, 0, 1, // component: entityId
                0, 0, 0, 1, // component: componentId
                0, 0, 0, 0, 0, 0, 0, 1, // component: timestamp (int64)
                0, 0, 0, 0, // component: data-lenght

                0, 0, 0, 12, //header: length = 12
                0, 0, 0, 44, //header: type = 44 (unknown)
                0, 0, 0, 1, // skip read
            };

            IntPtr unmanagedArray = Marshal.AllocHGlobal(message.Length);
            Marshal.Copy(message, 0, unmanagedArray, message.Length);

            int parsedCount = 0;
            using (var iterator = KernelBinaryMessageDeserializer.Deserialize(unmanagedArray, message.Length))
            {
                while (iterator.MoveNext())
                {
                    parsedCount++;
                }
            }
            Assert.AreEqual(2, parsedCount);
            Marshal.FreeHGlobal(unmanagedArray);

            parsedCount = 0;
            using (var iterator = KernelBinaryMessageDeserializer.Deserialize(message))
            {
                while (iterator.MoveNext())
                {
                    parsedCount++;
                }
            }
            Assert.AreEqual(2, parsedCount);
        }

        [Test]
        public void SerializeCRDTCorrectly()
        {
            var msgs = new[]
            {
                new CRDTMessage()
                {
                    key1 = 34465673,
                    key2 = 5858585,
                    timestamp = 9598327474,
                    data = null
                },
                new CRDTMessage()
                {
                    key1 = 7693,
                    key2 = 6,
                    timestamp = 799,
                    data = new byte[] { 0, 4, 7, 9, 1, 55, 89, 54 }
                },
                new CRDTMessage()
                {
                    key1 = 0,
                    key2 = 1,
                    timestamp = 0,
                    data = new byte[] { 1 }
                },
            };
            MemoryStream allMsgsMemoryStream = new MemoryStream();
            BinaryWriter allMsgsBinaryWriter = new BinaryWriter(allMsgsMemoryStream);
            for (int i = 0; i < msgs.Length; i++)
            {
                KernelBinaryMessageSerializer.Serialize(allMsgsBinaryWriter, msgs[i]);

                MemoryStream memoryStream = new MemoryStream();
                BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
                KernelBinaryMessageSerializer.Serialize(binaryWriter, msgs[i]);
                var bytes = memoryStream.ToArray();

                IBinaryReader reader = new ByteArrayReader(bytes);

                // check message header values
                Assert.AreEqual(bytes.Length, reader.ReadInt32());

                int expectedType = msgs[i].data != null ? (int)KernelBinaryMessageType.PUT_COMPONENT : (int)KernelBinaryMessageType.DELETE_COMPONENT;
                Assert.AreEqual(expectedType, reader.ReadInt32());

                binaryWriter.Dispose();
                memoryStream.Dispose();
            }

            // compare messages
            using (var iterator = KernelBinaryMessageDeserializer.Deserialize(new ByteArrayReader(allMsgsMemoryStream.ToArray())))
            {
                int index = 0;
                while (iterator.MoveNext())
                {
                    CRDTMessage result = (CRDTMessage)iterator.Current;
                    Assert.AreEqual(msgs[index].key1, result.key1);
                    Assert.AreEqual(msgs[index].timestamp, result.timestamp);
                    Assert.IsTrue(AreEqual((byte[])msgs[index].data, (byte[])result.data));
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