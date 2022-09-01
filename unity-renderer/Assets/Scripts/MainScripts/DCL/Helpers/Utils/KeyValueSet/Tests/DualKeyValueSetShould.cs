using NUnit.Framework;

namespace Tests
{
    public class DualKeyValueSetShould
    {
        [Test]
        public void AddCorrectly()
        {
            DualKeyValueSet<string, int, int> dualKeyValueSet = new DualKeyValueSet<string, int, int>();
            var item1 = new { key1 = "temptation", key2 = 0, value = 1 };
            var item2 = new { key1 = "temptation1", key2 = 0, value = 2 };
            var item3 = new { key1 = "temptatio", key2 = 0, value = 3 };

            dualKeyValueSet.Add(item1.key1, item1.key2, item1.value);
            dualKeyValueSet.Add(item2.key1, item2.key2, item2.value);
            dualKeyValueSet.Add(item3.key1, item3.key2, item3.value);

            Assert.AreEqual(item1.key1, dualKeyValueSet.Pairs[0].key1);
            Assert.AreEqual(item1.key2, dualKeyValueSet.Pairs[0].key2);
            Assert.AreEqual(item1.value, dualKeyValueSet.Pairs[0].value);

            Assert.AreEqual(item2.key1, dualKeyValueSet.Pairs[1].key1);
            Assert.AreEqual(item2.key2, dualKeyValueSet.Pairs[1].key2);
            Assert.AreEqual(item2.value, dualKeyValueSet.Pairs[1].value);

            Assert.AreEqual(item3.key1, dualKeyValueSet.Pairs[2].key1);
            Assert.AreEqual(item3.key2, dualKeyValueSet.Pairs[2].key2);
            Assert.AreEqual(item3.value, dualKeyValueSet.Pairs[2].value);
        }

        [Test]
        public void RemoveCorrectly()
        {
            DualKeyValueSet<string, int, int> dualKeyValueSet = new DualKeyValueSet<string, int, int>();
            var item1 = new { key1 = "temptation", key2 = 0, value = 1 };
            var item2 = new { key1 = "temptation1", key2 = 0, value = 2 };
            var item3 = new { key1 = "temptatio", key2 = 0, value = 3 };
            var item4 = new { key1 = "temptatio", key2 = 1, value = 3 };

            dualKeyValueSet.Add(item1.key1, item1.key2, item1.value);
            dualKeyValueSet.Add(item2.key1, item2.key2, item2.value);
            dualKeyValueSet.Add(item3.key1, item3.key2, item3.value);
            dualKeyValueSet.Add(item4.key1, item4.key2, item4.value);

            dualKeyValueSet.Remove(item2.key1, item2.key2);

            Assert.AreEqual(item1.key1, dualKeyValueSet.Pairs[0].key1);
            Assert.AreEqual(item1.key2, dualKeyValueSet.Pairs[0].key2);
            Assert.AreEqual(item1.value, dualKeyValueSet.Pairs[0].value);

            Assert.AreEqual(item4.key1, dualKeyValueSet.Pairs[1].key1);
            Assert.AreEqual(item4.key2, dualKeyValueSet.Pairs[1].key2);
            Assert.AreEqual(item4.value, dualKeyValueSet.Pairs[1].value);

            Assert.AreEqual(item3.key1, dualKeyValueSet.Pairs[2].key1);
            Assert.AreEqual(item3.key2, dualKeyValueSet.Pairs[2].key2);
            Assert.AreEqual(item3.value, dualKeyValueSet.Pairs[2].value);

            dualKeyValueSet.Remove(item3.key1, item3.key2);

            Assert.AreEqual(item1.key1, dualKeyValueSet.Pairs[0].key1);
            Assert.AreEqual(item1.key2, dualKeyValueSet.Pairs[0].key2);
            Assert.AreEqual(item1.value, dualKeyValueSet.Pairs[0].value);

            Assert.AreEqual(item4.key1, dualKeyValueSet.Pairs[1].key1);
            Assert.AreEqual(item4.key2, dualKeyValueSet.Pairs[1].key2);
            Assert.AreEqual(item4.value, dualKeyValueSet.Pairs[1].value);

            dualKeyValueSet.Remove(item1.key1, item1.key2);

            Assert.AreEqual(item4.key1, dualKeyValueSet.Pairs[0].key1);
            Assert.AreEqual(item4.key2, dualKeyValueSet.Pairs[0].key2);
            Assert.AreEqual(item4.value, dualKeyValueSet.Pairs[0].value);

            dualKeyValueSet.Remove(item4.key1, item4.key2);

            Assert.AreEqual(0, dualKeyValueSet.Count);
        }
    }
}