using System.Collections.Generic;
using DCL.ECSComponents;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class PrefabsShould
    {
        [Test]
        public void HaveCollidersSetupCorrectly()
        {
            IList<NFTShapeFrame> frames = GetNFTFrames();

            for (int i = 0; i < frames.Count; i++)
            {
                Assert.AreEqual(1, frames[i].GetComponentsInChildren<Collider>(true).Length);
                Assert.IsFalse(frames[i].boxCollider.enabled);
            }

            for (int i = 0; i < frames.Count; i++)
            {
                Object.DestroyImmediate(frames[i].gameObject);
            }
        }

        private static IList<NFTShapeFrame> GetNFTFrames()
        {
            var factory = Resources.Load<NFTShapeFrameFactory>("NFTShapeFrameFactory");
            int count = factory.loaderControllersPrefabs.Length;
            NFTShapeFrame[] frames = new NFTShapeFrame[count];
            for (int i = 0; i < frames.Length; i++)
            {
                frames[i] = factory.InstantiateLoaderController(i);
            }
            return frames;
        }
    }
}