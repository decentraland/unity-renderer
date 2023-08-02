using Cysharp.Threading.Tasks;
using DCL.Social.Chat;
using DCL.Interface;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

public class DefaultChatEntryShould
{
    private DefaultChatEntry entry;
    private Canvas canvas;
    private DefaultChatEntry.ILocalTimeConverterStrategy localTimeConverter;

    [SetUp]
    public void SetUp()
    {
        var canvasgo = new GameObject("canvas");
        canvas = canvasgo.AddComponent<Canvas>();
        localTimeConverter = Substitute.For<DefaultChatEntry.ILocalTimeConverterStrategy>();
        localTimeConverter.GetLocalTime(Arg.Any<ulong>())
                          .Returns(info => DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(info[0].ToString())).DateTime);
        ((RectTransform)canvas.transform).sizeDelta = new Vector2(500, 500);
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(entry.gameObject);
        Object.Destroy(canvas.gameObject);
    }

    [UnityTest]
    public IEnumerator PopulateSystemChat() =>
        UniTask.ToCoroutine(async () =>
        {
            GivenEntryChat("ChatEntrySystem");

            var message = new ChatEntryModel
            {
                messageType = ChatMessage.Type.SYSTEM,
                senderName = "user-test",
                recipientName = "receiver-test",
                bodyText = "test message",
                subType = ChatEntryModel.SubType.RECEIVED
            };

            entry.Populate(message);

            await UniTask.DelayFrame(4);

            Assert.AreEqual("<b>user-test:</b> test message", entry.body.text);
        });

    [UnityTest]
    public IEnumerator PopulateReceivedPublicChat() =>
        UniTask.ToCoroutine(async () =>
        {
            GivenEntryChat("PublicChatEntryReceived");

            var message = new ChatEntryModel
            {
                messageType = ChatMessage.Type.PUBLIC,
                senderName = "user-test",
                recipientName = "receiver-test",
                bodyText = "test message",
                subType = ChatEntryModel.SubType.RECEIVED
            };

            entry.Populate(message);

            await UniTask.DelayFrame(4);

            Assert.AreEqual("<b><link=username://user-test>user-test</link>:</b> test message", entry.body.text);
        });

    [UnityTest]
    public IEnumerator PopulateSentPublicChat() =>
        UniTask.ToCoroutine(async () =>
        {
            GivenEntryChat("PublicChatEntrySent");

            var message = new ChatEntryModel
            {
                messageType = ChatMessage.Type.PUBLIC,
                senderName = "user-test",
                recipientName = "receiver-test",
                bodyText = "test message",
                subType = ChatEntryModel.SubType.SENT
            };

            entry.Populate(message);

            await UniTask.DelayFrame(4);

            Assert.AreEqual("<b>You:</b> test message", entry.body.text);
        });

    [UnityTest]
    public IEnumerator PopulateReceivedWhisperInPublicChannel() =>
        UniTask.ToCoroutine(async () =>
        {
            GivenEntryChat("PublicChatEntryReceivedWhisper");

            var message = new ChatEntryModel
            {
                messageType = ChatMessage.Type.PRIVATE,
                senderName = "user-test",
                recipientName = "receiver-test",
                bodyText = "test message",
                subType = ChatEntryModel.SubType.RECEIVED
            };

            entry.Populate(message);

            await UniTask.DelayFrame(4);

            Assert.AreEqual("<b><color=#5EBD3D>From <link=username://user-test>user-test</link></color>:</b> test message", entry.body.text);
        });

    [UnityTest]
    public IEnumerator PopulateSentWhisperInPublicChannel() =>
        UniTask.ToCoroutine(async () =>
        {
            GivenEntryChat("PublicChatEntrySentWhisper");

            var message = new ChatEntryModel
            {
                messageType = ChatMessage.Type.PRIVATE,
                senderName = "user-test",
                recipientName = "receiver-test",
                bodyText = "test message",
                subType = ChatEntryModel.SubType.SENT
            };

            entry.Populate(message);

            await UniTask.DelayFrame(4);

            Assert.AreEqual("<b>To receiver-test:</b> test message", entry.body.text);
        });

    [UnityTest]
    public IEnumerator PopulateSentWhisperInPrivateChannel() =>
        UniTask.ToCoroutine(async () =>
        {
            GivenEntryChat("PrivateChatEntrySent");

            var message = new ChatEntryModel
            {
                messageType = ChatMessage.Type.PRIVATE,
                senderName = "user-test",
                recipientName = "receiver-test",
                bodyText = "test message",
                subType = ChatEntryModel.SubType.SENT
            };

            entry.Populate(message);

            await UniTask.DelayFrame(4);

            Assert.AreEqual("test message", entry.body.text);
        });

    [UnityTest]
    public IEnumerator PopulateReceivedWhisperInPrivateChannel() =>
        UniTask.ToCoroutine(async () =>
        {
            GivenEntryChat("PrivateChatEntryReceived");

            var message = new ChatEntryModel
            {
                messageType = ChatMessage.Type.PRIVATE,
                senderName = "user-test",
                recipientName = "receiver-test",
                bodyText = "test message",
                subType = ChatEntryModel.SubType.RECEIVED
            };

            entry.Populate(message);

            await UniTask.DelayFrame(4);

            Assert.AreEqual("test message", entry.body.text);
        });

    [UnityTest]
    [TestCase("im at 100,100", "100,100", "im at ", ExpectedResult = null)]
    [TestCase("nah we should go to 0,-122", "0,-122", "nah we should go to ", ExpectedResult = null)]
    public IEnumerator AddParcelCoordinates(string body, string coordinates, string bodyWithoutCoordinates) =>
        UniTask.ToCoroutine(async () =>
        {
            GivenEntryChat("PublicChatEntryReceivedWhisper");

            var message = new ChatEntryModel
            {
                messageType = ChatMessage.Type.PRIVATE,
                senderName = "user-test",
                recipientName = "receiver-test",
                bodyText = body,
                subType = ChatEntryModel.SubType.SENT
            };

            entry.Populate(message);

            await UniTask.DelayFrame(4);

            Assert.AreEqual($"<b>To receiver-test:</b> {bodyWithoutCoordinates}</noparse><link={coordinates}><color=#4886E3><u>{coordinates}</u></color></link><noparse>", entry.body.text);
        });

    [UnityTest]
    public IEnumerator AddManyParcelCoordinates() =>
        UniTask.ToCoroutine(async () =>
        {
            GivenEntryChat("PublicChatEntryReceivedWhisper");

            var message = new ChatEntryModel
            {
                messageType = ChatMessage.Type.PRIVATE,
                senderName = "user-test",
                recipientName = "receiver-test",
                bodyText = "perhaps 73,94 then -5,42 and after -36,72",
                subType = ChatEntryModel.SubType.SENT
            };

            entry.Populate(message);

            await UniTask.DelayFrame(4);

            Assert.AreEqual("<b>To receiver-test:</b> perhaps </noparse><link=73,94><color=#4886E3><u>73,94</u></color></link><noparse> then </noparse><link=-5,42><color=#4886E3><u>-5,42</u></color></link><noparse> and after </noparse><link=-36,72><color=#4886E3><u>-36,72</u></color></link><noparse>", entry.body.text);
        });

    [UnityTest]
    public IEnumerator DoNotShowUserName() =>
        UniTask.ToCoroutine(async () =>
        {
            GivenEntryChat("PrivateChatEntryReceived");
            entry.showUserName = false;

            var message = new ChatEntryModel
            {
                messageType = ChatMessage.Type.PRIVATE,
                senderName = "user-test",
                recipientName = "receiver-test",
                bodyText = "test message",
                subType = ChatEntryModel.SubType.RECEIVED
            };

            entry.Populate(message);

            await UniTask.DelayFrame(4);

            Assert.AreEqual("test message", entry.body.text);
        });

    [UnityTest]
    public IEnumerator ShowUserName() =>
        UniTask.ToCoroutine(async () =>
        {
            GivenEntryChat("PrivateChatEntryReceived");
            entry.showUserName = true;

            var message = new ChatEntryModel
            {
                messageType = ChatMessage.Type.PRIVATE,
                senderName = "user-test",
                recipientName = "receiver-test",
                bodyText = "test message",
                subType = ChatEntryModel.SubType.SENT
            };

            entry.Populate(message);

            await UniTask.DelayFrame(4);

            Assert.AreEqual("<b>To receiver-test:</b> test message", entry.body.text);
        });

    [TestCase((ulong)1680013416292, "Loading name - 03/28/23 2:23:36 PM")]
    [TestCase((ulong)1230011812000, "Loading name - 12/23/08 5:56:52 AM")]
    [TestCase((ulong)1430092000000, "Loading name - 04/26/15 11:46:40 PM")]
    public void GetHoverTextWhenLoadingNames(ulong timestamp, string expectedHoverText)
    {
        GivenEntryChat("PrivateChatEntryReceived");

        var message = new ChatEntryModel
        {
            messageType = ChatMessage.Type.PUBLIC,
            senderName = "0xf242fad23fad32adfaf2",
            recipientName = "0x1421f2afd124adf214df",
            bodyText = "test message",
            subType = ChatEntryModel.SubType.SENT,
            isLoadingNames = true,
            timestamp = timestamp,
        };

        entry.Populate(message);

        Assert.AreEqual(expectedHoverText, entry.HoverString);
    }

    [TestCase((ulong)1480072398390, "11/25/16 11:13:18 AM")]
    [TestCase((ulong)1920396820000, "11/08/30 7:33:40 PM")]
    [TestCase((ulong)1693000390000, "08/25/23 9:53:10 PM")]
    public void GetHoverText(ulong timestamp, string expectedHoverText)
    {
        GivenEntryChat("PrivateChatEntryReceived");

        var message = new ChatEntryModel
        {
            messageType = ChatMessage.Type.PUBLIC,
            senderName = "0xf242fad23fad32adfaf2",
            recipientName = "0x1421f2afd124adf214df",
            bodyText = "test message",
            subType = ChatEntryModel.SubType.SENT,
            timestamp = timestamp,
        };

        entry.Populate(message);

        Assert.AreEqual(expectedHoverText, entry.HoverString);
    }

    private void GivenEntryChat(string prefabName)
    {
        entry = Object.Instantiate(
            AssetDatabase.LoadAssetAtPath<DefaultChatEntry>(
                $"Assets/Scripts/MainScripts/DCL/Controllers/HUD/SocialBarPrefabs/SocialBarV1/Prefabs/{prefabName}.prefab"),
            canvas.transform, false);
        entry.LocalTimeConverterStrategy = localTimeConverter;
    }
}
