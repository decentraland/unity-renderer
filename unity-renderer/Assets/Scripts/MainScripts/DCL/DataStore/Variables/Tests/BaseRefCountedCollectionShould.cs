using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace VariableTests
{
    public class BaseRefCountedCollectionShould
    {
        private BaseRefCountedCollection<string> collection;
        List<(string, int)> onRefCountUpdatedCalls;

        [SetUp]
        public void SetUp()
        {
            collection = new BaseRefCountedCollection<string>();
            onRefCountUpdatedCalls = new List<(string, int)>();
            collection.OnRefCountUpdated += (value, count) => { onRefCountUpdatedCalls.Add((value, count)); };
        }

        [Test]
        public void AddAValueWhenIncreasedForTheFirstTime()
        {
            collection.IncreaseRefCount("value");

            Assert.IsTrue(collection.dictionary.ContainsKey("value"));
            Assert.AreEqual(1, collection["value"]);
            Assert.IsTrue(onRefCountUpdatedCalls.Contains(("value", 1)));
        }

        [Test]
        public void IncreaseTheRefCount()
        {
            collection.dictionary.Add("value", 5);

            collection.IncreaseRefCount("value");

            Assert.IsTrue(collection.dictionary.ContainsKey("value"));
            Assert.AreEqual(6, collection["value"]);
            Assert.IsTrue(onRefCountUpdatedCalls.Contains(("value", 6)));
        }

        [Test]
        public void DecreaseTheRefCount()
        {
            collection.dictionary.Add("value", 5);

            collection.DecreaseRefCount("value");

            Assert.IsTrue(collection.dictionary.ContainsKey("value"));
            Assert.AreEqual(4, collection["value"]);
            Assert.IsTrue(onRefCountUpdatedCalls.Contains(("value", 4)));
        }

        [Test]
        public void RemoveValueWhenCountIsZero()
        {
            collection.dictionary.Add("value", 1);

            collection.DecreaseRefCount("value");

            Assert.IsFalse(collection.dictionary.ContainsKey("value"));
            Assert.IsTrue(onRefCountUpdatedCalls.Contains(("value", 0)));
        }

        [Test]
        public void ClearAndCallEvents()
        {
            collection.dictionary.Add("value", 5);
            collection.dictionary.Add("value2", 5);
            collection.dictionary.Add("value3", 5);

            collection.Clear();

            Assert.AreEqual(0, collection.Count());
            Assert.IsTrue(onRefCountUpdatedCalls.Contains(("value", 0)));
            Assert.IsTrue(onRefCountUpdatedCalls.Contains(("value2", 0)));
            Assert.IsTrue(onRefCountUpdatedCalls.Contains(("value3", 0)));
        }

        [Test]
        public void SetRefCount()
        {
            collection.dictionary.Add("value", 9);

            collection.SetRefCount("value", 5);

            Assert.IsTrue(collection.dictionary.ContainsKey("value"));
            Assert.AreEqual(5, collection["value"]);
            Assert.IsTrue(onRefCountUpdatedCalls.Contains(("value", 5)));
        }

        [Test]
        public void SetRefCounts()
        {
            collection.dictionary.Add("value", 9);
            collection.dictionary.Add("value2", 9);

            collection.SetRefCounts(new [] { ("value", 5), ("value2", 7), ("value3", 8) });

            Assert.IsTrue(collection.dictionary.ContainsKey("value"));
            Assert.IsTrue(collection.dictionary.ContainsKey("value2"));
            Assert.IsTrue(collection.dictionary.ContainsKey("value3"));
            Assert.AreEqual(5, collection["value"]);
            Assert.AreEqual(7, collection["value2"]);
            Assert.AreEqual(8, collection["value3"]);
            Assert.IsTrue(onRefCountUpdatedCalls.Contains(("value", 5)));
            Assert.IsTrue(onRefCountUpdatedCalls.Contains(("value2", 7)));
            Assert.IsTrue(onRefCountUpdatedCalls.Contains(("value3", 8)));
        }
    }
}