using DCL.Configuration;
using DCL.Helpers;
using DCL.Models;
using DCL.Providers;
using Decentraland.Sdk.Ecs6;
using MainScripts.DCL.Components;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Components
{
    public class UIScreenSpace : UIShape<UIScreenSpaceReferencesContainer, UIScreenSpace.Model>
    {
        [System.Serializable]
        public new class Model : UIShape.Model
        {
            public Canvas UiScreenSpaceCanvas;
            public Color color = Color.clear;

            public override BaseModel GetDataFromJSON(string json) =>
                Utils.SafeFromJson<Model>(json);

            public override BaseModel GetDataFromPb(ComponentBodyPayload pbModel)
            {
                if (pbModel.PayloadCase != ComponentBodyPayload.PayloadOneofCase.UiScreenSpaceShape)
                    return Utils.SafeUnimplemented<UIScreenSpace, Model>(expected: ComponentBodyPayload.PayloadOneofCase.UiScreenSpaceShape, actual: pbModel.PayloadCase);

                var pb = new Model();
                if (pbModel.UiScreenSpaceShape.HasName) pb.name = pbModel.UiScreenSpaceShape.Name;
                if (pbModel.UiScreenSpaceShape.HasParentComponent) pb.parentComponent = pbModel.UiScreenSpaceShape.ParentComponent;
                if (pbModel.UiScreenSpaceShape.HasVisible) pb.visible = pbModel.UiScreenSpaceShape.Visible;
                if (pbModel.UiScreenSpaceShape.HasOpacity) pb.opacity = pbModel.UiScreenSpaceShape.Opacity;
                if (pbModel.UiScreenSpaceShape.HasHAlign) pb.hAlign = pbModel.UiScreenSpaceShape.HAlign;
                if (pbModel.UiScreenSpaceShape.HasVAlign) pb.vAlign = pbModel.UiScreenSpaceShape.VAlign;
                if (pbModel.UiScreenSpaceShape.Width != null) pb.width = SDK6DataMapExtensions.FromProtobuf(pb.width, pbModel.UiScreenSpaceShape.Width);
                if (pbModel.UiScreenSpaceShape.Height != null) pb.height = SDK6DataMapExtensions.FromProtobuf(pb.height, pbModel.UiScreenSpaceShape.Height);
                if (pbModel.UiScreenSpaceShape.PositionX != null) pb.positionX = SDK6DataMapExtensions.FromProtobuf(pb.positionX, pbModel.UiScreenSpaceShape.PositionX);
                if (pbModel.UiScreenSpaceShape.PositionY != null) pb.positionY = SDK6DataMapExtensions.FromProtobuf(pb.positionY, pbModel.UiScreenSpaceShape.PositionY);
                if (pbModel.UiScreenSpaceShape.HasIsPointerBlocker) pb.isPointerBlocker = pbModel.UiScreenSpaceShape.IsPointerBlocker;

                return pb;
            }
        }

        static bool VERBOSE = false;

        public Canvas canvas;
        public GraphicRaycaster graphicRaycaster;

        private CanvasGroup canvasGroup;
        private bool isInsideSceneBounds;
        private BaseVariable<bool> isUIEnabled => DataStore.i.HUDs.isCurrentSceneUiEnabled;
        private HUDCanvasCameraModeController hudCanvasCameraModeController;
        private readonly DataStore_Player dataStorePlayer = DataStore.i.player;

        private bool initialized = false;

        public UIScreenSpace(UIShapePool pool, UIShapeScheduler scheduler) : base(pool, scheduler)
        {
            this.pool = pool;
            this.scheduler = scheduler;
            dataStorePlayer.playerGridPosition.OnChange += OnPlayerCoordinatesChanged;
            DataStore.i.HUDs.isCurrentSceneUiEnabled.OnChange += OnChangeSceneUI;
            OnChangeSceneUI(isUIEnabled.Get(), true);
            model = new Model();
        }

        public override int GetClassId() { return (int) CLASS_ID.UI_SCREEN_SPACE_SHAPE; }

        public override void AttachTo(IDCLEntity entity, System.Type overridenAttachedType = null)
        {
            Debug.LogError(
                "Aborted UIScreenShape attachment to an entity. UIShapes shouldn't be attached to entities.");
        }

        public override void DetachFrom(IDCLEntity entity, System.Type overridenAttachedType = null) { }

        public override IEnumerator ApplyChanges(BaseModel newModel)
        {
            if (!initialized)
            {
                InitializeCanvas();
                initialized = true;
            }
            else
            {
                OnPlayerCoordinatesChanged(dataStorePlayer.playerGridPosition.Get(), dataStorePlayer.playerGridPosition.Get());
            }

            //We have to wait a frame for the Canvas Scaler to act
            yield return null;
        }

        public override void Dispose()
        {
            hudCanvasCameraModeController?.Dispose();
            dataStorePlayer.playerGridPosition.OnChange -= OnPlayerCoordinatesChanged;
            DataStore.i.HUDs.isCurrentSceneUiEnabled.OnChange -= OnChangeSceneUI;
            CommonScriptableObjects.allUIHidden.OnChange -= AllUIHidden_OnChange;

            if (referencesContainer != null)
                pool.ReleaseUIShape(referencesContainer);

            if (childHookRectTransform)
                childHookRectTransform.GetComponent<UIReferencesContainer>()?.owner.Dispose();
        }

        void OnChangeSceneUI(bool current, bool previous)
        {
            UpdateCanvasVisibility();
        }

        void OnPlayerCoordinatesChanged(Vector2Int current, Vector2Int previous)
        {
            if (canvas == null)
                return;

            UpdateCanvasVisibility();

            if (VERBOSE)
            {
                Debug.Log($"set screenspace = {current}");
            }
        }

        private void AllUIHidden_OnChange(bool current, bool previous) { UpdateCanvasVisibility(); }

        private void UpdateCanvasVisibility()
        {
            if (canvas == null || scene == null)
                return;

            var model = (Model) this.model;

            isInsideSceneBounds = scene.IsInsideSceneBoundaries(dataStorePlayer.playerGridPosition.Get());

            if (isInsideSceneBounds)
            {
                DataStore.i.Get<DataStore_World>().currentRaycaster.Set(graphicRaycaster);
            }

            bool shouldBeVisible = model.visible && isInsideSceneBounds && !CommonScriptableObjects.allUIHidden.Get() && isUIEnabled.Get();

            referencesContainer.SetVisibility(shouldBeVisible);
            referencesContainer.SetBlockRaycast(shouldBeVisible);
        }

        private void InitializeCanvas()
        {
            if (VERBOSE)
            {
                Debug.Log("Started canvas initialization in " + id);
            }

            GameObject canvasGameObject = pool.TakeUIShapeInsideParent(scene.GetSceneTransform()).gameObject;
            canvasGameObject.transform.ResetLocalTRS();

            // Canvas
            canvas = referencesContainer.UiScreenSpaceCanvas;
            hudCanvasCameraModeController = new HUDCanvasCameraModeController(canvas, DataStore.i.camera.hudsCamera);

            // Canvas Scaler (for maintaining ui aspect ratio)
            CanvasScaler canvasScaler = referencesContainer.UiScreenSpaceCanvasScaler;

            // Graphics Raycaster (for allowing touch/click input on the ui components)
            graphicRaycaster = referencesContainer.UiScreenSpaceGraphicRaycaster;

            canvas.sortingOrder = -1;

            if (scene.isPersistent && scene.sceneData.sceneNumber != EnvironmentSettings.AVATAR_GLOBAL_SCENE_NUMBER)
            {
                canvas.sortingOrder -= 1;
            }

            // We create a middleman-gameobject to change the size of the parcel-devs accessible canvas, to have its bottom limit at the taskbar height, etc.
            // NOTE: We should probably load from Addressables but would require deeper refactoring
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
            canvasGroup = referencesContainer.canvasGroup;
            referencesContainer.SetVisibility(false);
            referencesContainer.SetBlockRaycast(false);

            if (VERBOSE)
            {
                Debug.Log("canvas initialized, width: " + childHookRectTransform.rect.width);
                Debug.Log("canvas initialized, height: " + childHookRectTransform.rect.height);
            }

            OnPlayerCoordinatesChanged(dataStorePlayer.playerGridPosition.Get(), dataStorePlayer.playerGridPosition.Get());

            if (VERBOSE)
            {
                Debug.Log("Finished canvas initialization in " + id);
            }

            UpdateCanvasVisibility();
            CommonScriptableObjects.allUIHidden.OnChange += AllUIHidden_OnChange;
        }

    }
}
