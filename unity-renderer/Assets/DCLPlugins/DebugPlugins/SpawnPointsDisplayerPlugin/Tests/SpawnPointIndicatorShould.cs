using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Tests
{
    public class SpawnPointIndicatorShould
    {
        private SpawnPointIndicatorMonoBehaviour spawnPointGO;

        [SetUp]
        public void SetUp()
        {
            var resource = Resources.Load<SpawnPointIndicatorMonoBehaviour>(SpawnPointIndicatorInstantiator.RESOURCE_PATH);
            spawnPointGO = Object.Instantiate(resource);
        }

        [TearDown]
        public void TearDown()
        {
            if (!spawnPointGO.isDestroyed)
            {
                Object.Destroy(spawnPointGO.gameObject);
            }
        }

        [UnityTest]
        public IEnumerator DisposeCorrectly()
        {
            var indicator = new SpawnPointIndicator(spawnPointGO);
            ((IDisposable)indicator).Dispose();
            yield return null;
            Assert.IsTrue(spawnPointGO.isDestroyed);
        }

        [Test]
        public void SetPositionCorrectly()
        {
            ISpawnPointIndicator indicator = new SpawnPointIndicator(spawnPointGO);
            Vector3 position = new Vector3(2, 0, 1);
            indicator.SetPosition(position);

            Assert.AreEqual(position, spawnPointGO.areaIndicator.position);
            Assert.AreEqual(Vector3.zero, spawnPointGO.areaIndicator.localPosition);
        }

        [Test]
        public void SetSizeCorrectly()
        {
            ISpawnPointIndicator indicator = new SpawnPointIndicator(spawnPointGO);
            Vector3 size = new Vector3(2, 1, 1);
            indicator.SetSize(size);

            var expectedTextPosition = new Vector3(0, 0.5f, 0);
            var expectedArrowPosition = new Vector3(0, 0.5f, 0);

            Assert.AreEqual(size, spawnPointGO.areaIndicator.localScale);
            Assert.AreEqual(expectedTextPosition.ToString(), spawnPointGO.areaTextTransform.localPosition.ToString());
            Assert.AreEqual(expectedArrowPosition.ToString(), spawnPointGO.lookAtIndicator.localPosition.ToString());
        }

        [Test]
        public void SetRotationCorrectly()
        {
            ISpawnPointIndicator indicator = new SpawnPointIndicator(spawnPointGO);
            Quaternion rotation = Quaternion.Euler(0, 90, 0);
            indicator.SetRotation(rotation);

            Assert.AreEqual(rotation.eulerAngles, spawnPointGO.lookAtIndicator.rotation.eulerAngles);
            Assert.IsTrue(spawnPointGO.lookAtIndicator.gameObject.activeSelf);

            indicator.SetRotation(null);
            Assert.IsFalse(spawnPointGO.lookAtIndicator.gameObject.activeSelf);
        }
    }
}