using System.Collections;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.ECS7.Tests
{
    public class ECS7VisualUITesterHelper
    {
        private GameObject mainGameObject;
        private GameObject canvasGameObject;
        private GameObject childrenGameObject;
        private Camera camera;

        public void Setup(CanvasPainter canvasPainter)
        {
            mainGameObject = new GameObject("Camera GameObject");
            
            // Configure camera
            camera = mainGameObject.AddComponent<Camera>();
            camera.backgroundColor = Color.white;
            camera.allowHDR = false;
            camera.clearFlags = CameraClearFlags.SolidColor;
                        
            // Configure canvas
            canvasGameObject = new GameObject("Canvas GameObject");
            var canvas = canvasGameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
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
            
            rawImage.texture = renderTexture;
        }

        public IEnumerator TakeSnapshotAndAssert(string textureName)
        {
            yield return VisualTestUtils.TakeSnapshot(textureName, camera);
        }

        public void Dispose()
        {
            GameObject.Destroy(mainGameObject);
            GameObject.Destroy(canvasGameObject);
            GameObject.Destroy(childrenGameObject);
        }
    }
}