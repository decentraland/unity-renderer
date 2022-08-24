using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL.Emotes;
using DCL.Helpers;
using NSubstitute;
using NUnit.Framework;
using UnityEngine.TestTools;

public class EmotesCatalogServiceShould
{
    private EmotesCatalogService catalog;
    private IEmotesCatalogBridge bridge;

    [SetUp]
    public void SetUp()
    {
        bridge = Substitute.For<IEmotesCatalogBridge>();
        catalog = new EmotesCatalogService(bridge, Array.Empty<WearableItem>());
        catalog.Initialize();
    }

    [Test]
    public void ReturnUnresolvedPromiseWhenRequestNotReadyEmote()
    {
        var promise = catalog.RequestEmote("id1");

        Assert.NotNull(promise);
        Assert.IsNull(promise.value);
        Assert.IsTrue(promise.keepWaiting);
    }

    [Test]
    public void ReturnUnresolvedPromisesWhenRequestNotReadyEmotes()
    {
        var promises = catalog.RequestEmotes(new [] { "id1", "id2", "id3" });

        foreach (Promise<WearableItem> promise in promises)
        {
            Assert.NotNull(promise);
            Assert.IsNull(promise.value);
            Assert.IsTrue(promise.keepWaiting);
        }
    }

    [Test]
    public void ReturnUnresolvedPromiseWhenRequestNotReadyEmoteMultipleTimes()
    {
        var promise1 = catalog.RequestEmote("id1");
        var promise2 = catalog.RequestEmote("id1");

        Assert.NotNull(promise1);
        Assert.AreNotEqual(promise1, promise2);
        Assert.IsNull(promise1.value);
        Assert.IsTrue(promise1.keepWaiting);
        Assert.IsNull(promise2.value);
        Assert.IsTrue(promise2.keepWaiting);
    }

    [Test]
    public void UpdatesEmotesOnUseWhenRequestingNotReadyEmote()
    {
        catalog.RequestEmote("id1");

        Assert.AreEqual(1, catalog.emotesOnUse.Count);
        Assert.AreEqual(1, catalog.emotesOnUse["id1"]);
        Assert.AreEqual(1, catalog.promises["id1"].Count);
    }

    [Test]
    public void UpdatesEmotesOnUseWhenRequestingNotReadyEmoteMultipleTimes()
    {
        catalog.RequestEmote("id1");
        catalog.RequestEmote("id1");
        catalog.RequestEmote("id1");

        Assert.AreEqual(1, catalog.emotesOnUse.Count);
        Assert.AreEqual(3, catalog.emotesOnUse["id1"]);
        Assert.AreEqual(3, catalog.promises["id1"].Count);
    }

    [Test]
    public void UpdatesEmotesOnUseWhenRequestingNotReadyEmotes()
    {
        catalog.RequestEmotes(new [] { "id1", "id2" });

        Assert.AreEqual(2, catalog.emotesOnUse.Count);
        Assert.AreEqual(1, catalog.emotesOnUse["id1"]);
        Assert.AreEqual(1, catalog.emotesOnUse["id2"]);
        Assert.AreEqual(1, catalog.promises["id1"].Count);
        Assert.AreEqual(1, catalog.promises["id2"].Count);
    }

    [Test]
    public void UpdatesEmotesOnUseWhenRequestingNotReadyEmotesMultipleTimes()
    {
        catalog.RequestEmotes(new [] { "id1", "id2" });
        catalog.RequestEmotes(new [] { "id1", "id2" });
        catalog.RequestEmotes(new [] { "id1", "id2" });

        Assert.AreEqual(2, catalog.emotesOnUse.Count);
        Assert.AreEqual(3, catalog.emotesOnUse["id1"]);
        Assert.AreEqual(3, catalog.emotesOnUse["id2"]);
        Assert.AreEqual(3, catalog.promises["id1"].Count);
        Assert.AreEqual(3, catalog.promises["id2"].Count);
    }

    [Test]
    public void ResolveAndClearPromisesWhenReceivingEmote()
    {
        WearableItem promise1Sucess = null;
        var promise1 = catalog.RequestEmote("id1")
                              .Then(x =>
                              {
                                  promise1Sucess = x;
                              });

        WearableItem promise2Sucess = null;
        var promise2 = catalog.RequestEmote("id1")
                              .Then(x =>
                              {
                                  promise2Sucess = x;
                              });

        var wearable = new WearableItem { id = "id1" };
        bridge.OnEmotesReceived += Raise.Event<Action<WearableItem[]>>(new WearableItem[] { wearable });

        Assert.IsFalse(promise1.keepWaiting);
        Assert.AreEqual(wearable, promise1.value);
        Assert.AreEqual(wearable, promise1Sucess);
        Assert.IsFalse(promise2.keepWaiting);
        Assert.AreEqual(wearable, promise2.value);
        Assert.AreEqual(wearable, promise2Sucess);
        Assert.IsFalse(catalog.promises.ContainsKey("id1"));
    }

