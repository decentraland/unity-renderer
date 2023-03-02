using System;
using DCL.Components;
using DCL.Configuration;
using DCL.Helpers;
using DCL.Interface;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using DCL.Controllers;
using DCL.Models;
using DCLPlugins.UUIDEventComponentsPlugin.UUIDComponent.Interfaces;
using MainScripts.DCL.Helpers.UIHelpers;
using static DCL.Interface.WebInterface.OnGlobalPointerEventPayload;
using Ray = UnityEngine.Ray;

namespace DCL
{
    public class PointerEventsController
    {
        private static bool renderingEnabled => CommonScriptableObjects.rendererState.Get();

        private readonly PointerHoverController pointerHoverController;

        private readonly IRaycastHandler raycastHandler = new RaycastHandler();
        private readonly PointerEventData uiGraphicRaycastPointerEventData = new (null);
        private readonly List<RaycastResult> uiGraphicRaycastResults = new ();

        private readonly InputController_Legacy inputControllerLegacy;
        private readonly MouseCatcher mouseCatcher;
        private readonly BaseVariable<GraphicRaycaster> worldDataRaycaster;

        private DataStore_ECS7 dataStoreEcs7 = DataStore.i.ecs7;

        private IPointerInputEvent pointerInputUpEvent;
        private Camera charCamera;

        private RaycastHitInfo lastPointerDownEventHitInfo;
        private GraphicRaycaster uiGraphicRaycaster;
        private RaycastHit hitInfo;

        private IRaycastPointerClickHandler clickHandler;

        private StandaloneInputModuleDCL eventSystemInputModule;

        private StandaloneInputModuleDCL eventSystemInputModuleLazy => eventSystemInputModule ??= (StandaloneInputModuleDCL)EventSystem.current?.currentInputModule;

        public PointerEventsController(InputController_Legacy inputControllerLegacy, InteractionHoverCanvasController hoverCanvas, MouseCatcher mouseCatcher, BaseVariable<GraphicRaycaster> worldDataRaycaster)
        {
            this.worldDataRaycaster = worldDataRaycaster;
            this.inputControllerLegacy = inputControllerLegacy;
            this.mouseCatcher = mouseCatcher;
            pointerHoverController = new PointerHoverController(inputControllerLegacy, hoverCanvas);

            pointerHoverController.OnPointerHoverStarts += SetHoverCursor;
            pointerHoverController.OnPointerHoverEnds += SetNormalCursor;

            foreach (var actionButton in WebInterface.ConcreteActionButtons)
                inputControllerLegacy.AddListener(actionButton, OnButtonEvent);

            RetrieveCamera();

            Environment.i.platform.updateEventHandler.AddListener(IUpdateEventHandler.EventType.Update, Update);
            Utils.OnCursorLockChanged += HandleCursorLockChanges;

            HideOrShowCursor(Utils.IsCursorLocked);
        }

        public void Dispose()
        {
            foreach (var actionButton in WebInterface.ConcreteActionButtons)
                inputControllerLegacy.RemoveListener(actionButton, OnButtonEvent);

            pointerHoverController.OnPointerHoverStarts -= SetHoverCursor;
            pointerHoverController.OnPointerHoverEnds -= SetNormalCursor;

            Environment.i.platform.updateEventHandler.RemoveListener(IUpdateEventHandler.EventType.Update, Update);
            Utils.OnCursorLockChanged -= HandleCursorLockChanges;
        }

