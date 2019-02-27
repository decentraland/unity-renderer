using System.Collections;
using System.Collections.Generic;
using System.IO;
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

            Scene originalTestScene = SceneManager.GetActiveScene();

            yield return SceneManager.LoadSceneAsync(visualTestsSceneName, LoadSceneMode.Additive);

            Scene visualTestsScene = SceneManager.GetSceneByName(visualTestsSceneName);
            SceneManager.SetActiveScene(visualTestsScene);

            // ---------------------------------------------
            // TAKE SNAPSHOTS

            yield return new WaitForSeconds(2f);

            string currentTestImagesPath = Application.dataPath + "/../TestResources/VisualTests/Test1/CurrentTestImages/";
            string snapshot1Name = "snapshot_1.png";
            string snapshot2Name = "snapshot_2.png";
            string snapshot3Name = "snapshot_3.png";
            string snapshot4Name = "snapshot_4.png";

            // Create Camera & environment
            Camera visualTestsCam = new GameObject("VisualTestsCam").AddComponent<Camera>();
            visualTestsCam.depth = 11;
            int snapshotsWidth = TestSettings.VISUAL_TESTS_SNAPSHOT_WIDTH;
            int snapshotsHeight = TestSettings.VISUAL_TESTS_SNAPSHOT_HEIGHT;

            yield return new WaitForEndOfFrame();

            // Take 1st snapshot
            VisualTestHelpers.RepositionVisualTestsCamera(visualTestsCam.transform, new Vector3(10f, 10f, 0f));
            yield return VisualTestHelpers.TakeSnapshot(currentTestImagesPath, snapshot1Name, visualTestsCam, snapshotsWidth, snapshotsHeight);

            // Take 2nd snapshot
            yield return new WaitForSeconds(0.2f);
            VisualTestHelpers.RepositionVisualTestsCamera(visualTestsCam.transform, new Vector3(0f, 10f, 10f));
            yield return VisualTestHelpers.TakeSnapshot(currentTestImagesPath, snapshot2Name, visualTestsCam, snapshotsWidth, snapshotsHeight);

            // Take 3rd snapshot
            yield return new WaitForSeconds(0.2f);
            VisualTestHelpers.RepositionVisualTestsCamera(visualTestsCam.transform, new Vector3(-10f, 10f, 0f));
            yield return VisualTestHelpers.TakeSnapshot(currentTestImagesPath, snapshot3Name, visualTestsCam, snapshotsWidth, snapshotsHeight);

            // Take 4th snapshot
            yield return new WaitForSeconds(0.2f);
            VisualTestHelpers.RepositionVisualTestsCamera(visualTestsCam.transform, new Vector3(0f, 10f, -10f));
            yield return VisualTestHelpers.TakeSnapshot(currentTestImagesPath, snapshot4Name, visualTestsCam, snapshotsWidth, snapshotsHeight);

            yield return new WaitForSeconds(0.2f);
            GameObject.DestroyImmediate(visualTestsCam.gameObject);

            // ---------------------------------------------
            // COMPARE IMAGES

            string baselineImagesPath = Application.dataPath + "/../TestResources/VisualTests/Test1/BaselineImages/";

            Texture2D baselineSnapshot1 = new Texture2D(snapshotsWidth, snapshotsHeight, TextureFormat.RGB24, false);
            baselineSnapshot1.LoadImage(File.ReadAllBytes(baselineImagesPath + snapshot1Name));

            Texture2D baselineSnapshot2 = new Texture2D(snapshotsWidth, snapshotsHeight, TextureFormat.RGB24, false);
            baselineSnapshot2.LoadImage(File.ReadAllBytes(baselineImagesPath + snapshot2Name));

            Texture2D baselineSnapshot3 = new Texture2D(snapshotsWidth, snapshotsHeight, TextureFormat.RGB24, false);
            baselineSnapshot3.LoadImage(File.ReadAllBytes(baselineImagesPath + snapshot3Name));

            Texture2D baselineSnapshot4 = new Texture2D(snapshotsWidth, snapshotsHeight, TextureFormat.RGB24, false);
            baselineSnapshot4.LoadImage(File.ReadAllBytes(baselineImagesPath + snapshot4Name));

            Texture2D currentSnapshot1 = new Texture2D(snapshotsWidth, snapshotsHeight, TextureFormat.RGB24, false);
            currentSnapshot1.LoadImage(File.ReadAllBytes(currentTestImagesPath + snapshot1Name));

            Texture2D currentSnapshot2 = new Texture2D(snapshotsWidth, snapshotsHeight, TextureFormat.RGB24, false);
            currentSnapshot2.LoadImage(File.ReadAllBytes(currentTestImagesPath + snapshot2Name));

            Texture2D currentSnapshot3 = new Texture2D(snapshotsWidth, snapshotsHeight, TextureFormat.RGB24, false);
            currentSnapshot3.LoadImage(File.ReadAllBytes(currentTestImagesPath + snapshot3Name));

            Texture2D currentSnapshot4 = new Texture2D(snapshotsWidth, snapshotsHeight, TextureFormat.RGB24, false);
            currentSnapshot4.LoadImage(File.ReadAllBytes(currentTestImagesPath + snapshot4Name));

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

            // ---------------------------------------------
            // UNLOAD VISUAL TESTS SCENE
            SceneManager.SetActiveScene(originalTestScene);
            yield return SceneManager.UnloadSceneAsync(visualTestsScene);
        }
    }
}
