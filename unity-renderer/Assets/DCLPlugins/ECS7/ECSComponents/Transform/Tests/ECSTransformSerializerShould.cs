using DCL.ECSComponents;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class ECSTransformSerializerShould
    {
        [Test]
        public void SerializeAndDeserialzeCorrectly()
        {
            TestCase(new ECSTransform()
            {
                position = new Vector3(0, 1, 9999999999),
                scale = new Vector3(56765, -2394951, 100030404050505),
                rotation = new Quaternion(23423423, 34534566, -1273879950505, 90569045094),
                parentId = -10000
            });

            TestCase(new ECSTransform()
            {
                position = new Vector3(9435.9743589453f, 484, 28857.474f),
                scale = new Vector3(0.944384843583f, 293748292, 0.000001f),
                rotation = new Quaternion(-865840, 234234, 234234234, -235325),
                parentId = -858594
            });
        }

        private void TestCase(ECSTransform transform)
        {
            byte[] serialized = ECSTransformSerialization.Serialize(transform);
            ECSTransform deserialized = ECSTransformSerialization.Deserialize(serialized);

            Assert.AreEqual(transform.position, deserialized.position);
            Assert.AreEqual(transform.rotation, deserialized.rotation);
            Assert.AreEqual(transform.scale, deserialized.scale);
            Assert.AreEqual(transform.parentId, deserialized.parentId);
        }
    }
}