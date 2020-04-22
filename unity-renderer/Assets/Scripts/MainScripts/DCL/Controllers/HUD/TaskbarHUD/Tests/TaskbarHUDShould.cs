using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;

public class TaskbarHUDShould : TestsBase
{
    private TaskbarHUDController controller;
    private TaskbarHUDView view;

    protected override IEnumerator SetUp()
    {
        controller = new TaskbarHUDController();
        view = TaskbarHUDView.Create(controller);
        controller.view = view;
        view.controller = controller;

        Assert.IsTrue(view != null, "Taskbar view is null?");
        yield break;
    }

    [UnityTest]
    public IEnumerator AddWindowsProperly()
    {
        WorldChatWindowHUDController chatWindowController = new WorldChatWindowHUDController();
        chatWindowController.Initialize(null, null);

        controller.AddChatWindow(chatWindowController);

        Assert.IsTrue(chatWindowController.view.transform.parent == view.windowContainer, "Chat window isn't inside taskbar window container!");
        Assert.IsTrue(chatWindowController.view.gameObject.activeSelf, "Chat window is disabled!");
        yield return null;
    }

    [UnityTest]
    public IEnumerator ToggleWindowsProperly()
    {
        WorldChatWindowHUDController chatWindowController = new WorldChatWindowHUDController();
        chatWindowController.Initialize(null, null);

        controller.AddChatWindow(chatWindowController);

        view.chatButton.onClick.Invoke();

        Assert.IsFalse(chatWindowController.view.gameObject.activeSelf);

        view.chatButton.onClick.Invoke();

        Assert.IsTrue(chatWindowController.view.gameObject.activeSelf);
        yield break;
    }
}
