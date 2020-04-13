using DCL.Configuration;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;

namespace DCL.Helpers
{
    public static class VisualTestHelpers
    {
        public static string testImagesPath = Application.dataPath + "/../TestResources/VisualTests/CurrentTestImages/";

        public static string baselineImagesPath =
            Application.dataPath + "/../TestResources/VisualTests/BaselineImages/";

        private static bool generateBaseline = false;
        public static string currentTestName;
        public static int snapshotIndex;

        public static IEnumerator GenerateBaselineForTest(IEnumerator test)
        {
            generateBaseline = true;
            yield return test;
            generateBaseline = false;
        }

        public static IEnumerator InitVisualTestsScene(string testName)
        {
            currentTestName = testName;
            snapshotIndex = 0;

            DCLCharacterController.i.PauseGravity();
            DCLCharacterController.i.enabled = false;

            // Position character inside parcel (0,0)
            TestHelpers.SetCharacterPosition(new Vector3(0, 2f, 0f));

            yield return new WaitForSeconds(2.0f);
        }

        /// <summary>
        /// This coroutine will take a visual test snapshot positioning the camera from shotPosition and pointing at shotTarget.
        /// Used in tandem with GenerateBaselineForTest(), TakeSnapshot will also generate the baseline test images.
        /// 
        /// Snapshot name will be generated dinamically using the name set in InitVisualTestsScene() and an static counter.
        /// </summary>
        /// <param name="shotPosition">camera will be placed here.</param>
        /// <param name="shotTarget">camera will point towards here.</param>
        public static IEnumerator TakeSnapshot(Vector3? shotPosition = null, Vector3? shotTarget = null)
        {
            yield return TakeSnapshot(currentTestName + "_" + snapshotIndex + ".png", shotPosition, shotTarget);
            snapshotIndex++;
        }

        /// <summary>
        /// This coroutine will take a visual test snapshot positioning the camera from shotPosition and pointing at shotTarget.
        /// Used in tandem with GenerateBaselineForTest(), TakeSnapshot will also generate the baseline test images.
        /// </summary>
        /// <param name="snapshotName">name used for saving the visual test file</param>
        /// <param name="shotPosition">camera will be placed here.</param>
        /// <param name="shotTarget">camera will point towards here.</param>
        public static IEnumerator TakeSnapshot(string snapshotName, Vector3? shotPosition = null, Vector3? shotTarget = null)
        {
            if (shotPosition.HasValue || shotTarget.HasValue)
            {
                RepositionVisualTestsCamera(shotPosition, shotTarget);
            }

            yield return null;
            yield return null;

            int snapshotsWidth = TestSettings.VISUAL_TESTS_SNAPSHOT_WIDTH;
            int snapshotsHeight = TestSettings.VISUAL_TESTS_SNAPSHOT_HEIGHT;

            if (generateBaseline || !File.Exists(baselineImagesPath + snapshotName))
            {
                yield return TakeSnapshot(baselineImagesPath, snapshotName, VisualTestController.i.camera,
                    snapshotsWidth, snapshotsHeight);
            }
            else
            {
                yield return TakeSnapshot(testImagesPath, snapshotName, VisualTestController.i.camera, snapshotsWidth, snapshotsHeight);

                float ratio =
                    ComputeImageAffinityPercentage(baselineImagesPath + snapshotName, testImagesPath + snapshotName);
                Assert.IsTrue(ratio > TestSettings.VISUAL_TESTS_APPROVED_AFFINITY,
                    $"{snapshotName} has {ratio}% affinity, the minimum is {TestSettings.VISUAL_TESTS_APPROVED_AFFINITY}%. A diff image has been generated. Check it out at {testImagesPath}");
            }
        }

