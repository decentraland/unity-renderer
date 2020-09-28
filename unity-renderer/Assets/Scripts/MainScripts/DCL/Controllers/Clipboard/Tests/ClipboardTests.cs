using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

public class ClipboardTests
{
    [UnityTest]
    public IEnumerator ReadClipboardPromiseShouldBehaveCorrectly()
    {
        ClipboardHandler_Mock mockClipboardHandler = new ClipboardHandler_Mock();
        Clipboard clipboard = new Clipboard(mockClipboardHandler);

        const string firstText = "sometext";
        mockClipboardHandler.MockReadTextRequestResult(0.5f, firstText, false);

        var promise = clipboard.ReadText();
        promise.Then(value =>
            {
                Assert.IsNull(promise.error);
                Assert.IsNotNull(promise.value);
                Assert.IsTrue(value == firstText);
            })
            .Catch(error =>
            {
                Assert.Fail("it shouldn't call this");
            });
        yield return promise;

        Assert.IsNull(promise.error);
        Assert.IsNotNull(promise.value);
        Assert.IsTrue(promise.value == firstText);

        const string errorText = "errortext";
        mockClipboardHandler.MockReadTextRequestResult(0, errorText, true);

        promise = clipboard.ReadText();
        promise.Then(value =>
            {
                Assert.Fail("it shouldn't call this");
            })
            .Catch(error =>
            {
                Assert.IsNull(promise.value);
                Assert.IsNotNull(promise.error);
                Assert.IsTrue(error == errorText);
            });

        Assert.IsNull(promise.value);
        Assert.IsNotNull(promise.error);
        Assert.IsTrue(promise.error == errorText);
    }
}