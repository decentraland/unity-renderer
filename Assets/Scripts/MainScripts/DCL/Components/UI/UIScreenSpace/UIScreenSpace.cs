using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DCL.Helpers;
using DCL.Controllers;
using DCL.Models;

namespace DCL.Components
{
    public class UIScreenSpace : UIShape
    {
        static bool VERBOSE = false;
        public Canvas canvas;

        public UIScreenSpace(ParcelScene scene) : base(scene)
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

            if (scene.uiScreenSpace == null)
            {
                scene.uiScreenSpace = this;

                SceneController.i.StartCoroutine(InitializeCanvas());
            }
            else if (DCLCharacterController.i != null)
            {
                OnCharacterMoved(DCLCharacterController.i.transform.position);
            }

            return null;
        }

        public override void Dispose()
        {
            DCLCharacterController.OnCharacterMoved -= OnCharacterMoved;

            if (childHookRectTransform != null)
                Utils.SafeDestroy(childHookRectTransform.gameObject);
        }

        void OnCharacterMoved(Vector3 newCharacterPosition)
        {
            if (canvas != null)
            {
                canvas.enabled = scene.IsInsideSceneBoundaries(newCharacterPosition) && model.visible;

                if (VERBOSE)
                    Debug.Log($"set screenspace = {canvas.enabled}... {newCharacterPosition}");
            }
        }

        IEnumerator InitializeCanvas()
        {
            if (VERBOSE)
                Debug.Log("Started canvas initialization in " + id);

            GameObject canvasGameObject = new GameObject("UIScreenSpace");
            canvasGameObject.transform.SetParent(scene.transform);
            canvasGameObject.transform.ResetLocalTRS();

            canvas = canvasGameObject.AddComponent<Canvas>();
            // Canvas
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            // Canvas Scaler (for maintaining ui aspect ratio)
            CanvasScaler canvasScaler = canvasGameObject.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1280f, 720f);
            canvasScaler.matchWidthOrHeight = 1f; // Match height, recommended for landscape projects

            // Graphics Raycaster (for allowing touch/click input on the ui components)
            canvasGameObject.AddComponent<GraphicRaycaster>();

            childHookRectTransform = canvas.GetComponent<RectTransform>();

            CanvasGroup canvasGroup = canvas.gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;

            canvas.gameObject.SetActive(false);
            canvas.gameObject.SetActive(true);

            // we enable the canvas for 2 frames to force its auto-scaling
            yield return null;
            yield return null;

            canvasGroup.alpha = 1f;

            if (VERBOSE)
            {
                Debug.Log("canvas initialized, width: " + childHookRectTransform.rect.width);
                Debug.Log("canvas initialized, height: " + childHookRectTransform.rect.height);
            }

            if (canvas != null)
                canvas.enabled = false; // It will be enabled later when the player enters this scene

            if (DCLCharacterController.i != null)
                OnCharacterMoved(DCLCharacterController.i.transform.position);

            if (VERBOSE)
                Debug.Log("Finished canvas initialization in " + id);
        }
    }
}
