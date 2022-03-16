using System;
using System.Collections;
using DCL.Interface;
using NUnit.Framework;

public class ChatHUDViewShould : IntegrationTestSuite_Legacy
{
    private ChatHUDView view;
    
    protected override IEnumerator SetUp()
    {
        view = ChatHUDView.Create();
        yield break;
    }

    protected override IEnumerator TearDown()
    {
        view.Dispose();
        yield break;
    }

    [Test]
    public void CancelMessageSubmissionByEscapeKey()
    {
        const string testMessage = "test message";
        ChatMessage lastMsgSent = null;

        void SaveLastMessage(ChatMessage message) => lastMsgSent = message;
        view.OnSendMessage += SaveLastMessage;
        
        view.FocusInputField();
        view.inputField.text = testMessage;
        view.inputField.ProcessEvent(new UnityEngine.Event { keyCode = UnityEngine.KeyCode.Escape });
        view.inputField.onSubmit.Invoke(testMessage);
        
        Assert.AreEqual("", lastMsgSent.body);
        Assert.AreEqual(testMessage, view.inputField.text);
        
        view.OnSendMessage -= SaveLastMessage;
    }
}