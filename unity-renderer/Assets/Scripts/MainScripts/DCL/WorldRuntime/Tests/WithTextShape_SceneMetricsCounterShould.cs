using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

public class WithTextShape_SceneMetricsCounterShould : IntegrationTestSuite_SceneMetricsCounter
{
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
        yield return null;
    }

    [UnityTest]
    public IEnumerator CountWhenRemoved()
    {
        Assert.Fail();
        yield return null;
    }
}