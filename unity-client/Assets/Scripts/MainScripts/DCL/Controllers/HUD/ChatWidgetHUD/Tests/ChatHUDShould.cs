using DCL.Interface;
using NUnit.Framework;
using System.Collections;

public class ChatHUDShould : TestsBase
{
    ChatHUDController controller;
    ChatHUDView view;

    protected override IEnumerator SetUp()
    {
        controller = new ChatHUDController();
        controller.Initialize(null, OnSendMessage);
        this.view = controller.view;
        Assert.IsTrue(this.view != null);
        yield break;
    }

    string lastMsgSent;

    void OnSendMessage(string msg)
    {
        lastMsgSent = msg;
    }

    protected override IEnumerator TearDown()
    {
        controller.Dispose();
        yield break;
    }

    [Test]
    public void SubmitMessageProperly()
    {
        controller.view.inputField.onSubmit.Invoke("test message");
        Assert.AreEqual("test message", lastMsgSent);
    }

    [Test]
    public void TrimWhenTooMuchMessagesAreInView()
    {
        for (int i = 0; i < ChatHUDController.MAX_CHAT_ENTRIES + 5; i++)
        {
            var msg = new ChatEntry.Model()
            {
                messageType = ChatMessage.Type.PUBLIC,
                senderName = "test" + i,
                bodyText = "test" + i,
            };

            controller.AddChatMessage(msg);
        }

        Assert.AreEqual(ChatHUDController.MAX_CHAT_ENTRIES, controller.view.entries.Count);
        Assert.AreEqual("test5", controller.view.entries[0].message.bodyText);
    }

    [Test]
    public void AddAndClearChatEntriesProperly()
    {
        var msg = new ChatEntry.Model()
        {
            messageType = ChatMessage.Type.PUBLIC,
            senderName = "test",
            bodyText = "test",
        };

        controller.AddChatMessage(msg);

        Assert.AreEqual(1, controller.view.entries.Count);
        Assert.AreEqual(msg, controller.view.entries[0].message);

        controller.view.CleanAllEntries();

        Assert.AreEqual(0, controller.view.entries.Count);

    }
}
