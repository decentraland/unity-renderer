using DCL.Models;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;

namespace SceneBoundariesCheckerTests
{

    public class SceneBoundariesCheckerTests : TestsBase
    {
        [UnityTest]
        [Category("Explicit")]
        [Explicit("Scene boundaries checked in production is disabled for now")]
        public IEnumerator PShapeIsInvalidatedWhenStartingOutOfBounds()
        {
            yield return SBC_Asserts.PShapeIsInvalidatedWhenStartingOutOfBounds(scene);
        }

        [UnityTest]
        [Category("Explicit")]
        [Explicit("Scene boundaries checked in production is disabled for now")]
        public IEnumerator GLTFShapeIsInvalidatedWhenStartingOutOfBounds()
        {
            yield return SBC_Asserts.GLTFShapeIsInvalidatedWhenStartingOutOfBounds(scene);
        }

        [UnityTest]
        [Category("Explicit")]
        [Explicit("Too slow")]
        public IEnumerator NFTShapeIsInvalidatedWhenStartingOutOfBounds()
        {
            yield return SBC_Asserts.NFTShapeIsInvalidatedWhenStartingOutOfBounds(scene);
        }

        [UnityTest]
        [Category("Explicit")]
        [Explicit("Scene boundaries checked in production is disabled for now")]
        public IEnumerator PShapeIsInvalidatedWhenLeavingBounds()
        {
            yield return SBC_Asserts.PShapeIsInvalidatedWhenLeavingBounds(scene);
        }



        [UnityTest]
        [Category("Explicit")]
        [Explicit("Scene boundaries checked in production is disabled for now")]
        public IEnumerator GLTFShapeIsInvalidatedWhenLeavingBounds()
        {
            yield return SBC_Asserts.GLTFShapeIsInvalidatedWhenLeavingBounds(scene);
        }



        [UnityTest]
        [Category("Explicit")]
        [Explicit("Scene boundaries checked in production is disabled for now")]
        public IEnumerator NFTShapeIsInvalidatedWhenLeavingBounds()
        {
            yield return SBC_Asserts.NFTShapeIsInvalidatedWhenLeavingBounds(scene);
        }

        [Category("Explicit")]
        [Explicit("Scene boundaries checked in production is disabled for now")]
        public IEnumerator PShapeIsResetWhenReenteringBounds()
        {
            yield return SBC_Asserts.PShapeIsResetWhenReenteringBounds(scene);
        }



        [Category("Explicit")]
        [Explicit("Scene boundaries checked in production is disabled for now")]
        public IEnumerator GLTFShapeIsResetWhenReenteringBounds()
        {
            yield return SBC_Asserts.GLTFShapeIsResetWhenReenteringBounds(scene);
        }




        [UnityTest]
        [Category("Explicit")]
        [Explicit("Too slow")]
        public IEnumerator NFTShapeIsResetWhenReenteringBounds()
        {
            yield return SBC_Asserts.NFTShapeIsResetWhenReenteringBounds(scene);
        }



        [Category("Explicit")]
        [Explicit("Scene boundaries checked in production is disabled for now")]
        public IEnumerator ChildShapeIsEvaluated()
        {
            yield return SBC_Asserts.ChildShapeIsEvaluated(scene);
        }


        [Category("Explicit")]
        [Explicit("Scene boundaries checked in production is disabled for now")]
        public IEnumerator ChildShapeIsEvaluatedOnShapelessParent()
        {
            yield return SBC_Asserts.ChildShapeIsEvaluatedOnShapelessParent(scene);
        }

        [Category("Explicit")]
        [Explicit("Scene boundaries checked in production is disabled for now")]
        public IEnumerator HeightIsEvaluated()
        {
            yield return SBC_Asserts.HeightIsEvaluated(scene);
        }



        public bool MeshIsInvalid(DecentralandEntity.MeshesInfo meshesInfo)
        {
            return SBC_Asserts.MeshIsInvalid(meshesInfo);
        }

    }
}
