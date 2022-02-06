using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

public class WithTextShape_SceneMetricsCounterShould : IntegrationTestSuite_SceneMetricsCounter
{
    [UnityTest]
    [Explicit]
    [Category("Explicit")]
    public IEnumerator NotCountWhenAttachedToIgnoredEntities()
    {
        // TODO(Brian): Implement when TextShape metrics support is implemented
        Assert.Fail();
        yield break;
    }

    [UnityTest]
    [Explicit]
    [Category("Explicit")]
    public IEnumerator CountWhenAdded()
    {
        // TODO(Brian): Implement when TextShape metrics support is implemented
        Assert.Fail();
        yield break;
    }

    [UnityTest]
    [Explicit]
    [Category("Explicit")]
    public IEnumerator CountWhenRemoved()
    {
        // TODO(Brian): Implement when TextShape metrics support is implemented
        Assert.Fail();
        yield break;
    }
}