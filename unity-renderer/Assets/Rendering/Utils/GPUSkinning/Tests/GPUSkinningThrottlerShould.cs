using System.Collections;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GPUSkinning;
using NSubstitute;
using NUnit.Framework;
using UnityEngine.TestTools;

public class GPUSkinningThrottlerShould
{
    private GPUSkinningThrottler throttler;
    private IGPUSkinning gpuSkinning;

    [SetUp]
    public void SetUp()
    {
        GPUSkinningThrottler.startingFrame = 0;
        gpuSkinning = Substitute.For<IGPUSkinning>();
        throttler = new GPUSkinningThrottler();
        throttler.Bind(gpuSkinning);
    }

    [Test]
    [TestCase(1, 10, 10)]
    [TestCase(2, 10, 5)]
    [TestCase(4, 10, 2)]
    public async Task CallWaitForFramesProperly(int framesBetweenUpdates, int framesToCheck, int expectedCalls)
    {
        throttler.SetThrottling(framesBetweenUpdates);

        throttler.Start();
        for (int i = 0; i < framesToCheck; i++)
            await UniTask.WaitForEndOfFrame();

        gpuSkinning.Received(expectedCalls).Update();
    }

    [Test]
    public async Task StopProperly()
    {
        throttler.SetThrottling(1);

        throttler.Start();
        for (int i = 0; i < 3; i++)
            await UniTask.WaitForEndOfFrame();

        throttler.Stop();
        for (int i = 0; i < 3; i++)
            await UniTask.WaitForEndOfFrame();

        gpuSkinning.Received(3).Update();
    }

    [Test]
    public async Task StopWhenDisposed()
    {
        throttler.SetThrottling(1);

        throttler.Start();
        for (int i = 0; i < 3; i++)
            await UniTask.WaitForEndOfFrame();

        throttler.Dispose();
        for (int i = 0; i < 3; i++)
            await UniTask.WaitForEndOfFrame();

        gpuSkinning.Received(3).Update();
    }

    [TearDown]
    public void TearDown() { throttler.Dispose(); }
}