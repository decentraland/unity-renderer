using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using DCL.Models;
using UnityEngine.TestTools;
using Newtonsoft.Json;

namespace Tests {
  public class IntegrationTests {
    [UnityTest]
    public IEnumerator PlayMode_TestSceneIntegration1() {
      var GO = new GameObject();
      var sceneController = GO.AddComponent<SceneController>();
      var testScene = GO.AddComponent<IntegrationTestController>();

      yield return new WaitForSeconds(0.01f); // We wait to let unity creates

      testScene.Verify();

      Object.DestroyImmediate(GO);
    }

    SceneController GetOrInitializeSceneController() {
      var sceneController = Object.FindObjectOfType<SceneController>();

      if (sceneController == null) {
        sceneController = Resources.Load<GameObject>("Prefabs/SceneController").GetComponent<SceneController>();
      }

      sceneController.UnloadAllScenes();

      return sceneController;
    }
  }


}
