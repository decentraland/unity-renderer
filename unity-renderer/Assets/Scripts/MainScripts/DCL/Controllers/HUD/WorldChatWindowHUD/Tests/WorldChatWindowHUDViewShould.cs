using NUnit.Framework;

public class WorldChatWindowHUDViewShould
{
    [Test]
    public void HandleMouseCatcherProperly()
    {
        var mouseCatcher = new MouseCatcher_Mock();
        mouseCatcher.RaiseMouseLock();
        var view = WorldChatWindowHUDView.Create();
        Assert.AreEqual(0, view.group.alpha);
    }
}