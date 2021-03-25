using DCL.Configuration;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Components
{
    public class UIScreenSpace : UIShape
    {
        static bool VERBOSE = false;

        public Canvas canvas;
        public GraphicRaycaster graphicRaycaster;

        private DCLCharacterPosition currentCharacterPosition;
        private CanvasGroup canvasGroup;

        public UIScreenSpace()
        {
            DCLCharacterController.OnCharacterMoved += OnCharacterMoved;
            model = new Model();
        }

        public override int GetClassId()
        {
            return (int) CLASS_ID.UI_SCREEN_SPACE_SHAPE;
        }

        public override void AttachTo(IDCLEntity entity, System.Type overridenAttachedType = null)
        {
            Debug.LogError(
                "Aborted UIScreenShape attachment to an entity. UIShapes shouldn't be attached to entities.");
        }

        public override void DetachFrom(IDCLEntity entity, System.Type overridenAttachedType = null)
        {
        }

        private bool initialized = false;

        public override IEnumerator ApplyChanges(BaseModel newModel)
        {
            var model = (Model) newModel;

            if (!initialized)
            {
                InitializeCanvas();
                initialized = true;
            }
            else if (DCLCharacterController.i != null)
            {
                OnCharacterMoved(DCLCharacterController.i.characterPosition);
            }

            //We have to wait a frame for the Canvas Scaler to act
            yield return null;
        }

        public override void Dispose()
        {
            DCLCharacterController.OnCharacterMoved -= OnCharacterMoved;
            CommonScriptableObjects.allUIHidden.OnChange -= AllUIHidden_OnChange;

            if (childHookRectTransform != null)
            {
                Utils.SafeDestroy(childHookRectTransform.gameObject);
            }
        }

        void OnCharacterMoved(DCLCharacterPosition newCharacterPosition)
        {
            if (canvas != null)
            {
                currentCharacterPosition = newCharacterPosition;

                UpdateCanvasVisibility();

                if (VERBOSE)
                {
                    Debug.Log($"set screenspace = {currentCharacterPosition}");
                }
            }
        }

        private void AllUIHidden_OnChange(bool current, bool previous)
        {
            UpdateCanvasVisibility();
        }

        private void UpdateCanvasVisibility()
        {
            if (canvas != null && scene != null)
            {
                var model = (Model) this.model;

                bool isInsideSceneBounds = scene.IsInsideSceneBoundaries(Utils.WorldToGridPosition(currentCharacterPosition.worldPosition));
                bool shouldBeVisible = scene.isPersistent || (model.visible && isInsideSceneBounds && !CommonScriptableObjects.allUIHidden.Get());
                canvasGroup.alpha = shouldBeVisible ? 1f : 0f;
                canvasGroup.blocksRaycasts = shouldBeVisible;
            }
        }

        void InitializeCanvas()
        {
            if (VERBOSE)
            {
                Debug.Log("Started canvas initialization in " + id);
            }

            GameObject canvasGameObject = new GameObject("UIScreenSpace");
            canvasGameObject.layer = LayerMask.NameToLayer("UI");
            canvasGameObject.transform.SetParent(scene.GetSceneTransform());
            canvasGameObject.transform.ResetLocalTRS();

            // Canvas
            canvas = canvasGameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            // Canvas Scaler (for maintaining ui aspect ratio)
            CanvasScaler canvasScaler = canvasGameObject.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1280f, 720f);
            canvasScaler.matchWidthOrHeight = 1f; // Match height, recommended for landscape projects

            // Graphics Raycaster (for allowing touch/click input on the ui components)
            graphicRaycaster = canvasGameObject.AddComponent<GraphicRaycaster>();

            canvas.sortingOrder = -1;

            // We create a middleman-gameobject to change the size of the parcel-devs accessible canvas, to have its bottom limit at the taskbar height, etc.
            GameObject resizedPanel = new GameObject("ResizeUIArea");

            resizedPanel.AddComponent<CanvasRenderer>();
            childHookRectTransform = resizedPanel.AddComponent<RectTransform>();
            childHookRectTransform.SetParent(canvas.transform);
            childHookRectTransform.ResetLocalTRS();

            childHookRectTransform.anchorMin = Vector2.zero;
            childHookRectTransform.anchorMax = new Vector2(1, 0);

            // We scale the panel downwards to subtract the viewport's top 10%
            float canvasHeight = canvasScaler.referenceResolution.y;
            childHookRectTransform.pivot = new Vector2(0.5f, 0f);
            float canvasSubtraction = canvasHeight * UISettings.RESERVED_CANVAS_TOP_PERCENTAGE / 100;
            childHookRectTransform.sizeDelta = new Vector2(0, canvasHeight - canvasSubtraction);

            // We scale the panel upwards to subtract the viewport's bottom 5% for Decentraland's taskbar
            canvasHeight = childHookRectTransform.sizeDelta.y;
            childHookRectTransform.pivot = new Vector2(0.5f, 1f);
            childHookRectTransform.anchoredPosition = new Vector3(0, canvasHeight, 0f);
            childHookRectTransform.sizeDelta = new Vector2(0, canvasHeight - canvasSubtraction / 2);

            // Canvas group
            canvasGroup = canvas.gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f; // Alpha will be updated later when the player enters this scene
            canvasGroup.blocksRaycasts = false;

            if (VERBOSE)
            {
                Debug.Log("canvas initialized, width: " + childHookRectTransform.rect.width);
                Debug.Log("canvas initialized, height: " + childHookRectTransform.rect.height);
            }

            if (DCLCharacterController.i != null)
            {
                OnCharacterMoved(DCLCharacterController.i.characterPosition);
            }

            if (VERBOSE)
            {
                Debug.Log("Finished canvas initialization in " + id);
            }

            if (!scene.isPersistent)
            {
                UpdateCanvasVisibility();
                CommonScriptableObjects.allUIHidden.OnChange += AllUIHidden_OnChange;
            }
        }
    }
}