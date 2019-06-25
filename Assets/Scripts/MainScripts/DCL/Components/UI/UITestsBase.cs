using DCL.Helpers;
using System.Collections;
using UnityEngine;

namespace Tests
{
    public class UITestsBase : TestsBase
    {
        protected override IEnumerator InitScene(bool usesWebServer = false, bool spawnCharController = true)
        {
            yield return base.InitScene(usesWebServer, spawnCharController);

            if (spawnCharController)
            {
                DCLCharacterController.i.gravity = 0f;

                TestHelpers.SetCharacterPosition(Vector3.zero);
            }
        }
    }
}