        private void Update()
        {
            if (charCamera == null)
                RetrieveCamera();

            if (!CommonScriptableObjects.rendererState.Get() || charCamera == null)
                return;

            if (NeedToUnhoverWhileCursorUnlocked())
            {
                // New interaction model
                UnhoverLastHoveredObject();
                return;
            }

            // We use Physics.Raycast() instead of our raycastHandler.Raycast() as that one is slower, sometimes 2x, because it fetches info we don't need here
            Ray ray = Utils.IsCursorLocked ? GetRayFromCamera() : GetRayFromMouse();

            bool didHit = Physics.Raycast(ray, out hitInfo, Mathf.Infinity, PhysicsLayers.physicsCastLayerMaskWithoutCharacter);

            if (dataStoreEcs7.isEcs7Enabled)
                dataStoreEcs7.lastPointerRayHit.UpdateByHitInfo(hitInfo, didHit, ray);

            var uiIsBlocking = false;

            // NOTE: in case of a single scene loaded (preview or builder) sceneId is set to null when stepping outside
            if (didHit && IsValidCurrentScene())
            {
                GraphicRaycaster raycaster = worldDataRaycaster.Get();

                if (raycaster != null)
                {
                    uiGraphicRaycastPointerEventData.position = Utils.IsCursorLocked ? new Vector2(Screen.width / 2, Screen.height / 2) : Input.mousePosition;
                    uiGraphicRaycastResults.Clear();
                    raycaster.Raycast(uiGraphicRaycastPointerEventData, uiGraphicRaycastResults);
                    uiIsBlocking = uiGraphicRaycastResults.Count > 0;
                }
            }

            if (!didHit || uiIsBlocking)
            {
                clickHandler = null;
                UnhoverLastHoveredObject();

                return;
            }

            var raycastHandlerTarget = hitInfo.collider.GetComponent<IRaycastPointerHandler>();

            if (raycastHandlerTarget != null)
            {
                ResolveGenericRaycastHandlers(raycastHandlerTarget);
                UnhoverLastHoveredObject();

                return;
            }

            var target = CollidersManager.i.GetColliderInfo(hitInfo.collider, out var colliderInfo)
                ? colliderInfo.entity.gameObject
                : hitInfo.collider.gameObject;

            clickHandler = null;

            Type typeToUse = Utils.IsCursorLocked ? typeof(IPointerEvent) : typeof(IUnlockedCursorInputEvent);
            pointerHoverController.OnRaycastHit(hitInfo, colliderInfo, target, typeToUse);
        }

        private static bool IsValidCurrentScene()
        {
            IWorldState worldState = Environment.i.world.state;
            int currentSceneNumber = worldState.GetCurrentSceneNumber();
            return currentSceneNumber > 0 && worldState.ContainsScene(currentSceneNumber);
        }

        private static IList<IPointerInputEvent> GetPointerInputEvents(GameObject hitGameObject)
        {
            if (!Utils.IsCursorLocked || Utils.LockedThisFrame())
                return hitGameObject.GetComponentsInChildren<IAvatarOnPointerDown>();

            return hitGameObject.GetComponentsInChildren<IPointerInputEvent>();
        }

        private void ResolveGenericRaycastHandlers(IRaycastPointerHandler raycastHandlerTarget)
        {
            if (Utils.LockedThisFrame())
                return;

            bool mouseIsDown = Input.GetMouseButtonDown(0);
            bool mouseIsUp = Input.GetMouseButtonUp(0);

            switch (raycastHandlerTarget)
            {
                case IRaycastPointerDownHandler down:
                {
                    if (mouseIsDown)
                        down.OnPointerDown();

                    break;
                }
                case IRaycastPointerUpHandler up:
                {
                    if (mouseIsUp)
                        up.OnPointerUp();

                    break;
                }
                case IRaycastPointerClickHandler click:
                {
                    if (mouseIsDown)
                        clickHandler = click;

                    if (mouseIsUp)
                    {
                        if (clickHandler == click)
                            click.OnPointerClick();

                        clickHandler = null;
                    }

                    break;
                }
            }
        }

        private void UnhoverLastHoveredObject() =>
            pointerHoverController.ResetHoveredObject();

        private void RetrieveCamera()
        {
            if (charCamera == null)
                charCamera = Camera.main;
        }

        private Ray GetRayFromCamera() =>
            charCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        private Ray GetRayFromMouse() =>
            charCamera.ScreenPointToRay(Input.mousePosition);

        private bool NeedToUnhoverWhileCursorUnlocked() =>
            (!Utils.IsCursorLocked || Utils.LockedThisFrame()) &&
            (!CanRaycastWhileUnlocked() || !DataStore.i.featureFlags.flags.Get().IsFeatureEnabled("avatar_outliner"));