    [Test]
    public void ForgetEmotesWithNoUsesGracefully()
    {
        Assert.DoesNotThrow(() => catalog.ForgetEmote("id1"));
    }

    [Test]
    public void KeepEmotesInCatalogIfUsesIsNot0()
    {
        catalog.emotes["id1"] = new WearableItem { id = "id1" };
        catalog.emotesOnUse["id1"] = 10;

        catalog.ForgetEmote("id1");

        Assert.IsTrue(catalog.emotes.ContainsKey("id1"));
        Assert.AreEqual(9, catalog.emotesOnUse["id1"]);
    }

    [Test]
    public void RemoveEmotesFromCatalogIfUsesIs0()
    {
        catalog.emotes["id1"] = new WearableItem { id = "id1" };
        catalog.emotesOnUse["id1"] = 1;

        catalog.ForgetEmote("id1");

        Assert.IsFalse(catalog.emotes.ContainsKey("id1"));
        Assert.IsFalse(catalog.emotesOnUse.ContainsKey("id1"));
    }

    [Test]
    public void NotAddEmotesToCatalogWhenNoPromises()
    {
        bridge.OnEmotesReceived += Raise.Event<Action<WearableItem[]>>(new WearableItem[] { new WearableItem { id = "id1" } });

        Assert.IsFalse(catalog.emotes.ContainsKey("id1"));
        Assert.IsFalse(catalog.emotesOnUse.ContainsKey("id1"));
    }

    [Test]
    public void AddEmotesToCatalogWhenPromises()
    {
        catalog.promises["id1"] = new HashSet<Promise<WearableItem>>( ) { new Promise<WearableItem>() };
        catalog.emotesOnUse["id1"] = 1;

        bridge.OnEmotesReceived += Raise.Event<Action<WearableItem[]>>(new WearableItem[] { new WearableItem { id = "id1" } });

        Assert.IsTrue(catalog.emotes.ContainsKey("id1"));
        Assert.IsTrue(catalog.emotesOnUse.ContainsKey("id1"));
    }

    [Test]
    public void ReturnResolvedPromiseIfEmoteIsOnCatalog()
    {
        WearableItem emote = new WearableItem() { id = "id1" };
        catalog.emotes["id1"] = emote;
        catalog.emotesOnUse["id1"] = 1;

        var promise = catalog.RequestEmote("id1");

        Assert.IsFalse(promise.keepWaiting);
        Assert.AreEqual(emote, promise.value);
    }

    [Test]
    public void ReturnResolvedPromiseIfEmotesIsOnCatalog()
    {
        WearableItem emote1 = new WearableItem() { id = "id1" };
        catalog.emotes["id1"] = emote1;
        catalog.emotesOnUse["id1"] = 1;
        WearableItem emote2 = new WearableItem() { id = "id2" };
        catalog.emotes["id2"] = emote2;
        catalog.emotesOnUse["id2"] = 1;

        var promises = catalog.RequestEmotes(new [] { "id1", "id2" });

        Assert.IsFalse(promises[0].keepWaiting);
        Assert.AreEqual(emote1, promises[0].value);
        Assert.IsFalse(promises[1].keepWaiting);
        Assert.AreEqual(emote2, promises[1].value);
    }

    [UnityTest]
    public IEnumerator ResolveAsyncEmoteRequest() => UniTask.ToCoroutine(async () =>
    {
        WearableItem emote = new WearableItem { id = "id1" };
        bridge.When(x => x.RequestEmote(Arg.Any<string>()))
              .Do((x) =>
              {
                  bridge.OnEmotesReceived += Raise.Event<Action<WearableItem[]>>(new WearableItem[] { emote });
              });

        var emoteReceived = await catalog.RequestEmoteAsync("id1");

        Assert.AreEqual(emote, emoteReceived);
    });

    [UnityTest]
    public IEnumerator ResolveAsyncEmotesRequest() => UniTask.ToCoroutine(async () =>
    {
        Dictionary<string, WearableItem> emotes = new Dictionary<string, WearableItem>
        {
            { "id1", new WearableItem { id = "id1" } },
            { "id2", new WearableItem { id = "id2" } },
        };
        bridge.When(x => x.RequestEmote(Arg.Any<string>()))
              .Do((x) =>
              {
                  bridge.OnEmotesReceived += Raise.Event<Action<WearableItem[]>>(new WearableItem[] { emotes[x.Arg<string>()] });
              });

        var emotesReceived = await catalog.RequestEmotesAsync(new [] { "id1", "id2" });

        Assert.AreEqual(emotes["id1"], emotesReceived[0]);
        Assert.AreEqual(emotes["id2"], emotesReceived[1]);
    });

