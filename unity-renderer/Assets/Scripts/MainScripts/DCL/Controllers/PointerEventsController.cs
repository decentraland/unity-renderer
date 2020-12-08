using DCL.Components;
using DCL.Configuration;
using DCL.Helpers;
using DCL.Interface;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace DCL
{
    public class PointerEventsController
    {
        private static bool renderingEnabled => CommonScriptableObjects.rendererState.Get();
        public System.Action OnPointerHoverStarts;
        public System.Action OnPointerHoverEnds;

        InteractionHoverCanvasController hoverController;
        RaycastHitInfo lastPointerDownEventHitInfo;
        OnPointerUp pointerUpEvent;
        IRaycastHandler raycastHandler = new RaycastHandler();
        Camera charCamera;
        GameObject lastHoveredObject = null;
        GameObject newHoveredObject = null;
        OnPointerEvent newHoveredEvent = null;
        OnPointerEvent[] lastHoveredEventList = null;
        RaycastHit hitInfo;
        PointerEventData uiGraphicRaycastPointerEventData = new PointerEventData(null);
        List<RaycastResult> uiGraphicRaycastResults = new List<RaycastResult>();
        GraphicRaycaster uiGraphicRaycaster;

        public void Initialize()
        {
            InputController_Legacy.i.AddListener(WebInterface.ACTION_BUTTON.POINTER, OnButtonEvent);
            InputController_Legacy.i.AddListener(WebInterface.ACTION_BUTTON.PRIMARY, OnButtonEvent);
            InputController_Legacy.i.AddListener(WebInterface.ACTION_BUTTON.SECONDARY, OnButtonEvent);

            hoverController = Environment.i.interactionHoverCanvasController;

            RetrieveCamera();
        }

        private IRaycastPointerClickHandler clickHandler;

        public void Update()
        {
            if (!CommonScriptableObjects.rendererState.Get() || charCamera == null) return;

            WorldState worldState = Environment.i.worldState;

            // We use Physics.Raycast() instead of our raycastHandler.Raycast() as that one is slower, sometimes 2x, because it fetches info we don't need here
            bool didHit = Physics.Raycast(GetRayFromCamera(), out hitInfo, Mathf.Infinity, PhysicsLayers.physicsCastLayerMaskWithoutCharacter);
            bool uiIsBlocking = false;
            string currentSceneId = worldState.currentSceneId;

            // NOTE: in case of a single scene loaded (preview or builder) sceneId is set to null when stepping outside
            if (!string.IsNullOrEmpty(currentSceneId) && didHit && worldState.loadedScenes.ContainsKey(currentSceneId))
            {
                GraphicRaycaster raycaster = worldState.loadedScenes[currentSceneId].uiScreenSpace?.graphicRaycaster;
                if (raycaster)
                {
                    uiGraphicRaycastPointerEventData.position = new Vector2(Screen.width / 2, Screen.height / 2);
                    uiGraphicRaycastResults.Clear();
                    raycaster.Raycast(uiGraphicRaycastPointerEventData, uiGraphicRaycastResults);

                    uiIsBlocking = uiGraphicRaycastResults.Count > 0;
                }
            }

            if (!didHit || uiIsBlocking)
            {
                clickHandler = null;
                UnhoverLastHoveredObject(hoverController);
                return;
            }

            var raycastHandlerTarget = hitInfo.collider.GetComponent<IRaycastPointerHandler>();
            if (raycastHandlerTarget != null)
            {
                ResolveGenericRaycastHandlers(raycastHandlerTarget);
                UnhoverLastHoveredObject(hoverController);
                return;
            }

            if (CollidersManager.i.GetColliderInfo(hitInfo.collider, out ColliderInfo info))
                newHoveredEvent = info.entity.gameObject.GetComponentInChildren<OnPointerEvent>();
            else
                newHoveredEvent = hitInfo.collider.GetComponentInChildren<OnPointerEvent>();

            clickHandler = null;

            if (!EventObjectCanBeHovered(newHoveredEvent, info, hitInfo.distance))
            {
                UnhoverLastHoveredObject(hoverController);
                return;
            }

            newHoveredObject = newHoveredEvent.gameObject;

            if (newHoveredObject != lastHoveredObject)
            {
                UnhoverLastHoveredObject(hoverController);

                lastHoveredObject = newHoveredObject;
                lastHoveredEventList = newHoveredObject.GetComponents<OnPointerEvent>();
                OnPointerHoverStarts?.Invoke();
            }

            // OnPointerDown/OnClick and OnPointerUp should display their hover feedback at different moments
            if (lastHoveredEventList != null && lastHoveredEventList.Length > 0)
            {
                for (int i = 0; i < lastHoveredEventList.Length; i++)
                {
                    OnPointerEvent e = lastHoveredEventList[i];

                    bool eventButtonIsPressed = InputController_Legacy.i.IsPressed(e.GetActionButton());

                    if (e is OnPointerUp && eventButtonIsPressed)
                        e.SetHoverState(true);
                    else if ((e is OnPointerDown || e is OnClick) && !eventButtonIsPressed)
                        e.SetHoverState(true);
                    else
                        e.SetHoverState(false);
                }
            }

            newHoveredObject = null;
            newHoveredEvent = null;
        }

        private bool EventObjectCanBeHovered(OnPointerEvent targetEvent, ColliderInfo colliderInfo, float distance)
        {
            return newHoveredEvent != null && newHoveredEvent.IsAtHoverDistance(distance) && (IsAvatarPointerEvent(newHoveredEvent) || (newHoveredEvent.IsVisible() && AreSameEntity(newHoveredEvent, colliderInfo)));
        }

        private void ResolveGenericRaycastHandlers(IRaycastPointerHandler raycastHandlerTarget)
        {
            if (Utils.LockedThisFrame()) return;

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

        void UnhoverLastHoveredObject(InteractionHoverCanvasController interactionHoverCanvasController)
        {
            if (lastHoveredObject == null)
            {
                interactionHoverCanvasController.SetHoverState(false);
                return;
            }

            OnPointerHoverEnds?.Invoke();

            for (int i = 0; i < lastHoveredEventList.Length; i++)
            {
                if (lastHoveredEventList[i] == null) continue;
                lastHoveredEventList[i].SetHoverState(false);
            }

            lastHoveredEventList = null;
            lastHoveredObject = null;
        }

        public void Cleanup()
        {
            InputController_Legacy.i.RemoveListener(WebInterface.ACTION_BUTTON.POINTER, OnButtonEvent);
            InputController_Legacy.i.RemoveListener(WebInterface.ACTION_BUTTON.PRIMARY, OnButtonEvent);
            InputController_Legacy.i.RemoveListener(WebInterface.ACTION_BUTTON.SECONDARY, OnButtonEvent);

            lastHoveredObject = null;
            newHoveredObject = null;
            newHoveredEvent = null;
            lastHoveredEventList = null;
        }

        void RetrieveCamera()
        {
            if (charCamera == null)
            {
                charCamera = Camera.main;
            }
        }

        public Ray GetRayFromCamera()
        {
            return charCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        }

        void OnButtonEvent(WebInterface.ACTION_BUTTON buttonId, InputController_Legacy.EVENT evt, bool useRaycast)
        {
            //TODO(Brian): We should remove this when we get a proper initialization layer
            if (!EnvironmentSettings.RUNNING_TESTS)
            {
                if (Utils.LockedThisFrame())
                    return;

                if (!Utils.isCursorLocked || !renderingEnabled)
                    return;
            }

            if (charCamera == null)
            {
                RetrieveCamera();

                if (charCamera == null)
                    return;
            }

            var pointerEventLayer = PhysicsLayers.physicsCastLayerMaskWithoutCharacter; //Ensure characterController is being filtered
            var globalLayer = pointerEventLayer & ~PhysicsLayers.physicsCastLayerMask;

            if (evt == InputController_Legacy.EVENT.BUTTON_DOWN)
            {
                ProcessButtonDown(buttonId, useRaycast, pointerEventLayer, globalLayer);
            }
            else if (evt == InputController_Legacy.EVENT.BUTTON_UP)
            {
                ProcessButtonUp(buttonId, useRaycast, pointerEventLayer, globalLayer);
            }
        }

        private void ProcessButtonUp(WebInterface.ACTION_BUTTON buttonId, bool useRaycast, LayerMask pointerEventLayer, int globalLayer)
        {
            RaycastHitInfo raycastGlobalLayerHitInfo;
            Ray ray = GetRayFromCamera();

            // Raycast for global pointer events
            RaycastResultInfo raycastInfoGlobalLayer = raycastHandler.Raycast(ray, charCamera.farClipPlane, globalLayer, null);
            raycastGlobalLayerHitInfo = raycastInfoGlobalLayer.hitInfo;

            if (pointerUpEvent != null)
            {
                // Raycast for pointer event components
                RaycastResultInfo raycastInfoPointerEventLayer = raycastHandler.Raycast(ray, charCamera.farClipPlane, pointerEventLayer, null);

                bool isOnClickComponentBlocked = IsBlockingOnClick(raycastInfoPointerEventLayer.hitInfo, raycastGlobalLayerHitInfo);
                bool isSameEntityThatWasPressed = AreCollidersFromSameEntity(raycastInfoPointerEventLayer.hitInfo, lastPointerDownEventHitInfo);

                if (!isOnClickComponentBlocked && isSameEntityThatWasPressed)
                {
                    bool isHitInfoValid = raycastInfoPointerEventLayer.hitInfo.hit.collider != null;
                    pointerUpEvent.Report(buttonId, ray, raycastInfoPointerEventLayer.hitInfo.hit, isHitInfoValid);
                }

                pointerUpEvent = null;
            }

            string sceneId = Environment.i.worldState.currentSceneId;

            if (useRaycast && raycastGlobalLayerHitInfo.isValid)
            {
                CollidersManager.i.GetColliderInfo(raycastGlobalLayerHitInfo.hit.collider, out ColliderInfo colliderInfo);

                WebInterface.ReportGlobalPointerUpEvent(
                    buttonId,
                    raycastInfoGlobalLayer.ray,
                    raycastGlobalLayerHitInfo.hit.point,
                    raycastGlobalLayerHitInfo.hit.normal,
                    raycastGlobalLayerHitInfo.hit.distance,
                    sceneId,
                    colliderInfo.entity != null ? colliderInfo.entity.entityId : null,
                    colliderInfo.meshName,
                    isHitInfoValid: true);
            }
            else
            {
                WebInterface.ReportGlobalPointerUpEvent(buttonId, raycastInfoGlobalLayer.ray, Vector3.zero, Vector3.zero, 0, sceneId);
            }
        }

        private void ProcessButtonDown(WebInterface.ACTION_BUTTON buttonId, bool useRaycast, LayerMask pointerEventLayer, int globalLayer)
        {
            RaycastHitInfo raycastGlobalLayerHitInfo;
            Ray ray = GetRayFromCamera();

            // Raycast for pointer event components
            RaycastResultInfo raycastInfoPointerEventLayer = raycastHandler.Raycast(ray, charCamera.farClipPlane, pointerEventLayer, null);

            // Raycast for global pointer events
            RaycastResultInfo raycastInfoGlobalLayer = raycastHandler.Raycast(ray, charCamera.farClipPlane, globalLayer, null);
            raycastGlobalLayerHitInfo = raycastInfoGlobalLayer.hitInfo;

            bool isOnClickComponentBlocked = IsBlockingOnClick(raycastInfoPointerEventLayer.hitInfo, raycastGlobalLayerHitInfo);

            if (!isOnClickComponentBlocked && raycastInfoPointerEventLayer.hitInfo.hit.collider)
            {
                Collider collider = raycastInfoPointerEventLayer.hitInfo.hit.collider;

                GameObject hitGameObject;
                if (CollidersManager.i.GetColliderInfo(collider, out ColliderInfo info))
                    hitGameObject = info.entity.gameObject;
                else
                    hitGameObject = collider.gameObject;

                OnClick onClick = hitGameObject.GetComponentInChildren<OnClick>();
                if (AreSameEntity(onClick, info))
                    onClick.Report(buttonId, raycastInfoPointerEventLayer.hitInfo.hit);

                OnPointerDown onPointerDown = hitGameObject.GetComponentInChildren<OnPointerDown>();
                if (IsAvatarPointerEvent(onPointerDown) || AreSameEntity(onPointerDown, info))
                    onPointerDown.Report(buttonId, ray, raycastInfoPointerEventLayer.hitInfo.hit);

                pointerUpEvent = hitGameObject.GetComponentInChildren<OnPointerUp>();
                if (!AreSameEntity(pointerUpEvent, info))
                    pointerUpEvent = null;

                lastPointerDownEventHitInfo = raycastInfoPointerEventLayer.hitInfo;
            }

            string sceneId = Environment.i.worldState.currentSceneId;

            if (useRaycast && raycastGlobalLayerHitInfo.isValid)
            {
                CollidersManager.i.GetColliderInfo(raycastGlobalLayerHitInfo.hit.collider, out ColliderInfo colliderInfo);

                WebInterface.ReportGlobalPointerDownEvent(
                    buttonId,
                    raycastInfoGlobalLayer.ray,
                    raycastGlobalLayerHitInfo.hit.point,
                    raycastGlobalLayerHitInfo.hit.normal,
                    raycastGlobalLayerHitInfo.hit.distance,
                    sceneId,
                    colliderInfo.entity != null ? colliderInfo.entity.entityId : null,
                    colliderInfo.meshName,
                    isHitInfoValid: true);
            }
            else
            {
                WebInterface.ReportGlobalPointerDownEvent(buttonId, raycastInfoGlobalLayer.ray, Vector3.zero, Vector3.zero, 0, sceneId);
            }
        }

        bool IsAvatarPointerEvent(OnPointerEvent targetPointerEvent)
        {
            return targetPointerEvent != null && targetPointerEvent is AvatarOnPointerDown;
        }

        bool AreSameEntity(OnPointerEvent pointerEvent, ColliderInfo colliderInfo)
        {
            return pointerEvent != null && colliderInfo.entity != null && pointerEvent.entity == colliderInfo.entity;
        }

        bool IsBlockingOnClick(RaycastHitInfo targetOnClickHit, RaycastHitInfo potentialBlockerHit)
        {
            return
                potentialBlockerHit.hit.collider != null // Does a potential blocker hit exist?
                && targetOnClickHit.hit.collider != null // Was a target entity with a pointer event component hit?
                && potentialBlockerHit.hit.distance <= targetOnClickHit.hit.distance // Is potential blocker nearer than target entity?
                && !AreCollidersFromSameEntity(potentialBlockerHit, targetOnClickHit); // Does potential blocker belong to other entity rather than target entity?
        }

        bool AreCollidersFromSameEntity(RaycastHitInfo hitInfoA, RaycastHitInfo hitInfoB)
        {
            CollidersManager.i.GetColliderInfo(hitInfoA.hit.collider, out ColliderInfo colliderInfoA);
            CollidersManager.i.GetColliderInfo(hitInfoB.hit.collider, out ColliderInfo colliderInfoB);

            var entityA = colliderInfoA.entity;
            var entityB = colliderInfoB.entity;

            bool entityAHasEvent = entityA != null && entityA.components.ContainsKey(Models.CLASS_ID_COMPONENT.UUID_CALLBACK);
            bool entityBHasEvent = entityB != null && entityB.components.ContainsKey(Models.CLASS_ID_COMPONENT.UUID_CALLBACK);

            // If both entities has OnClick/PointerEvent component
            if (entityAHasEvent && entityBHasEvent)
            {
                return entityA == entityB;
            }
            // If only one of them has OnClick/PointerEvent component
            else if (entityAHasEvent ^ entityBHasEvent)
            {
                return false;
            }
            // None of them has OnClick/PointerEvent component
            else
            {
                return colliderInfoA.entity == colliderInfoB.entity;
            }
        }
    }
}