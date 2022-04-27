using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

public class PrivateChatWindowHUDViewShould
{
    [UnityTest]
    public IEnumerator RaiseEventWhenPressedBack()
    {
        var view = PrivateChatWindowHUDView.Create();
        bool pressedBack = false;
        view.OnPressBack += () => pressedBack = true;
        view.backButton.onClick.Invoke();
        Assert.IsTrue(pressedBack);
        yield break;
    }
}