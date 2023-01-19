using DCL;
using DCL.Rendering;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;

namespace CullingControllerTests
{
    public class CullingObjectsTrackerShould
    {
        private DataStore_WorldObjects data;

        
        [SetUp]
        public void SetUp()
        {
            data = new DataStore_WorldObjects();
            DataStore.i.Set(data);
        }

        [TearDown]
        public void TearDown()
        {
            data = null;
            DataStore.Clear();
        }

        [UnityTest]
        public IEnumerator FilterIgnoredLayerMasksCorrectly()
        {
            // Arrange
            DataStore.i.sceneWorldObjects.sceneData.Add(0, new DataStore_WorldObjects.SceneData());
            int layer = 5;
            int layerMask = 1 << layer;
            var tracker = new CullingObjectsTracker();
            tracker.SetIgnoredLayersMask(layerMask);

            var testGameObjectA = new GameObject();
            var originalRendererA = testGameObjectA.AddComponent<MeshRenderer>();
            DataStore.i.sceneWorldObjects.sceneData[0].renderers.Add(originalRendererA);
            testGameObjectA.layer = layer;

            var testGameObjectB = new GameObject();
            var originalRendererB = testGameObjectB.AddComponent<MeshRenderer>();
            DataStore.i.sceneWorldObjects.sceneData[0].renderers.Add(originalRendererB);
            testGameObjectB.layer = 0;

            var testGameObjectC = new GameObject();
            var originalRendererC = testGameObjectC.AddComponent<SkinnedMeshRenderer>();
            testGameObjectC.layer = layer;

            var testGameObjectD = new GameObject();
            var originalRendererD = testGameObjectD.AddComponent<SkinnedMeshRenderer>();
            testGameObjectD.layer = 0;

            IReadOnlyList<Renderer> renderers = null;
            Renderer obtainedRendererA = null, obtainedRendererB = null, obtainedRendererC = null, obtainedRendererD = null;

            // Act
            yield return tracker.PopulateRenderersList();

            renderers = tracker.GetRenderers();
            obtainedRendererA = renderers.FirstOrDefault(x => x == originalRendererA);
            obtainedRendererB = renderers.FirstOrDefault(x => x == originalRendererB);
            obtainedRendererC = tracker.GetSkinnedRenderers().FirstOrDefault(x => x == originalRendererC);
            obtainedRendererD = tracker.GetSkinnedRenderers().FirstOrDefault(x => x == originalRendererD);

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
