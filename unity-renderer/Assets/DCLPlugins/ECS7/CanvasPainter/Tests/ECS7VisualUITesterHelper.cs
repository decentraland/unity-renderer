using System.Collections;
using System.Collections.Generic;
using System.IO;
using DCL.Helpers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

namespace DCL.ECS7.Tests
{
    public class ECS7VisualUITesterHelper
    {
        public static string testImagesPath = Application.dataPath + "/../TestResources/VisualTests/CurrentTestImages/";
        public static string baselineImagesPath =
            Application.dataPath + "/../TestResources/VisualTests/BaselineImages/";
        
        private Color[] testsColor;
        private GameObject mainGameObject;
        private GameObject canvasGameObject;
        private GameObject childrenGameObject;
        private GameObject testGameObject;
        private Camera camera;

        public ECS7VisualUITesterHelper()
        {
            testsColor = new [] { Color.red, Color.gray, Color.blue, Color.black, Color.magenta, Color.yellow,  Color.cyan   };
        }

        public void Setup(CanvasPainter canvasPainter)
        {
            // We setup the background colors in order to ensure that the images will look always the same
            mainGameObject = new GameObject("Camera GameObject");
            
            // Configure camera
            camera = mainGameObject.AddComponent<Camera>();
            camera.transform.position = camera.transform.position + UnityEngine.Vector3.back * 17;
            camera.backgroundColor = Color.white;
            camera.allowHDR = false;
            camera.clearFlags = CameraClearFlags.SolidColor;
                        
            // Configure canvas
            canvasGameObject = new GameObject("Canvas GameObject");

            var canvas = canvasGameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = camera;
            
            // Adding a canvas scaler for the canvas, this way we ensure that the image will maintain proportions 
            var canvasScaler = canvasGameObject.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1440, 900);

            var canvasRectTransform = canvasGameObject.GetComponent<RectTransform>();
            canvasRectTransform.anchorMin = new Vector2(1, 0);
            canvasRectTransform.anchorMax = new Vector2(0, 1);
            canvasRectTransform.pivot = new Vector2(0.5f, 0.5f);
            canvasRectTransform.sizeDelta = new Vector2(1440, 900);
            canvasRectTransform.position += UnityEngine.Vector3.forward * 850;

            // Create render texture
            RenderTexture renderTexture = new RenderTexture(1440, 900, 16);
            renderTexture.Create();
            canvasPainter.rootNode.panelSettings.targetTexture = renderTexture;
            
            
            // Configure RawImage
            childrenGameObject = new GameObject("RawImage GameObject");
            childrenGameObject.transform.SetParent(canvasGameObject.transform, false);
            
            var rawImage = childrenGameObject.AddComponent<RawImage>();
            
            var rawImageRectTransform = rawImage.GetComponent<RectTransform>();
            rawImageRectTransform.anchorMin = new Vector2(0, 0);
            rawImageRectTransform.anchorMax = new Vector2(1, 1);
            rawImageRectTransform.pivot = new Vector2(0.5f, 0.5f);
            rawImageRectTransform.sizeDelta = new Vector2(1440, 900);
            rawImageRectTransform.offsetMax = Vector2.zero;
            rawImageRectTransform.offsetMin = Vector2.zero;
            
            // rawImage.texture = renderTexture;
            rawImage.color = Color.blue;
            
            testGameObject = new GameObject("Image GameObject");
            testGameObject.transform.SetParent(canvasGameObject.transform, false);

            Image image = testGameObject.AddComponent<Image>();
            image.color = Color.green;
        }

        public void SetupBackgroundColorsInOrder(List<VisualElementRepresentation> visualElements)
        {
            if (visualElements.Count > testsColor.Length)
            {
                Debug.LogError("There are more elements than colors in this test! You need to add " + (visualElements.Count - testsColor.Length) + " colors to do this test");
                return;
            }
            for (int i = 0; i < visualElements.Count; i++)
            {
                visualElements[i].visualElement.style.backgroundColor = testsColor[i];
            }
        }

        public IEnumerator TakeSnapshotAndAssert(string textureNameRaw)
        {
            string textureName = textureNameRaw + ".png";
            int snapshotsWidth = 1440;
            int snapshotsHeight = 900;
            float ratio = 95f;
    
			yield return new WaitForSeconds(10);
            yield return VisualTestUtils.TakeSnapshot(testImagesPath, textureName, camera, snapshotsWidth, snapshotsHeight);
            // yield return VisualTestUtils.TakeSnapshot(textureName, camera);
            
            float ratioResult =
                VisualTestUtils.ComputeImageAffinityPercentage(baselineImagesPath + textureName, testImagesPath + textureName);
            
                Assert.IsTrue(ratioResult >= ratio,
                    $"{Path.GetFileName(baselineImagesPath + textureName)} has {ratioResult}% affinity, the minimum is {ratio}%. A diff image has been generated. Check it out at {testImagesPath}");
            
        }

        public void Dispose()
        {
            GameObject.Destroy(mainGameObject);
            GameObject.Destroy(canvasGameObject);
            GameObject.Destroy(childrenGameObject);
            GameObject.Destroy(testGameObject);
        }
    }
}