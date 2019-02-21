using System.Collections;
using System.Collections.Generic;
using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using DCL.Configuration;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

namespace Tests
{
    public class VisualTests
    {
        [UnityTest]
        [Explicit]
        public IEnumerator VisualTest1()
        {
            // ---------------------------------------------
            // LOAD VISUAL TESTS SCENE
            // By doing this we can configure that scene to show whatever we want, with the environment we want,
            // take the baseline snapshots there and use that exact configuration here without having to replicate it manually on code somehow
            string visualTestsSceneName = "VisualTest1";

            yield return SceneManager.LoadSceneAsync(visualTestsSceneName);

            // ---------------------------------------------
            // TAKE SNAPSHOTS

            yield return new WaitForSeconds(2f);

            string outputPath = "/Resources/VisualTests/Test1/CurrentTestImages/";
            string snapshot1Name = "snapshot_1";
            string snapshot2Name = "snapshot_2";
            string snapshot3Name = "snapshot_3";
            string snapshot4Name = "snapshot_4";

            // Create Camera & environment
            Camera visualTestsCam = new GameObject("VisualTestsCam").AddComponent<Camera>();
            visualTestsCam.depth = 11;

            yield return new WaitForEndOfFrame();

            // Take 1st snapshot
            VisualTestHelpers.RepositionVisualTestsCamera(visualTestsCam.transform, new Vector3(10f, 10f, 0f));
            yield return VisualTestHelpers.TakeSnapshot(outputPath + snapshot1Name + ".png", visualTestsCam);

            // Take 2nd snapshot
            yield return new WaitForSeconds(0.2f);
            VisualTestHelpers.RepositionVisualTestsCamera(visualTestsCam.transform, new Vector3(0f, 10f, 10f));
            yield return VisualTestHelpers.TakeSnapshot(outputPath + snapshot2Name + ".png", visualTestsCam);

            // Take 3rd snapshot
            yield return new WaitForSeconds(0.2f);
            VisualTestHelpers.RepositionVisualTestsCamera(visualTestsCam.transform, new Vector3(-10f, 10f, 0f));
            yield return VisualTestHelpers.TakeSnapshot(outputPath + snapshot3Name + ".png", visualTestsCam);

            // Take 4th snapshot
            yield return new WaitForSeconds(0.2f);
            VisualTestHelpers.RepositionVisualTestsCamera(visualTestsCam.transform, new Vector3(0f, 10f, -10f));
            yield return VisualTestHelpers.TakeSnapshot(outputPath + snapshot4Name + ".png", visualTestsCam);

            yield return new WaitForSeconds(0.2f);
            GameObject.DestroyImmediate(visualTestsCam.gameObject);

            // ---------------------------------------------
            // COMPARE IMAGES
            string baselineImagesPath = "VisualTests/Test1/BaselineImages/";
            string currentTestImagesPath = "VisualTests/Test1/CurrentTestImages/";

            Texture2D baselineSnapshot1 = Resources.Load<Texture2D>(baselineImagesPath + snapshot1Name);
            Texture2D baselineSnapshot2 = Resources.Load<Texture2D>(baselineImagesPath + snapshot2Name);
            Texture2D baselineSnapshot3 = Resources.Load<Texture2D>(baselineImagesPath + snapshot3Name);
            Texture2D baselineSnapshot4 = Resources.Load<Texture2D>(baselineImagesPath + snapshot4Name);

            Texture2D currentSnapshot1 = Resources.Load<Texture2D>(currentTestImagesPath + snapshot1Name);
            Texture2D currentSnapshot2 = Resources.Load<Texture2D>(currentTestImagesPath + snapshot2Name);
            Texture2D currentSnapshot3 = Resources.Load<Texture2D>(currentTestImagesPath + snapshot3Name);
            Texture2D currentSnapshot4 = Resources.Load<Texture2D>(currentTestImagesPath + snapshot4Name);

            float imageAffinity = VisualTestHelpers.GetImageAffinityPercentage(baselineSnapshot1, currentSnapshot1, currentTestImagesPath + "snapshot_diff_1.png");
            Debug.Log(snapshot1Name + " affinity: " + imageAffinity + "%");

            imageAffinity = VisualTestHelpers.GetImageAffinityPercentage(baselineSnapshot2, currentSnapshot2, currentTestImagesPath + "snapshot_diff_2.png");
            Debug.Log(snapshot2Name + " affinity: " + imageAffinity + "%");

            imageAffinity = VisualTestHelpers.GetImageAffinityPercentage(baselineSnapshot3, currentSnapshot3, currentTestImagesPath + "snapshot_diff_3.png");
            Debug.Log(snapshot3Name + " affinity: " + imageAffinity + "%");

            imageAffinity = VisualTestHelpers.GetImageAffinityPercentage(baselineSnapshot4, currentSnapshot4, currentTestImagesPath + "snapshot_diff_4.png");
            Debug.Log(snapshot4Name + " affinity: " + imageAffinity + "%");

            Assert.GreaterOrEqual(imageAffinity, TestSettings.VISUAL_TESTS_APPROVED_AFFINITY, $"{snapshot1Name} has {imageAffinity}% affinity, the minimum is {TestSettings.VISUAL_TESTS_APPROVED_AFFINITY}%, A diff image has been generated to check the differences.");
            Assert.GreaterOrEqual(imageAffinity, TestSettings.VISUAL_TESTS_APPROVED_AFFINITY, $"{snapshot2Name} has {imageAffinity}% affinity, the minimum is {TestSettings.VISUAL_TESTS_APPROVED_AFFINITY}%, A diff image has been generated to check the differences.");
            Assert.GreaterOrEqual(imageAffinity, TestSettings.VISUAL_TESTS_APPROVED_AFFINITY, $"{snapshot3Name} has {imageAffinity}% affinity, the minimum is {TestSettings.VISUAL_TESTS_APPROVED_AFFINITY}%, A diff image has been generated to check the differences.");
            Assert.GreaterOrEqual(imageAffinity, TestSettings.VISUAL_TESTS_APPROVED_AFFINITY, $"{snapshot4Name} has {imageAffinity}% affinity, the minimum is {TestSettings.VISUAL_TESTS_APPROVED_AFFINITY}%, A diff image has been generated to check the differences.");
        }
    }
}
