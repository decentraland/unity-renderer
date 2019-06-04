using DCL;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class IntegrationTests
    {
        [UnityTest]
        public IEnumerator IntegrationTest_SceneIntegration1()
        {
            var GO = new GameObject();
            var testScene = GO.AddComponent<IntegrationTestController>();

            yield return null; // We wait to let unity create

            yield return testScene.Initialize();

            testScene.Verify();

            Object.DestroyImmediate(GO);
        }

        SceneController GetOrInitializeSceneController()
        {
            var sceneController = Object.FindObjectOfType<SceneController>();

            if (sceneController == null)
            {
                sceneController = new GameObject().AddComponent<SceneController>();
            }

            sceneController.UnloadAllScenes();

            return sceneController;
        }
    }
}
