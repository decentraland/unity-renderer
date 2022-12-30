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
using Ray = UnityEngine.Ray;

namespace DCL
{
    public class PointerEventsController
    {
        private PointerHoverController pointerHoverController;

        private static bool renderingEnabled => CommonScriptableObjects.rendererState.Get();

        RaycastHitInfo lastPointerDownEventHitInfo;
        IPointerInputEvent pointerInputUpEvent;
        IRaycastHandler raycastHandler = new RaycastHandler();

        Camera charCamera;

        RaycastHit hitInfo;
        PointerEventData uiGraphicRaycastPointerEventData = new PointerEventData(null);
        List<RaycastResult> uiGraphicRaycastResults = new List<RaycastResult>();
        GraphicRaycaster uiGraphicRaycaster;

        private IRaycastPointerClickHandler clickHandler;
        private readonly InputController_Legacy inputControllerLegacy;

        private DataStore_ECS7 dataStoreEcs7 = DataStore.i.ecs7;

        public PointerEventsController(InputController_Legacy inputControllerLegacy,
            InteractionHoverCanvasController hoverCanvas)
        {
            this.inputControllerLegacy = inputControllerLegacy;
            pointerHoverController = new PointerHoverController(inputControllerLegacy, hoverCanvas);

            pointerHoverController.OnPointerHoverStarts += SetHoverCursor;
            pointerHoverController.OnPointerHoverEnds += SetNormalCursor;

            for (int i = 0; i < Enum.GetValues(typeof(WebInterface.ACTION_BUTTON)).Length; i++)
            {
                var buttonId = (WebInterface.ACTION_BUTTON)i;

                if (buttonId == WebInterface.ACTION_BUTTON.ANY)
                    continue;

                inputControllerLegacy.AddListener(buttonId, OnButtonEvent);
            }

            RetrieveCamera();

            Environment.i.platform.updateEventHandler.AddListener(IUpdateEventHandler.EventType.Update, Update);
            Utils.OnCursorLockChanged += HandleCursorLockChanges;

            HideOrShowCursor(Utils.IsCursorLocked);
        }