        void OnButtonEvent(WebInterface.ACTION_BUTTON buttonId, InputController_Legacy.EVENT evt, bool useRaycast, bool enablePointerEvent)
        {
            // TODO(Brian): We should remove this when we get a proper initialization layer

            if (!EnvironmentSettings.RUNNING_TESTS)
            {
                if (!renderingEnabled)
                    return;

                if (NeedToUnhoverWhileCursorUnlocked())
                {
                    // New interaction model
                    UnhoverLastHoveredObject();
                    return;
                }
            }

            if (charCamera == null)
            {
                RetrieveCamera();

                if (charCamera == null)
                    return;
            }

            var pointerEventLayer = PhysicsLayers.physicsCastLayerMaskWithoutCharacter; // Ensure characterController is being filtered
            int globalLayer = pointerEventLayer & ~PhysicsLayers.physicsCastLayerMask;

            if (evt == InputController_Legacy.EVENT.BUTTON_DOWN) ProcessButtonDown(buttonId, useRaycast, enablePointerEvent, pointerEventLayer, globalLayer);
            else if (evt == InputController_Legacy.EVENT.BUTTON_UP) ProcessButtonUp(buttonId, useRaycast, enablePointerEvent, pointerEventLayer, globalLayer);

            if (dataStoreEcs7.isEcs7Enabled && IsValidButtonId(buttonId))
                dataStoreEcs7.inputActionState[(int)buttonId] = evt == InputController_Legacy.EVENT.BUTTON_DOWN;
        }

        private bool IsValidButtonId(WebInterface.ACTION_BUTTON buttonId) =>
            buttonId >= 0 && (int)buttonId < dataStoreEcs7.inputActionState.Length;

        private void ProcessButtonUp(WebInterface.ACTION_BUTTON buttonId, bool useRaycast, bool enablePointerEvent,
            LayerMask pointerEventLayer, int globalLayer)
        {
            IWorldState worldState = Environment.i.world.state;

            int currentSceneNumber = worldState.GetCurrentSceneNumber();

            if (currentSceneNumber <= 0)
                return;

            RaycastHitInfo raycastGlobalLayerHitInfo;
            Ray ray = !Utils.IsCursorLocked || Utils.LockedThisFrame() ? GetRayFromMouse() : GetRayFromCamera();

            // Raycast for global pointer events
            worldState.TryGetScene(currentSceneNumber, out var loadedScene);

            RaycastResultInfo raycastInfoGlobalLayer = raycastHandler.Raycast(ray, charCamera.farClipPlane, globalLayer,
                loadedScene);

            raycastGlobalLayerHitInfo = raycastInfoGlobalLayer.hitInfo;

            RaycastResultInfo raycastInfoPointerEventLayer = null;

            if (pointerInputUpEvent != null || dataStoreEcs7.isEcs7Enabled)
            {
                // Raycast for pointer event components
                raycastInfoPointerEventLayer = raycastHandler.Raycast(ray, charCamera.farClipPlane, pointerEventLayer, loadedScene);
            }

            if (pointerInputUpEvent != null && raycastInfoPointerEventLayer != null)
            {
                bool isOnClickComponentBlocked =
                    IsBlockingOnClick(raycastInfoPointerEventLayer.hitInfo, raycastGlobalLayerHitInfo);

                bool isSameEntityThatWasPressed = AreCollidersFromSameEntity(raycastInfoPointerEventLayer.hitInfo,
                    lastPointerDownEventHitInfo);

                if (!isOnClickComponentBlocked && isSameEntityThatWasPressed && enablePointerEvent) { pointerInputUpEvent.Report(buttonId, ray, raycastInfoPointerEventLayer.hitInfo.hit); }

                pointerInputUpEvent = null;
            }

            ReportGlobalPointerEvent(InputEventType.UP, buttonId, useRaycast, raycastGlobalLayerHitInfo, raycastInfoGlobalLayer, currentSceneNumber);

            // Raycast for global pointer events (for each PE scene)
            foreach (string pexId in DataStore.i.Get<DataStore_World>().portableExperienceIds.Get())
            {
                IParcelScene pexScene = worldState.GetPortableExperienceScene(pexId);
                if (pexScene != null)
                {
                    raycastInfoGlobalLayer = raycastHandler.Raycast(ray, charCamera.farClipPlane, globalLayer, pexScene);
                    raycastGlobalLayerHitInfo = raycastInfoGlobalLayer.hitInfo;

                    ReportGlobalPointerEvent(InputEventType.UP, buttonId, useRaycast, raycastGlobalLayerHitInfo, raycastInfoGlobalLayer, pexScene.sceneData.sceneNumber);
                }
            }
        }

