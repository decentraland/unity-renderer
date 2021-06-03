using System.Collections;
using System.Linq;
using DCL.Rendering;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace CullingControllerTests
{
    public class CullingObjectsTrackerShould
    {
        [UnityTest]
        public IEnumerator FilterIgnoredLayerMasksCorrectly()
        {
            // Arrange
            int layer = 5;
            int layerMask = 1 << layer;
            var tracker = new CullingObjectsTracker();
            tracker.SetIgnoredLayersMask(layerMask);

            var testGameObjectA = new GameObject();
            var originalRendererA = testGameObjectA.AddComponent<MeshRenderer>();
            testGameObjectA.layer = layer;

            var testGameObjectB = new GameObject();
            var originalRendererB = testGameObjectB.AddComponent<MeshRenderer>();
            testGameObjectB.layer = 0;

            var testGameObjectC = new GameObject();
            var originalRendererC = testGameObjectC.AddComponent<SkinnedMeshRenderer>();
            testGameObjectC.layer = layer;

            var testGameObjectD = new GameObject();
            var originalRendererD = testGameObjectD.AddComponent<SkinnedMeshRenderer>();
            testGameObjectD.layer = 0;

            Renderer[] renderers = null;
            Renderer obtainedRendererA = null, obtainedRendererB = null, obtainedRendererC = null, obtainedRendererD = null;

            // Act
            yield return tracker.PopulateRenderersList();

            renderers = tracker.GetRenderers();
            obtainedRendererA = renderers.FirstOrDefault( x => x == originalRendererA );
            obtainedRendererB = renderers.FirstOrDefault( x => x == originalRendererB );
            obtainedRendererC = tracker.GetSkinnedRenderers().FirstOrDefault( x => x == originalRendererC );
            obtainedRendererD = tracker.GetSkinnedRenderers().FirstOrDefault( x => x == originalRendererD );

            // Assert
            Assert.IsTrue(originalRendererA != null);
            Assert.IsTrue(originalRendererB != null);
            Assert.IsTrue(originalRendererC != null);
            Assert.IsTrue(originalRendererD != null);

            Assert.IsTrue(obtainedRendererA == null, "Renderer should be null because it has an invalid layer");
            Assert.IsTrue(obtainedRendererB != null, "Renderer should not be null because it has a valid layer");
            Assert.IsTrue(obtainedRendererC == null, "Skinned renderer should be null because it has a invalid layer");
            Assert.IsTrue(obtainedRendererD != null, "Skinned renderer should not be null because it has a valid layer");

            // Cleanup
            Object.Destroy(testGameObjectA);
            Object.Destroy(testGameObjectB);
            Object.Destroy(testGameObjectC);
            Object.Destroy(testGameObjectD);

            yield return null;
        }
    }
}