using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;

public class BaseCollectionShould
{
    [Test]
    public void BeEmptyOnDefaultCreation()
    {
        BaseCollection<int> collection = new BaseCollection<int>();
        Assert.AreEqual(0, collection.Count());
    }

    [Test]
    public void AddAndAccessItems()
    {
        BaseCollection<int> collection = new BaseCollection<int>();
        collection.Add(1);
        collection.Add(2);

        Assert.AreEqual(1, collection[0]);
        Assert.AreEqual(1, collection.ElementAt(0));
        Assert.AreEqual(2, collection[1]);
        Assert.AreEqual(2, collection.ElementAt(1));
    }

    [Test]
    public void ProcessParametrizedConstructor()
    {
        BaseCollection<int> collection = new BaseCollection<int>( new [] {1, 2, 3});
        Assert.AreEqual(3, collection.Count());
        Assert.AreEqual(1, collection[0]);
        Assert.AreEqual(2, collection[1]);
        Assert.AreEqual(3, collection[2]);
    }

    [Test]
    public void RemoveItems()
    {
        BaseCollection<int> collection = new BaseCollection<int>( new [] {1, 2, 3});

        collection.Remove(3);
        collection.Remove(2);
        Assert.AreEqual(1, collection.Count());
        Assert.AreEqual(1, collection[0]);
    }

    [Test]
    public void RemoveAt()
    {
        BaseCollection<int> collection = new BaseCollection<int>( new [] {1, 2, 3});

        collection.RemoveAt(2);
        collection.RemoveAt(1);
        Assert.AreEqual(1, collection.Count());
        Assert.AreEqual(1, collection[0]);
    }

    [Test]
    public void Set()
    {
        BaseCollection<int> collection = new BaseCollection<int>( );
        collection.Set(new [] {1, 2, 3});
        Assert.AreEqual(3, collection.Count());
        Assert.AreEqual(1, collection[0]);
        Assert.AreEqual(2, collection[1]);
        Assert.AreEqual(3, collection[2]);
    }

    [Test]
    public void CallEvents()
    {
        BaseCollection<int> collection = new BaseCollection<int>();
        var setSubscriber = Substitute.For<IDummyEventSubscriber<IEnumerable<int>>>();
        var addedSubscriber = Substitute.For<IDummyEventSubscriber<int>>();
        var removedSubscriber = Substitute.For<IDummyEventSubscriber<int>>();

        collection.OnSet += setSubscriber.React;
        collection.OnAdded += addedSubscriber.React;
        collection.OnRemoved += removedSubscriber.React;

        collection.Set(new [] { 1, 2, 3, 4 });
        collection.Add(4);
        collection.Remove(2);
        collection.RemoveAt(0);

        setSubscriber.Received().React(collection.list);
        addedSubscriber.Received().React(4);
        removedSubscriber.Received().React(2);
        removedSubscriber.Received().React(1);
    }
}

public class BaseDictionaryShould
{
    [Test]
    public void BeEmptyOnDefaultCreation()
    {
        BaseDictionary<int, string> dictionary = new BaseDictionary<int, string>();
        Assert.AreEqual(0, dictionary.Count());
    }

    [Test]
    public void AddAndAccessItems()
    {
        BaseDictionary<int, string> dictionary = new BaseDictionary<int, string>();
        dictionary.Add(1, "1");
        dictionary.Add(2, "2");

        Assert.AreEqual(2, dictionary.Count());
        Assert.AreEqual("1", dictionary[1]);
        Assert.AreEqual("2", dictionary[2]);
    }

    [Test]
    public void ProcessParametrizedConstructor()
    {
        IEnumerable<(int, string)> values = new []{ (1, "1")};

        BaseDictionary<int, string> dictionary = new BaseDictionary<int, string>(new []{ (1, "1"), (2, "2")});

        Assert.AreEqual(2, dictionary.Count());
        Assert.AreEqual("1", dictionary[1]);
        Assert.AreEqual("2", dictionary[2]);
    }

    [Test]
    public void RemoveItems()
    {
        BaseDictionary<int, string> dictionary = new BaseDictionary<int, string>(new []{ (1, "1"), (2, "2"), (3, "3")});

        dictionary.Remove(3);
        dictionary.Remove(2);
        Assert.AreEqual(1, dictionary.Count());
        Assert.AreEqual("1", dictionary[1]);
    }

    [Test]
    public void Set()
    {
        BaseDictionary<int, string> dictionary = new BaseDictionary<int, string>( );
        dictionary.Set(new []{ (1, "1"), (2, "2"), (3, "3")});
        Assert.AreEqual(3, dictionary.Count());
        Assert.AreEqual("1", dictionary[1]);
        Assert.AreEqual("2", dictionary[2]);
        Assert.AreEqual("3", dictionary[3]);
    }

    [Test]
    public void CallEvents()
    {
        BaseDictionary<int, string> dictionary = new BaseDictionary<int, string>();
        var setSubscriber = Substitute.For<IDummyEventSubscriber<IEnumerable<KeyValuePair<int, string>>>>();
        var addedSubscriber = Substitute.For<IDummyEventSubscriber<int, string>>();
        var removedSubscriber = Substitute.For<IDummyEventSubscriber<int, string>>();

        dictionary.OnSet += setSubscriber.React;
        dictionary.OnAdded += addedSubscriber.React;
        dictionary.OnRemoved += removedSubscriber.React;

        dictionary.Set(new []{ (1, "1"), (2, "2"), (3, "3")});
        dictionary.Add(4, "4");
        dictionary.Remove(1);

        setSubscriber.Received().React(dictionary.dictionary);
        addedSubscriber.Received().React(4, "4");
        removedSubscriber.Received().React(1, "1");
    }
}