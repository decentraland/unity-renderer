using System.Collections;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using UnityEngine.TestTools;

public class WithAudioClip_SceneMetricsCounterShould : IntegrationTestSuite_SceneMetricsCounter
{
    [UnityTest]
    public IEnumerator CountAudioClipMemorySize()
    {
        var entity = CreateEntityWithTransform();
        yield return TestUtils.CreateAudioSourceWithClipForEntity(entity);

        Assert.That(scene.metricsCounter.currentCount.audioClipMemory, Is.EqualTo(5117752));

        var audioClip = scene.GetSharedComponent("audioClipTest");
        audioClip.Dispose();

        Assert.That(scene.metricsCounter.currentCount.audioClipMemory, Is.EqualTo(0));

        yield return null;
    }
}