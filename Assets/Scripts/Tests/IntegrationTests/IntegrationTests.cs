using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using DCL.Models;
using UnityEngine.TestTools;
using Newtonsoft.Json;
using DCL.Helpers;

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
