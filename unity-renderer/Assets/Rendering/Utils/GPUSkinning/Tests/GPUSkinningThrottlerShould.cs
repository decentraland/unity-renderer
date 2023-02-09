using System.Collections;
using Cysharp.Threading.Tasks;
using GPUSkinning;
using NSubstitute;
using NUnit.Framework;
using UnityEngine.TestTools;

public class GPUSkinningThrottlerShould
{
    private GPUSkinningThrottlerService throttler;
    private IGPUSkinning gpuSkinning;

    [SetUp]
    public void SetUp()
    {
        gpuSkinning = Substitute.For<IGPUSkinning>();
        throttler = GPUSkinningThrottlerService.Create(true);
        throttler.Register(gpuSkinning);
    }

    [UnityTest]
    [TestCase(1, 10, 10, ExpectedResult = null)]
    [TestCase(2, 10, 5, ExpectedResult = null)]
    [TestCase(5, 10, 2, ExpectedResult = null)]
    public IEnumerator CallWaitForFramesProperly(int framesBetweenUpdates, int framesToCheck, int expectedCalls) => UniTask.ToCoroutine(async () =>
    {
        throttler.ModifyThrottling(gpuSkinning, framesBetweenUpdates);
        for (int i = 0; i < framesToCheck; i++)
            await UniTask.WaitForEndOfFrame();

        gpuSkinning.Received(expectedCalls).Update();
    });

    [UnityTest]
    public IEnumerator StopProperly() => UniTask.ToCoroutine(async () =>
    {
        throttler.ModifyThrottling(gpuSkinning, 1);

        for (int i = 0; i < 3; i++)
            await UniTask.WaitForEndOfFrame();

        throttler.ForceStop();
        for (int i = 0; i < 3; i++)
            await UniTask.WaitForEndOfFrame();

        gpuSkinning.Received(3).Update();
    });

    [UnityTest]
    public IEnumerator StopWhenDisposed() => UniTask.ToCoroutine(async () =>
    {
        throttler.ModifyThrottling(gpuSkinning, 1);

        for (int i = 0; i < 3; i++)
            await UniTask.WaitForEndOfFrame();

        throttler.Dispose();
        for (int i = 0; i < 3; i++)
            await UniTask.WaitForEndOfFrame();

        gpuSkinning.Received(3).Update();
    });

    [TearDown]
    public void TearDown() { throttler.Dispose(); }
}
