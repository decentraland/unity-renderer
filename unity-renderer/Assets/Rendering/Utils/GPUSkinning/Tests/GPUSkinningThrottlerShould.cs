using System.Collections;
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
        throttler = new GPUSkinningThrottler(gpuSkinning);
    }

    [Test]
    [TestCase(1, 10)]
    [TestCase(2, 10)]
    [TestCase(4, 10)]
    public void WaitBetweenFrames_EveryFrame(int framesBetweenUpdates, int iterations)
    {
        throttler.SetThrottling(framesBetweenUpdates);

        for (int i = 0; i < iterations; i++)
            throttler.TryUpdate();

        int requiredNumberOfCalls = iterations / framesBetweenUpdates;
        gpuSkinning.Received(requiredNumberOfCalls).Update();
    }
}