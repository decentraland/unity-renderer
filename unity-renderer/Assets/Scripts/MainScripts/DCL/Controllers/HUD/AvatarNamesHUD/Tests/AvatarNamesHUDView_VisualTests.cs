using System.Collections;
using System.Linq;
using AvatarNamesHUD;
using DCL;
using DCL.Components;
using DCL.Helpers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.AvatarNamesHUD
{
    public class AvatarNamesHUDView_VisualTests : VisualTestsBase
    {
        private const int MAX_AVATAR_NAMES = 3;
        private AvatarNamesHUDView hudView;

        protected override IEnumerator InitScene(bool spawnCharController = true, bool spawnTestScene = true, bool spawnUIScene = true, bool debugMode = false, bool reloadUnityScene = true)
        {
            yield return base.InitScene(spawnCharController, spawnTestScene, spawnUIScene, debugMode, reloadUnityScene);
            hudView = Object.Instantiate(Resources.Load<GameObject>("AvatarNamesHUD")).GetComponent<AvatarNamesHUDView>();
            hudView.Initialize(MAX_AVATAR_NAMES);
        }

        [UnityTest, VisualTest]
        [Explicit, Category("Explicit")]
        public IEnumerator VisualTest0_Generate() { yield return VisualTestHelpers.GenerateBaselineForTest(VisualTest0()); }

        [UnityTest, VisualTest]
        public IEnumerator VisualTest0()
        {
            yield return InitVisualTestsScene("AvatarNamesHUD_VisualTest0");

            //Set the canvas to CameraSpace so it gets renderer in the screenshot
            Canvas canvas = hudView.GetComponent<Canvas>();
            canvas.worldCamera = Camera.main;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;

            VisualTestHelpers.RepositionVisualTestsCamera(VisualTestController.i.camera, new Vector3(6, 1.2f, -1.5f), new Vector3(6, 1.2f, -1.5f) + Vector3.forward);

            BoxShape box0 = TestHelpers.CreateEntityWithBoxShape(scene, new Vector3(4, 0, 4), true);
            PlayerStatus user0 = new PlayerStatus { id = "user0", name = "The User0 in Box", worldPosition = box0.attachedEntities.First().gameObject.transform.position, isTalking = false };
            yield return box0.routine;
            hudView.TrackPlayer(user0);

            SphereShape sphere = TestHelpers.CreateEntityWithSphereShape(scene, new Vector3(8, 0, 8), true);
            PlayerStatus user1 = new PlayerStatus { id = "user1", name = "The User1 in Sphere", worldPosition = sphere.attachedEntities.First().gameObject.transform.position, isTalking = true };
            yield return sphere.routine;
            hudView.TrackPlayer(user1);

            yield return null;

            yield return new WaitForSeconds(30);

            yield return VisualTestHelpers.TakeSnapshot();
        }

        protected override IEnumerator TearDown()
        {
            if (hudView != null)
                Object.Destroy(hudView.gameObject);
            return base.TearDown();
        }
    }
}