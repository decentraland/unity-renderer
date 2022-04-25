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

        Assert.That(scene.metricsCounter.currentCount.audioClipMemoryScore, Is.EqualTo(2560376));

        var audioClip = scene.componentsManagerLegacy.GetSceneSharedComponent("audioClipTest");
        audioClip.Dispose();

        Assert.That(scene.metricsCounter.currentCount.audioClipMemoryScore, Is.EqualTo(0));

        yield return null;
    }
}