        private void ProcessButtonDown(WebInterface.ACTION_BUTTON buttonId, bool useRaycast, bool enablePointerEvent,
            LayerMask pointerEventLayer, int globalLayer)
        {
            IWorldState worldState = Environment.i.world.state;

            int currentSceneNumber = worldState.GetCurrentSceneNumber();

            if (currentSceneNumber <= 0)
                return;

            Ray ray = !Utils.IsCursorLocked || Utils.LockedThisFrame() ? GetRayFromMouse() : GetRayFromCamera();
            worldState.TryGetScene(currentSceneNumber, out var loadedScene);

            // Raycast for pointer event components
            RaycastResultInfo raycastInfoPointerEventLayer = raycastHandler.Raycast(ray, charCamera.farClipPlane, pointerEventLayer, loadedScene);

            // Raycast for global pointer events
            RaycastResultInfo raycastInfoGlobalLayer = raycastHandler.Raycast(ray, charCamera.farClipPlane, globalLayer, loadedScene);
            RaycastHitInfo raycastGlobalLayerHitInfo = raycastInfoGlobalLayer.hitInfo;

            bool isOnClickComponentBlocked =
                IsBlockingOnClick(raycastInfoPointerEventLayer.hitInfo, raycastGlobalLayerHitInfo);

            if (!isOnClickComponentBlocked && raycastInfoPointerEventLayer.hitInfo.hit.collider)
            {
                Collider collider = raycastInfoPointerEventLayer.hitInfo.hit.collider;

                GameObject hitGameObject;

                if (CollidersManager.i.GetColliderInfo(collider, out ColliderInfo info))
                    hitGameObject = info.entity.gameObject;
                else
                    hitGameObject = collider.gameObject;

                IList<IPointerInputEvent> events = GetPointerInputEvents(hitGameObject);

                for (var i = 0; i < events.Count; i++)
                {
                    IPointerInputEvent e = events[i];
                    bool areSameEntity = AreSameEntity(e, info);

                    switch (e.GetEventType())
                    {
                        case PointerInputEventType.CLICK:
                            if (areSameEntity && enablePointerEvent)
                                e.Report(buttonId, ray, raycastInfoPointerEventLayer.hitInfo.hit);

                            break;
                        case PointerInputEventType.DOWN:
                            if (areSameEntity && enablePointerEvent)
                                e.Report(buttonId, ray, raycastInfoPointerEventLayer.hitInfo.hit);

                            break;
                        case PointerInputEventType.UP:
                            if (areSameEntity && enablePointerEvent)
                                pointerInputUpEvent = e;
                            else
                                pointerInputUpEvent = null;

                            break;
                    }
                }

                lastPointerDownEventHitInfo = raycastInfoPointerEventLayer.hitInfo;
            }

            ReportGlobalPointerEvent(InputEventType.DOWN, buttonId, useRaycast, raycastGlobalLayerHitInfo, raycastInfoGlobalLayer, currentSceneNumber);

            // Raycast for global pointer events (for each PE scene)
            IEnumerable<string> currentPortableExperienceSceneIds = DataStore.i.world.portableExperienceIds.Get();

            foreach (var pexSceneId in currentPortableExperienceSceneIds)
            {
                IParcelScene pexSene = worldState.GetPortableExperienceScene(pexSceneId);

                if (pexSene != null)
                {
                    raycastInfoGlobalLayer = raycastHandler.Raycast(ray, charCamera.farClipPlane, globalLayer, pexSene);

                    raycastGlobalLayerHitInfo = raycastInfoGlobalLayer.hitInfo;

                    ReportGlobalPointerEvent(InputEventType.DOWN, buttonId, useRaycast, raycastGlobalLayerHitInfo, raycastInfoGlobalLayer, pexSene.sceneData.sceneNumber);
                }
            }
        }