    [UnityTest]
    public IEnumerator RemovePromisesWhenCancellingRequestEmoteAsync() => UniTask.ToCoroutine(async () =>
    {
        CancellationTokenSource autoCancelCTS = new CancellationTokenSource();
        autoCancelCTS.CancelAfterSlim(10); //Auto cancel in 10 miliseconds
        CancellationTokenSource cts = new CancellationTokenSource();

        catalog.RequestEmoteAsync("id1", cts.Token); //This wont be autocancelled
        catalog.RequestEmoteAsync("id1", cts.Token); //This wont be autocancelled
        await catalog.RequestEmoteAsync("id1", autoCancelCTS.Token); //This will be autocancelled

        await UniTask.NextFrame();

        Assert.AreEqual(2, catalog.promises["id1"].Count);

        cts.Cancel();
    });

    [UnityTest]
    public IEnumerator RemovePromiseEntryWhenCancellingAllRequestEmoteAsync() => UniTask.ToCoroutine(async () =>
    {
        CancellationTokenSource autoCancelCTS = new CancellationTokenSource();
        autoCancelCTS.CancelAfterSlim(10); //Auto cancel in 10 miliseconds

        catalog.RequestEmoteAsync("id1", autoCancelCTS.Token); //This wont be autocancelled
        catalog.RequestEmoteAsync("id1", autoCancelCTS.Token); //This wont be autocancelled
        await catalog.RequestEmoteAsync("id1", autoCancelCTS.Token); //This will be autocancelled

        await UniTask.NextFrame();

        Assert.IsFalse(catalog.promises.ContainsKey("id1"));
    });

    [UnityTest]
    public IEnumerator RemovePromisesWhenCancellingRequestEmotesAsync() => UniTask.ToCoroutine(async () =>
    {
        CancellationTokenSource autoCancelCTS = new CancellationTokenSource();
        autoCancelCTS.CancelAfterSlim(10); //Auto cancel in 10 miliseconds
        CancellationTokenSource cts = new CancellationTokenSource();

        catalog.RequestEmotesAsync(new [] { "id1", "id2" }, cts.Token); //This wont be autocancelled
        catalog.RequestEmotesAsync(new [] { "id1", "id2" }, cts.Token); //This wont be autocancelled
        await catalog.RequestEmotesAsync(new [] { "id1", "id2" }, autoCancelCTS.Token); //This will be autocancelled

        await UniTask.NextFrame();

        Assert.AreEqual(2, catalog.promises["id1"].Count);
        Assert.AreEqual(2, catalog.promises["id2"].Count);

        cts.Cancel();
    });

    [UnityTest]
    public IEnumerator RemovePromiseEntryWhenCancellingAllRequestEmotesAsync() => UniTask.ToCoroutine(async () =>
    {
        CancellationTokenSource autoCancelCTS = new CancellationTokenSource();
        autoCancelCTS.CancelAfterSlim(10); //Auto cancel in 10 miliseconds

        catalog.RequestEmotesAsync(new [] { "id1", "id2" }, autoCancelCTS.Token); //This will be autocancelled
        await catalog.RequestEmotesAsync(new [] { "id1", "id2" }, autoCancelCTS.Token); //This will be autocancelled

        await UniTask.NextFrame();

        Assert.IsFalse(catalog.promises.ContainsKey("id1"));
        Assert.IsFalse(catalog.promises.ContainsKey("id2"));
    });

    [Test]
    public void EmbedEmotes()
    {
        WearableItem[] embededEmotes = new [] { new WearableItem { id = "id1" }, new WearableItem { id = "id2" }, new WearableItem { id = "id3" } };
        catalog = new EmotesCatalogService(Substitute.For<IEmotesCatalogBridge>(), embededEmotes);

        Assert.AreEqual(catalog.emotes["id1"], embededEmotes[0]);
        Assert.AreEqual(catalog.emotes["id2"], embededEmotes[1]);
        Assert.AreEqual(catalog.emotes["id3"], embededEmotes[2]);
        Assert.AreEqual(catalog.emotesOnUse["id1"], 5000);
        Assert.AreEqual(catalog.emotesOnUse["id2"], 5000);
        Assert.AreEqual(catalog.emotesOnUse["id3"], 5000);
    }
}