        /// <summary>
        /// This coroutine will take a visual test snapshot using the camera provided, with the given image size.
        /// The image will be saved to disk as png.
        /// </summary>
        /// <param name="snapshotPath">Path to the directory where the image will be saved. Will be created if not exists.</param>
        /// <param name="snapshotName">output filename, it should include the png extension</param>
        /// <param name="camera">camera used to take the shot</param>
        /// <param name="width">Width of the final image</param>
        /// <param name="height">Height of the final image</param>
        public static IEnumerator TakeSnapshot(string snapshotPath, string snapshotName, Camera camera, int width,
            int height)
        {
            if (string.IsNullOrEmpty(snapshotName) || camera == null)
            {
                Debug.Log("snapshot name or camera is not valid. Snapshot aborted.");
                yield break;
            }

            var previousQualityLevel = QualitySettings.GetQualityLevel();
            QualitySettings.SetQualityLevel((int)QualityLevel.Good, true);

            string finalPath = snapshotPath + snapshotName;

            if (File.Exists(finalPath))
            {
                File.Delete(finalPath);

                // Just in case, wait until the file is deleted
                yield return new DCL.WaitUntil(() => { return !File.Exists(finalPath); }, 10f);
            }

            // We should only read the screen buffer after rendering is complete
            yield return null;

            RenderTexture renderTexture = new RenderTexture(width, height, 24);
            camera.targetTexture = renderTexture;
            camera.Render();

            RenderTexture.active = renderTexture;
            Texture2D currentSnapshot = new Texture2D(width, height, TextureFormat.RGB24, false);
            currentSnapshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            currentSnapshot.Apply();

            yield return null;

            if (!Directory.Exists(snapshotPath))
            {
                Directory.CreateDirectory(snapshotPath);
            }

            byte[] bytes = currentSnapshot.EncodeToPNG();
            File.WriteAllBytes(finalPath, bytes);

            // Just in case, wait until the file is created
            yield return new DCL.WaitUntil(() => { return File.Exists(finalPath); }, 10f);

            RenderTexture.active = null;
            renderTexture.Release();

            yield return new WaitForSeconds(0.2f);

            QualitySettings.SetQualityLevel(previousQualityLevel, true);
        }

        public static float ComputeImageAffinityPercentage(string baselineImagePathWithFilename,
            string testImagePathWithFilename)
        {
            Texture2D baselineSnapshot = new Texture2D(TestSettings.VISUAL_TESTS_SNAPSHOT_WIDTH,
                TestSettings.VISUAL_TESTS_SNAPSHOT_HEIGHT, TextureFormat.RGB24, false);
            baselineSnapshot.LoadImage(File.ReadAllBytes(baselineImagePathWithFilename));

            Texture2D currentSnapshot = new Texture2D(TestSettings.VISUAL_TESTS_SNAPSHOT_WIDTH,
                TestSettings.VISUAL_TESTS_SNAPSHOT_HEIGHT, TextureFormat.RGB24, false);
            currentSnapshot.LoadImage(File.ReadAllBytes(testImagePathWithFilename));

            string finalDiffPath = Path.GetDirectoryName(testImagePathWithFilename) + "/" +
                                   Path.GetFileNameWithoutExtension(testImagePathWithFilename) + "_diff" +
                                   Path.GetExtension(testImagePathWithFilename);

            return ComputeImageAffinityPercentage(baselineSnapshot, currentSnapshot, finalDiffPath);
        }

