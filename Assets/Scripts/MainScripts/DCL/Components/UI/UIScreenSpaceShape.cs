using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DCL.Helpers;
using DCL.Controllers;
using DCL.Models;

namespace DCL.Components.UI
{
    public class ScreenSpaceShape : UIShape
    {
        static bool VERBOSE = false;
        public override string componentName => "UIScreenSpaceShape";

        public ScreenSpaceShape(ParcelScene scene) : base(scene)
        {
            DCLCharacterController.OnCharacterMoved += OnCharacterMoved;
        }

        public override void AttachTo(DecentralandEntity entity)
        {
            Debug.LogError("Aborted UIScreenShape attachment to an entity. UIShapes shouldn't be attached to entities.");
        }

        public override void DetachFrom(DecentralandEntity entity)
        {
        }


        public override IEnumerator ApplyChanges(string newJson)
        {
            model = Utils.SafeFromJson<Model>(newJson);

            if (scene.uiScreenSpaceCanvas == null)
            {
                yield return InitializeCanvas();
            }

            transform = scene.uiScreenSpaceCanvas.GetComponent<RectTransform>();

            if (DCLCharacterController.i != null)
                OnCharacterMoved(DCLCharacterController.i.transform.position);
        }

        public override void Dispose()
        {
            DCLCharacterController.OnCharacterMoved -= OnCharacterMoved;

            if (transform != null)
                Utils.SafeDestroy(transform.gameObject);
        }

        void OnCharacterMoved(Vector3 newCharacterPosition)
        {
            if (scene.uiScreenSpaceCanvas != null)
            {
                scene.uiScreenSpaceCanvas.enabled = model.visible && scene.IsInsideSceneBoundaries(newCharacterPosition);

                if (VERBOSE)
                    Debug.Log($"set screenspace = {scene.uiScreenSpaceCanvas.enabled}... {newCharacterPosition}");
            }
        }

        IEnumerator InitializeCanvas()
        {
            if (VERBOSE)
                Debug.Log("Started canvas initialization in " + id);

            GameObject canvasGameObject = new GameObject("UIScreenSpaceShape");
            canvasGameObject.transform.SetParent(scene.transform);
            canvasGameObject.transform.ResetLocalTRS();

            // Canvas
            scene.uiScreenSpaceCanvas = canvasGameObject.AddComponent<Canvas>();
            scene.uiScreenSpaceCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

            // Canvas Scaler (for maintaining ui aspect ratio)
            CanvasScaler canvasScaler = canvasGameObject.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1280f, 720f);
            canvasScaler.matchWidthOrHeight = 1f; // Match height, recommended for landscape projects

            // Graphics Raycaster (for allowing touch/click input on the ui components)
            canvasGameObject.AddComponent<GraphicRaycaster>();

            transform = scene.uiScreenSpaceCanvas.GetComponent<RectTransform>();

            scene.uiScreenSpaceCanvas.gameObject.SetActive(false);
            scene.uiScreenSpaceCanvas.gameObject.SetActive(true);

            // we enable the canvas for 2 frames to force its auto-scaling
            yield return null;
            yield return null;

            if (VERBOSE)
            {
                Debug.Log("canvas initialized, width: " + transform.rect.width);
                Debug.Log("canvas initialized, height: " + transform.rect.height);
            }

            scene.uiScreenSpaceCanvas.enabled = false; // It will be enabled later when the player enters this scene

            if (DCLCharacterController.i != null)
                OnCharacterMoved(DCLCharacterController.i.transform.position);

            if (VERBOSE)
                Debug.Log("Finished canvas initialization in " + id);
        }
    }
}
