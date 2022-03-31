using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

public class WithBillboard_SceneMetricsCounterShould : IntegrationTestSuite_SceneMetricsCounter
{
    [UnityTest]
    [Explicit]
    [Category("Explicit")]
    public IEnumerator CountWhenAdded()
    {
        // TODO(Brian): Implement when Billboard metrics support is implemented
        Assert.Fail();
        yield return null;
    }

    [UnityTest]
    [Explicit]
    [Category("Explicit")]
    public IEnumerator CountWhenRemoved()
    {
        // TODO(Brian): Implement when Billboard metrics support is implemented
        Assert.Fail();
        yield return null;
    }

    [UnityTest]
    [Explicit]
    [Category("Explicit")]
    public IEnumerator NotCountIgnoredEntitiesWithBillboard()
    {
        // TODO(Brian): Implement when Billboard metrics support is implemented
        Assert.Fail();
        yield return null;
    }
}