        private static void ReportGlobalPointerEvent(InputEventType eventType, WebInterface.ACTION_BUTTON buttonId, bool useRaycast, RaycastHitInfo raycastGlobalLayerHitInfo,
            RaycastResultInfo raycastInfoGlobalLayer, int sceneNumber)
        {
            if (useRaycast && raycastGlobalLayerHitInfo.isValid)
            {
                CollidersManager.i.GetColliderInfo(raycastGlobalLayerHitInfo.hit.collider, out ColliderInfo colliderInfo);

                string entityId = colliderInfo.entity != null
                    ? Environment.i.world.sceneController.entityIdHelper.GetOriginalId(colliderInfo.entity.entityId)
                    : SpecialEntityIdLegacyLiteral.SCENE_ROOT_ENTITY;

                WebInterface.ReportGlobalPointerEvent(eventType ,buttonId, raycastInfoGlobalLayer.ray, raycastGlobalLayerHitInfo.hit.point, raycastGlobalLayerHitInfo.hit.normal,
                    raycastGlobalLayerHitInfo.hit.distance, sceneNumber, entityId, colliderInfo.meshName, isHitInfoValid: true);
            }
            else
                WebInterface.ReportGlobalPointerEvent(eventType, buttonId, raycastInfoGlobalLayer.ray, Vector3.zero, Vector3.zero, 0, sceneNumber);
        }

        private static bool AreSameEntity(IPointerEvent pointerInputEvent, ColliderInfo colliderInfo)
        {
            if (pointerInputEvent == null) return false;
            if (pointerInputEvent.entity == null && colliderInfo.entity == null) return true;
            return pointerInputEvent.entity == colliderInfo.entity;
        }

        private bool IsBlockingOnClick(RaycastHitInfo targetOnClickHit, RaycastHitInfo potentialBlockerHit) =>
            potentialBlockerHit.hit.collider != null // Does a potential blocker hit exist?
            && targetOnClickHit.hit.collider != null // Was a target entity with a pointer event component hit?
            && potentialBlockerHit.hit.distance <= targetOnClickHit.hit.distance // Is potential blocker nearer than target entity?
            && !AreCollidersFromSameEntity(potentialBlockerHit, targetOnClickHit); // Does potential blocker belong to other entity rather than target entity?

        private static bool EntityHasPointerEvent(IDCLEntity entity)
        {
            var componentsManager = entity.scene.componentsManagerLegacy;

            return componentsManager.HasComponent(entity, CLASS_ID_COMPONENT.UUID_CALLBACK) ||
                   componentsManager.HasComponent(entity, CLASS_ID_COMPONENT.UUID_ON_UP) ||
                   componentsManager.HasComponent(entity, CLASS_ID_COMPONENT.UUID_ON_DOWN) ||
                   componentsManager.HasComponent(entity, CLASS_ID_COMPONENT.UUID_ON_CLICK);
        }

        private bool AreCollidersFromSameEntity(RaycastHitInfo hitInfoA, RaycastHitInfo hitInfoB)
        {
            CollidersManager.i.GetColliderInfo(hitInfoA.hit.collider, out ColliderInfo colliderInfoA);
            CollidersManager.i.GetColliderInfo(hitInfoB.hit.collider, out ColliderInfo colliderInfoB);

            var entityA = colliderInfoA.entity;
            var entityB = colliderInfoB.entity;

            bool entityAHasEvent = entityA != null && EntityHasPointerEvent(entityA);
            bool entityBHasEvent = entityB != null && EntityHasPointerEvent(entityB);

            // If both entities has OnClick/PointerEvent component
            if (entityAHasEvent && entityBHasEvent)
                return entityA == entityB;

            // If only one of them has OnClick/PointerEvent component
            if (entityAHasEvent ^ entityBHasEvent)
                return false;

            // None of them has OnClick/PointerEvent component
            return colliderInfoA.entity == colliderInfoB.entity;
        }

        private void HandleCursorLockChanges(bool isLocked)
        {
            HideOrShowCursor(isLocked);

            if (!isLocked)
                UnhoverLastHoveredObject();
        }

        private static void HideOrShowCursor(bool isCursorLocked) =>
            DataStore.i.Get<DataStore_Cursor>().cursorVisible.Set(isCursorLocked);

        private static void SetHoverCursor() =>
            DataStore.i.Get<DataStore_Cursor>().cursorType.Set(DataStore_Cursor.CursorType.HOVER);

        private static void SetNormalCursor() =>
            DataStore.i.Get<DataStore_Cursor>().cursorType.Set(DataStore_Cursor.CursorType.NORMAL);

        private bool CanRaycastWhileUnlocked()
        {
            if (eventSystemInputModuleLazy == null)
                return true;

            return mouseCatcher.IsEqualsToRaycastTarget(
                eventSystemInputModuleLazy.GetPointerData().pointerCurrentRaycast.gameObject);
        }
    }
}