        public void Update()
        {
            if (charCamera == null)
                RetrieveCamera();

            if (!CommonScriptableObjects.rendererState.Get() || charCamera == null)
                return;

            Type typeToUse = typeof(IPointerEvent);

            if (!Utils.IsCursorLocked)
            {
                //New interaction model
                if (!DataStore.i.featureFlags.flags.Get().IsFeatureEnabled("avatar_outliner"))
                    return;

                typeToUse = typeof(IAvatarOnPointerDown);
            }

            IWorldState worldState = Environment.i.world.state;

            // We use Physics.Raycast() instead of our raycastHandler.Raycast() as that one is slower, sometimes 2x, because it fetches info we don't need here
            Ray ray = Utils.IsCursorLocked ? GetRayFromCamera() : GetRayFromMouse();

            bool didHit = Physics.Raycast(ray, out hitInfo, Mathf.Infinity,
                PhysicsLayers.physicsCastLayerMaskWithoutCharacter);

            bool uiIsBlocking = false;
            int currentSceneNumber = worldState.GetCurrentSceneNumber();

            bool validCurrentScene = currentSceneNumber > 0 && worldState.ContainsScene(currentSceneNumber);

            // NOTE: in case of a single scene loaded (preview or builder) sceneId is set to null when stepping outside
            if (didHit && validCurrentScene)
            {
                DataStore_World worldData = DataStore.i.Get<DataStore_World>();
                GraphicRaycaster raycaster = worldData.currentRaycaster.Get();

                if (raycaster)
                {
                    uiGraphicRaycastPointerEventData.position = Utils.IsCursorLocked ? new Vector2(Screen.width / 2, Screen.height / 2) : Input.mousePosition;
                    uiGraphicRaycastResults.Clear();
                    raycaster.Raycast(uiGraphicRaycastPointerEventData, uiGraphicRaycastResults);
                    uiIsBlocking = uiGraphicRaycastResults.Count > 0;
                }
            }

            if (dataStoreEcs7.isEcs7Enabled)
            {
                dataStoreEcs7.lastPointerRayHit.hit.collider = hitInfo.collider;
                dataStoreEcs7.lastPointerRayHit.hit.point = hitInfo.point;
                dataStoreEcs7.lastPointerRayHit.hit.normal = hitInfo.normal;
                dataStoreEcs7.lastPointerRayHit.hit.distance = hitInfo.distance;
                dataStoreEcs7.lastPointerRayHit.didHit = didHit;
                dataStoreEcs7.lastPointerRayHit.ray = ray;
                dataStoreEcs7.lastPointerRayHit.hasValue = true;
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

            var targetGO = CollidersManager.i.GetColliderInfo(hitInfo.collider, out var colliderInfo)
                ? colliderInfo.entity.gameObject
                : hitInfo.collider.gameObject;

            clickHandler = null;
            pointerHoverController.OnRaycastHit(hitInfo, colliderInfo, targetGO, typeToUse);
        }

        private IList<IPointerInputEvent> GetPointerInputEvents(GameObject hitGameObject)
        {
            if (!Utils.IsCursorLocked || Utils.LockedThisFrame())
                return hitGameObject.GetComponentsInChildren<IAvatarOnPointerDown>();

            return hitGameObject.GetComponentsInChildren<IPointerInputEvent>();
        }

        private void ResolveGenericRaycastHandlers(IRaycastPointerHandler raycastHandlerTarget)
        {
            if (Utils.LockedThisFrame())
                return;

            var mouseIsDown = Input.GetMouseButtonDown(0);
            var mouseIsUp = Input.GetMouseButtonUp(0);

            if (raycastHandlerTarget is IRaycastPointerDownHandler down)
            {
                if (mouseIsDown)
                    down.OnPointerDown();
            }

            if (raycastHandlerTarget is IRaycastPointerUpHandler up)
            {
                if (mouseIsUp)
                    up.OnPointerUp();
            }

            if (raycastHandlerTarget is IRaycastPointerClickHandler click)
            {
                if (mouseIsDown)
                    clickHandler = click;

                if (mouseIsUp)
                {
                    if (clickHandler == click)
                        click.OnPointerClick();

                    clickHandler = null;
                }
            }
        }

        private void UnhoverLastHoveredObject()
        {
            pointerHoverController.ResetHoveredObject();
        }

        public void Dispose()
        {
            for (int i = 0; i < Enum.GetValues(typeof(WebInterface.ACTION_BUTTON)).Length; i++)
            {
                var buttonId = (WebInterface.ACTION_BUTTON)i;

                if (buttonId == WebInterface.ACTION_BUTTON.ANY)
                    continue;

                inputControllerLegacy.RemoveListener(buttonId, OnButtonEvent);
            }

            pointerHoverController.OnPointerHoverStarts -= SetHoverCursor;
            pointerHoverController.OnPointerHoverEnds -= SetNormalCursor;

            Environment.i.platform.updateEventHandler.RemoveListener(IUpdateEventHandler.EventType.Update, Update);
            Utils.OnCursorLockChanged -= HandleCursorLockChanges;
        }

        void RetrieveCamera()
        {
            if (charCamera == null) { charCamera = Camera.main; }
        }

        public Ray GetRayFromCamera()
        {
            return charCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        }

        public Ray GetRayFromMouse()
        {
            return charCamera.ScreenPointToRay(Input.mousePosition);
        }

        void OnButtonEvent(WebInterface.ACTION_BUTTON buttonId, InputController_Legacy.EVENT evt, bool useRaycast,
            bool enablePointerEvent)
        {
            //TODO(Brian): We should remove this when we get a proper initialization layer
            if (!EnvironmentSettings.RUNNING_TESTS)
            {
                if (!renderingEnabled)
                    return;

                if (Utils.LockedThisFrame() || !Utils.IsCursorLocked)
                {
                    //New interaction model
                    if (!DataStore.i.featureFlags.flags.Get().IsFeatureEnabled("avatar_outliner"))
                        return;
                }
            }

            if (charCamera == null)
            {
                RetrieveCamera();

                if (charCamera == null)
                    return;
            }

            var pointerEventLayer =
                PhysicsLayers.physicsCastLayerMaskWithoutCharacter; //Ensure characterController is being filtered

            var globalLayer = pointerEventLayer & ~PhysicsLayers.physicsCastLayerMask;

            if (evt == InputController_Legacy.EVENT.BUTTON_DOWN) { ProcessButtonDown(buttonId, useRaycast, enablePointerEvent, pointerEventLayer, globalLayer); }
            else if (evt == InputController_Legacy.EVENT.BUTTON_UP) { ProcessButtonUp(buttonId, useRaycast, enablePointerEvent, pointerEventLayer, globalLayer); }

            if (dataStoreEcs7.isEcs7Enabled)
            {
                dataStoreEcs7.lastPointerInputEvent.buttonId = (int)buttonId;
                dataStoreEcs7.lastPointerInputEvent.isButtonDown = evt == InputController_Legacy.EVENT.BUTTON_DOWN;
                dataStoreEcs7.lastPointerInputEvent.hasValue = evt == InputController_Legacy.EVENT.BUTTON_DOWN || evt == InputController_Legacy.EVENT.BUTTON_UP;
            }
        }

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
                raycastInfoPointerEventLayer = raycastHandler.Raycast(ray, charCamera.farClipPlane,
                    pointerEventLayer, loadedScene);
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

            ReportGlobalPointerUpEvent(buttonId, useRaycast, raycastGlobalLayerHitInfo, raycastInfoGlobalLayer, currentSceneNumber);

            // Raycast for global pointer events (for each PE scene)
            List<string> currentPortableExperienceIds = DataStore.i.Get<DataStore_World>().portableExperienceIds.Get().ToList();

            for (int i = 0; i < currentPortableExperienceIds.Count; i++)
            {
                IParcelScene pexSene = worldState.GetPortableExperienceScene(currentPortableExperienceIds[i]);

                if (pexSene != null)
                {
                    raycastInfoGlobalLayer = raycastHandler.Raycast(ray, charCamera.farClipPlane, globalLayer, pexSene);
                    raycastGlobalLayerHitInfo = raycastInfoGlobalLayer.hitInfo;

                    ReportGlobalPointerUpEvent(buttonId, useRaycast, raycastGlobalLayerHitInfo, raycastInfoGlobalLayer,
                        pexSene.sceneData.sceneNumber);
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

            RaycastHitInfo raycastGlobalLayerHitInfo;
            Ray ray = !Utils.IsCursorLocked || Utils.LockedThisFrame() ? GetRayFromMouse() : GetRayFromCamera();
            worldState.TryGetScene(currentSceneNumber, out var loadedScene);

            // Raycast for pointer event components
            RaycastResultInfo raycastInfoPointerEventLayer = raycastHandler.Raycast(ray, charCamera.farClipPlane, pointerEventLayer, loadedScene);

            // Raycast for global pointer events
            RaycastResultInfo raycastInfoGlobalLayer = raycastHandler.Raycast(ray, charCamera.farClipPlane, globalLayer, loadedScene);

            raycastGlobalLayerHitInfo = raycastInfoGlobalLayer.hitInfo;

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

            ReportGlobalPointerDownEvent(buttonId, useRaycast, raycastGlobalLayerHitInfo, raycastInfoGlobalLayer, currentSceneNumber);

            // Raycast for global pointer events (for each PE scene)
            IEnumerable<string> currentPortableExperienceSceneIds = DataStore.i.world.portableExperienceIds.Get();

            foreach (var pexSceneId in currentPortableExperienceSceneIds)
            {
                IParcelScene pexSene = worldState.GetPortableExperienceScene(pexSceneId);

                if (pexSene != null)
                {
                    raycastInfoGlobalLayer = raycastHandler.Raycast(ray, charCamera.farClipPlane, globalLayer, pexSene);

                    raycastGlobalLayerHitInfo = raycastInfoGlobalLayer.hitInfo;

                    ReportGlobalPointerDownEvent(buttonId, useRaycast, raycastGlobalLayerHitInfo, raycastInfoGlobalLayer, pexSene.sceneData.sceneNumber);
                }
            }
        }

        private void ReportGlobalPointerUpEvent(
            WebInterface.ACTION_BUTTON buttonId,
            bool useRaycast,
            RaycastHitInfo raycastGlobalLayerHitInfo,
            RaycastResultInfo raycastInfoGlobalLayer,
            int sceneNumber)
        {
            if (useRaycast && raycastGlobalLayerHitInfo.isValid)
            {
                CollidersManager.i.GetColliderInfo(raycastGlobalLayerHitInfo.hit.collider,
                    out ColliderInfo colliderInfo);

                string entityId = SpecialEntityIdLegacyLiteral.SCENE_ROOT_ENTITY;

                if (colliderInfo.entity != null)
                    entityId =
                        Environment.i.world.sceneController.entityIdHelper.GetOriginalId(colliderInfo.entity.entityId);

                WebInterface.ReportGlobalPointerUpEvent(
                    buttonId,
                    raycastInfoGlobalLayer.ray,
                    raycastGlobalLayerHitInfo.hit.point,
                    raycastGlobalLayerHitInfo.hit.normal,
                    raycastGlobalLayerHitInfo.hit.distance,
                    sceneNumber,
                    entityId,
                    colliderInfo.meshName,
                    isHitInfoValid: true);
            }
            else
            {
                WebInterface.ReportGlobalPointerUpEvent(buttonId, raycastInfoGlobalLayer.ray, Vector3.zero,
                    Vector3.zero, 0, sceneNumber);
            }
        }

        private void ReportGlobalPointerDownEvent(
            WebInterface.ACTION_BUTTON buttonId,
            bool useRaycast,
            RaycastHitInfo raycastGlobalLayerHitInfo,
            RaycastResultInfo raycastInfoGlobalLayer,
            int sceneNumber)
        {
            if (useRaycast && raycastGlobalLayerHitInfo.isValid)
            {
                CollidersManager.i.GetColliderInfo(raycastGlobalLayerHitInfo.hit.collider,
                    out ColliderInfo colliderInfo);

                string entityId = SpecialEntityIdLegacyLiteral.SCENE_ROOT_ENTITY;

                if (colliderInfo.entity != null)
                    entityId =
                        Environment.i.world.sceneController.entityIdHelper.GetOriginalId(colliderInfo.entity.entityId);

                WebInterface.ReportGlobalPointerDownEvent(
                    buttonId,
                    raycastInfoGlobalLayer.ray,
                    raycastGlobalLayerHitInfo.hit.point,
                    raycastGlobalLayerHitInfo.hit.normal,
                    raycastGlobalLayerHitInfo.hit.distance,
                    sceneNumber,
                    entityId,
                    colliderInfo.meshName,
                    isHitInfoValid: true);
            }
            else
            {
                WebInterface.ReportGlobalPointerDownEvent(buttonId, raycastInfoGlobalLayer.ray, Vector3.zero,
                    Vector3.zero, 0, sceneNumber);
            }
        }

        bool AreSameEntity(IPointerEvent pointerInputEvent, ColliderInfo colliderInfo)
        {
            return pointerInputEvent != null && colliderInfo.entity != null &&
                   pointerInputEvent.entity == colliderInfo.entity;
        }

        bool IsBlockingOnClick(RaycastHitInfo targetOnClickHit, RaycastHitInfo potentialBlockerHit)
        {
            return
                potentialBlockerHit.hit.collider != null // Does a potential blocker hit exist?
                && targetOnClickHit.hit.collider != null // Was a target entity with a pointer event component hit?
                && potentialBlockerHit.hit.distance <=
                targetOnClickHit.hit.distance // Is potential blocker nearer than target entity?
                && !AreCollidersFromSameEntity(potentialBlockerHit,
                    targetOnClickHit); // Does potential blocker belong to other entity rather than target entity?
        }

        bool EntityHasPointerEvent(IDCLEntity entity)
        {
            var componentsManager = entity.scene.componentsManagerLegacy;

            return componentsManager.HasComponent(entity, Models.CLASS_ID_COMPONENT.UUID_CALLBACK) ||
                   componentsManager.HasComponent(entity, Models.CLASS_ID_COMPONENT.UUID_ON_UP) ||
                   componentsManager.HasComponent(entity, Models.CLASS_ID_COMPONENT.UUID_ON_DOWN) ||
                   componentsManager.HasComponent(entity, Models.CLASS_ID_COMPONENT.UUID_ON_CLICK);
        }

        bool AreCollidersFromSameEntity(RaycastHitInfo hitInfoA, RaycastHitInfo hitInfoB)
        {
            CollidersManager.i.GetColliderInfo(hitInfoA.hit.collider, out ColliderInfo colliderInfoA);
            CollidersManager.i.GetColliderInfo(hitInfoB.hit.collider, out ColliderInfo colliderInfoB);

            var entityA = colliderInfoA.entity;
            var entityB = colliderInfoB.entity;

            bool entityAHasEvent = entityA != null && EntityHasPointerEvent(entityA);
            bool entityBHasEvent = entityB != null && EntityHasPointerEvent(entityB);

            // If both entities has OnClick/PointerEvent component
            if (entityAHasEvent && entityBHasEvent) { return entityA == entityB; }

            // If only one of them has OnClick/PointerEvent component
            else if (entityAHasEvent ^ entityBHasEvent) { return false; }

            // None of them has OnClick/PointerEvent component
            else { return colliderInfoA.entity == colliderInfoB.entity; }
        }

        private void HandleCursorLockChanges(bool isLocked)
        {
            HideOrShowCursor(isLocked);

            if (!isLocked)
                UnhoverLastHoveredObject();
        }

        private void HideOrShowCursor(bool isCursorLocked)
        {
            DataStore.i.Get<DataStore_Cursor>().cursorVisible.Set(isCursorLocked);
        }

        private void SetHoverCursor()
        {
            DataStore.i.Get<DataStore_Cursor>().cursorType.Set(DataStore_Cursor.CursorType.HOVER);
        }

        private void SetNormalCursor()
        {
            DataStore.i.Get<DataStore_Cursor>().cursorType.Set(DataStore_Cursor.CursorType.NORMAL);
        }
    }
}
