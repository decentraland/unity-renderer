using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using DCL;
using UnityEngine;
using NSubstitute;
using UnityEngine.TestTools;

namespace Tests
{
    public class NavmapTests : IntegrationTestSuite_Legacy
    {
        private MinimapHUDController controller;
        DCL.NavmapView navmapView;

        protected override List<GameObject> SetUp_LegacySystems()
        {
            List<GameObject> result = new List<GameObject>();
            result.Add(MainSceneFactory.CreateNavMap());
            return result;
        }


        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();

            controller = new MinimapHUDController(Substitute.For<MinimapMetadataController>(), Substitute.For<IHomeLocationController>());
            controller.Initialize();
            navmapView = Object.FindObjectOfType<NavmapView>();
        }

        protected override IEnumerator TearDown()
        {
            controller.Dispose();
            yield return base.TearDown();
        }

        [UnityTest]
        [Explicit]
        [Category("Explicit")]
        public IEnumerator Toggle()
        {
            InputAction_Trigger action = null;
            var inputController = GameObject.FindObjectOfType<InputController>();
            for (int i = 0; i < inputController.triggerTimeActions.Length; i++)
            {
                // Find the open nav map action used by the input controller
                if (inputController.triggerTimeActions[i].GetDCLAction() == DCLAction_Trigger.ToggleNavMap)
                {
                    action = inputController.triggerTimeActions[i];
                    break;
                }
            }

            Assert.IsNotNull(action);

            action.RaiseOnTriggered();

            yield return null;

            Assert.IsTrue(navmapView.scrollRect.gameObject.activeSelf);

            action.RaiseOnTriggered();

            yield return null;

            Assert.IsFalse(navmapView.scrollRect.gameObject.activeSelf);
        }

        [Test]
        public void ReactToPlayerCoordsChange()
        {
            const string sceneName = "SCENE_NAME";
            MinimapMetadata.GetMetadata()
                .AddSceneInfo(
                    new MinimapMetadata.MinimapSceneInfo
                    {
                        parcels = new List<Vector2Int>
                        {
                            new Vector2Int(-77, -77)
                        },
                        name = sceneName
                    });
            CommonScriptableObjects.playerCoords.Set(new Vector2Int(-77, -77));
            Assert.AreEqual(sceneName, navmapView.currentSceneNameText.text);
            Assert.AreEqual("-77,-77", navmapView.currentSceneCoordsText.text);
        }
    }
}