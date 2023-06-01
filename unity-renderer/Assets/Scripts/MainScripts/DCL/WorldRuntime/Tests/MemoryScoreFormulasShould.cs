using System.Collections;
using DCL;
using DCL.Helpers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.TestTools;

public class MemoryScoreFormulasShould
{
    [Test]
    public void OvershootTextureMemoryWhenScoreIsComputed()
    {
        var texture = new Texture2D(100, 100);
        texture.Apply(false, true);
        long score = MetricsScoreUtils.ComputeTextureScore(texture);

        // NOTE(Brian): We have to divide the expectancy by 2 because on editor/standalone runtime the
        //              reported memory is doubled when it shouldn't.
        Assert.That(score, Is.GreaterThan(Profiler.GetRuntimeMemorySizeLong(texture) / 2));
        Object.Destroy(texture);
    }

    [Test]
    public void OvershootAudioClipMemoryWhenScoreIsComputed()
    {
        var audioClip = AudioClip.Create("test", 10000, 2, 11000, false);
        long score = MetricsScoreUtils.ComputeAudioClipScore(audioClip);

        // NOTE(Brian): We have to divide the expectancy by 2 because on editor/standalone runtime the
        //              reported memory is doubled when it shouldn't.
        Assert.That(score, Is.GreaterThan(Profiler.GetRuntimeMemorySizeLong(audioClip) / 2));
        Object.Destroy(audioClip);
    }
}