        /// <summary>
        /// This will compare the pixels of two images in order to make visual tests.
        /// </summary>
        /// <param name="baselineImage">Reference or "golden" image</param>
        /// <param name="testImage">Image to compare</param>
        /// <param name="diffImagePath"></param>
        /// <returns>Affinity percentage</returns>
        public static float ComputeImageAffinityPercentage(Texture2D baselineImage, Texture2D testImage,
            string diffImagePath)
        {
            baselineImage = DuplicateTextureAsReadable(baselineImage);
            testImage = DuplicateTextureAsReadable(testImage);

            if (string.IsNullOrEmpty(diffImagePath))
            {
                Debug.Log("diff image path is not valid. Image affinity percentage check aborted.");

                return -1;
            }

            if (baselineImage.width != testImage.width || baselineImage.height != testImage.height)
            {
                Debug.Log("CAN'T COMPARE IMAGES WITH DIFFERENT DIMENSIONS:");
                Debug.Log("baseline image dimensions: " + baselineImage.width + "," + baselineImage.height);
                Debug.Log("test image dimensions: " + testImage.width + "," + testImage.height);

                return -1;
            }

            Color32[] baselineImagePixels = baselineImage.GetPixels32();
            Color32[] testImagePixels = testImage.GetPixels32();
            Color32[] diffImagePixels = new Color32[testImagePixels.Length];
            Color32 diffColor = new Color32(255, 0, 0, 255);
            int differentPixels = 0;

            for (int i = 0; i < testImagePixels.Length; i++)
            {
                if (!IsSamePixel(testImagePixels[i], baselineImagePixels[i],
                    TestSettings.VISUAL_TESTS_PIXELS_CHECK_THRESHOLD))
                {
                    differentPixels++;
                    diffImagePixels[i] = diffColor;
                }
                else
                {
                    diffImagePixels[i] = baselineImagePixels[i];
                }
            }

            // Calculate Image Affinity
            float imageAffinity = ((testImagePixels.Length - differentPixels) * 100) / testImagePixels.Length;

            // Save diff image
            if (imageAffinity < TestSettings.VISUAL_TESTS_APPROVED_AFFINITY)
            {
                Texture2D diffImage = new Texture2D(baselineImage.width, baselineImage.height);
                diffImage.SetPixels32(diffImagePixels);
                diffImage.Apply();
                byte[] bytes = diffImage.EncodeToPNG();
                File.WriteAllBytes(diffImagePath, bytes);
            }
            else if (File.Exists(diffImagePath))
            {
                File.Delete(diffImagePath);

                if (File.Exists(diffImagePath + ".meta"))
                    File.Delete(diffImagePath + ".meta");
            }

            return imageAffinity;
        }

        public static Texture2D DuplicateTextureAsReadable(Texture2D source)
        {
            RenderTexture renderTex = RenderTexture.GetTemporary(
                source.width,
                source.height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear);

            Graphics.Blit(source, renderTex);

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;

            Texture2D readableText = new Texture2D(source.width, source.height);
            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableText.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);

            return readableText;
        }

        public static Texture2D ResizeTexture(Texture2D source, int newWidth, int newHeight)
        {
            source.filterMode = FilterMode.Point;

            RenderTexture renderTex = RenderTexture.GetTemporary(newWidth, newHeight);

            renderTex.filterMode = FilterMode.Point;
            RenderTexture.active = renderTex;
            Graphics.Blit(source, renderTex);

            Texture2D newTex = new Texture2D(newWidth, newHeight);
            newTex.ReadPixels(new Rect(0, 0, newWidth, newWidth), 0, 0);
            newTex.Apply();

            RenderTexture.active = null;

            return newTex;
        }

        public static bool IsSamePixel(Color32 pixelA, Color32 pixelB, float checkThreshold)
        {
            return (pixelA.r > pixelB.r - checkThreshold && pixelA.r < pixelB.r + checkThreshold) &&
                   (pixelA.g > pixelB.g - checkThreshold && pixelA.g < pixelB.g + checkThreshold) &&
                   (pixelA.b > pixelB.b - checkThreshold && pixelA.b < pixelB.b + checkThreshold);
        }

        public static void RepositionVisualTestsCamera(Transform cameraTransform, Vector3? position = null, Vector3? target = null)
        {
            if (position.HasValue)
            {
                cameraTransform.position = position.Value;
            }

            if (target.HasValue)
            {
                cameraTransform.forward = target.Value - cameraTransform.position;
            }
        }

        public static void RepositionVisualTestsCamera(Vector3? position = null, Vector3? target = null)
        {
            RepositionVisualTestsCamera(VisualTestController.i.camera.transform, position, target);
        }
    }
}
