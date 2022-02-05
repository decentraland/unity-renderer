using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

public class WithBasicMaterial_SceneMetricsCounterShould : IntegrationTestSuite_SceneMetricsCounter
{
    [UnityTest]
    public IEnumerator NotCountBasicMaterialsWhenNoShapeIsPresent()
    {
        var material1 = CreateBasicMaterial("");

        yield return material1.routine;

        Assert.That( scene.metricsCounter.currentCount.materials, Is.EqualTo(0) );
    }


    [UnityTest]
    public IEnumerator NotCountWhenAttachedToIgnoredEntities()
    {
        Assert.Fail();
        yield return null;
    }


    [UnityTest]
    public IEnumerator CountWhenAdded()
    {
        Assert.Fail();
        yield break;
    }

    [UnityTest]
    public IEnumerator CountWhenRemoved()
    {
        Assert.Fail();
        yield break;
    }
}