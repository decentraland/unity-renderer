using DCL.Configuration;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Components
{
    public class UIScreenSpace : UIShape
    {
        static bool VERBOSE = false;

        const string GLOBAL_VISIBILITY_TOGGLE_PATH = "GlobalVisibilityToggle";
        public Canvas canvas;

        private static bool globalVisibilityToggleValue = true;
        private static bool globalVisibilityToggle
        {
            get => globalVisibilityToggleValue;
            set
            {
                globalVisibilityToggleValue = value;
                OnUIGlobalVisibilityToggleChanged.Invoke();
            }
        }

        private static Action OnUIGlobalVisibilityToggleChanged = () => { };
        private static Toggle toggle;

        private DCLCharacterPosition currentCharacterPosition;
        private CanvasGroup canvasGroup;

        public UIScreenSpace(ParcelScene scene) : base(scene)
        {
            DCLCharacterController.OnCharacterMoved += OnCharacterMoved;

            //Only no-dcl scenes are listening the the global visibility event
            if (!scene.isPersistent)
            {
                OnUIGlobalVisibilityToggleChanged += UpdateCanvasVisibility;
            }
        }

        public override void AttachTo(DecentralandEntity entity, System.Type overridenAttachedType = null)
        {
            Debug.LogError(
                "Aborted UIScreenShape attachment to an entity. UIShapes shouldn't be attached to entities.");
        }

        public override void DetachFrom(DecentralandEntity entity, System.Type overridenAttachedType = null)
        {
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            model = SceneController.i.SafeFromJson<Model>(newJson);

            if (scene.uiScreenSpace == null)
            {
                scene.uiScreenSpace = this;

                InitializeCanvas();
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
            OnUIGlobalVisibilityToggleChanged -= UpdateCanvasVisibility;

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

        private void UpdateCanvasVisibility()
        {
            if (canvas != null && scene != null)
            {
                bool isInsideSceneBounds = scene.IsInsideSceneBoundaries(Utils.WorldToGridPosition(currentCharacterPosition.worldPosition));
                bool shouldBeVisible = scene.isPersistent || (model.visible && isInsideSceneBounds && globalVisibilityToggle);
                canvasGroup.alpha = shouldBeVisible ? 1f : 0f;
                canvasGroup.blocksRaycasts = shouldBeVisible;
            }

            UpdateToggleVisibility();
        }

        private void UpdateToggleVisibility()
        {
            if (toggle != null)
            {
                if (scene.isPersistent)
                    toggle.gameObject.SetActive(false);
                else
                    toggle.gameObject.SetActive(toggle.gameObject.activeSelf || scene.IsInsideSceneBoundaries(Utils.WorldToGridPosition(currentCharacterPosition.worldPosition)));
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
            canvasGameObject.transform.SetParent(scene.transform);
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
            canvasGameObject.AddComponent<GraphicRaycaster>();

            if (scene.isPersistent)
            {
                childHookRectTransform = canvas.GetComponent<RectTransform>();

                // we make sure DCL UI renders above every parcel UI
                canvas.sortingOrder = 1;
            }
            else
            {
                // "Constrained" panel mask (to avoid rendering parcels UI on the viewport's top 10%)
                GameObject constrainedPanel = new GameObject("ConstrainedPanel");
                constrainedPanel.AddComponent<RectMask2D>();
                childHookRectTransform = constrainedPanel.GetComponent<RectTransform>();
                childHookRectTransform.SetParent(canvas.transform);
                childHookRectTransform.ResetLocalTRS();
                childHookRectTransform.anchorMin = Vector2.zero;
                childHookRectTransform.anchorMax = Vector2.one;
                childHookRectTransform.sizeDelta = Vector2.zero;

                // We scale the panel downwards to release the viewport's top 10%
                childHookRectTransform.pivot = new Vector2(0.5f, 0f);
                childHookRectTransform.localScale = new Vector3(1f, 1f - (UISettings.RESERVED_CANVAS_TOP_PERCENTAGE / 100), 1f);
            }

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

            //Only for the DCL UI scene
            if (scene.isPersistent)
            {
                CreateGlobalVisibilityToggle();
            }
            else
            {
                UpdateCanvasVisibility();
            }
        }


        private void CreateGlobalVisibilityToggle()
        {
            GameObject toggleGameObject = UnityEngine.Object.Instantiate(Resources.Load(GLOBAL_VISIBILITY_TOGGLE_PATH), childHookRectTransform) as GameObject;
            if (toggleGameObject == null)
            {
                Debug.Log("Cannot find Global Visibility Toggle");
                return;
            }

            toggle = toggleGameObject.GetComponent<Toggle>();
            if (toggle == null)
            {
                Debug.Log("Global Visibility Toggle contains no toggle");
                return;
            }

            toggle.onValueChanged.AddListener((x) => globalVisibilityToggle = x);
            toggle.isOn = true;

            toggle.gameObject.SetActive(false);
        }
    }
}
