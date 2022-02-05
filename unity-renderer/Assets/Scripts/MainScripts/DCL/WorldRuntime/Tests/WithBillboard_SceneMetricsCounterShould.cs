using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

public class WithBillboard_SceneMetricsCounterShould : IntegrationTestSuite_SceneMetricsCounter
{
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

    [UnityTest]
    public IEnumerator NotCountIgnoredEntitiesWithBillboard()
    {
        Assert.Fail();
        yield return null;
    }
}