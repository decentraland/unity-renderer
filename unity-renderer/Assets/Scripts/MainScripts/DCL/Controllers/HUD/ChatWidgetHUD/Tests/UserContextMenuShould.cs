using DCL.Interface;
using NUnit.Framework;
using System.Collections;
using UnityEngine;

public class UserContextMenuShould : TestsBase
{
    private const string CONTEXT_MENU_PREFAB_NAME = "ChatEntryContextMenu";
    private const string TEST_USER_ID = "test userId";
    private const string TEST_USER_NAME = "Test User";

    private UserContextMenu contextMenu;
    private Canvas canvas;
    private bool showEventInvoked;
    private bool passportEventInvoked;
    private bool reportEventInvoked;
    private bool blockEventInvoked;

    protected override IEnumerator SetUp()
    {
        var canvasgo = new GameObject("canvas");
        canvas = canvasgo.AddComponent<Canvas>();
        var go = Object.Instantiate(Resources.Load(CONTEXT_MENU_PREFAB_NAME), canvas.transform, false) as GameObject;

        contextMenu = go.GetComponent<UserContextMenu>();
        contextMenu.Initialize(TEST_USER_ID, TEST_USER_NAME, false);

        Assert.AreEqual(TEST_USER_NAME, contextMenu.userName.text, "The context menu title should coincide with the configured user.");

        contextMenu.OnShowMenu += ContextMenu_OnShowMenu;
        contextMenu.OnPassport += ContextMenu_OnPassport;
        contextMenu.OnReport += ContextMenu_OnReport;
        contextMenu.OnBlock += ContextMenu_OnBlock;

        yield break;
    }

    protected override IEnumerator TearDown()
    {
        contextMenu.OnShowMenu -= ContextMenu_OnShowMenu;
        contextMenu.OnPassport -= ContextMenu_OnPassport;
        contextMenu.OnReport -= ContextMenu_OnReport;
        contextMenu.OnBlock -= ContextMenu_OnBlock;

        Object.Destroy(contextMenu.gameObject);
        Object.Destroy(canvas.gameObject);

        yield break;
    }

    [Test]
    public void ShowContextMenuProperly()
    {
        showEventInvoked = false;
        contextMenu.Show();

        Assert.IsTrue(contextMenu.gameObject.activeSelf, "The context menu should be visible.");
        Assert.IsTrue(showEventInvoked);
    }

    [Test]
    public void HideContextMenuProperly()
    {
        contextMenu.Hide();

        Assert.IsFalse(contextMenu.gameObject.activeSelf, "The context menu should not be visible.");
    }

    [Test]
    public void ClickOnPassportButton()
    {
        passportEventInvoked = false;
        contextMenu.Show();
        contextMenu.passportButton.onClick.Invoke();

        Assert.IsTrue(passportEventInvoked);
        Assert.IsFalse(contextMenu.gameObject.activeSelf, "The context menu should not be visible.");
    }

    [Test]
    public void ClickOnReportButton()
    {
        reportEventInvoked = false;
        contextMenu.Show();
        contextMenu.reportButton.onClick.Invoke();

        Assert.IsTrue(reportEventInvoked);
        Assert.IsFalse(contextMenu.gameObject.activeSelf, "The context menu should not be visible.");
    }

    [Test]
    public void ClickOnBlockButton()
    {
        blockEventInvoked = false;
        contextMenu.Show();
        contextMenu.blockButton.onClick.Invoke();

        Assert.IsTrue(blockEventInvoked);
        Assert.IsFalse(contextMenu.gameObject.activeSelf, "The context menu should not be visible.");
    }

    private void ContextMenu_OnShowMenu()
    {
        showEventInvoked = true;
    }

    private void ContextMenu_OnPassport(string obj)
    {
        passportEventInvoked = true;
    }

    private void ContextMenu_OnReport(string obj)
    {
        reportEventInvoked = true;
    }

    private void ContextMenu_OnBlock(string arg1, bool arg2)
    {
        blockEventInvoked = true;
    }
}
