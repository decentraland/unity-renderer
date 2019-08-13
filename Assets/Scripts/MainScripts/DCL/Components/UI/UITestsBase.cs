using DCL.Helpers;
using System.Collections;
using UnityEngine;

namespace Tests
{
    public class UITestsBase : TestsBase
    {
        protected override IEnumerator InitScene(bool usesWebServer = false, bool spawnCharController = true, bool spawnTestScene = true, bool spawnUIScene = true)
        {
            yield return base.InitScene(usesWebServer, spawnCharController, spawnTestScene, spawnUIScene);

            if (spawnCharController)
            {
                DCLCharacterController.i.gravity = 0f;
                TestHelpers.SetCharacterPosition(Vector3.zero);
            }
        }
    }
}
