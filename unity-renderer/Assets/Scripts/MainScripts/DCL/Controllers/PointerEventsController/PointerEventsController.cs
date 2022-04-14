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
using DCL.Models;
using Ray = UnityEngine.Ray;

namespace DCL
{
    public class PointerEventsController : IPointerEventsController
    {
        private static bool renderingEnabled => CommonScriptableObjects.rendererState.Get();
        public System.Action OnPointerHoverStarts;
        public System.Action OnPointerHoverEnds;

        InteractionHoverCanvasController hoverController;
        RaycastHitInfo lastPointerDownEventHitInfo;
        IPointerInputEvent pointerInputUpEvent;
        IRaycastHandler raycastHandler = new RaycastHandler();

        Camera charCamera;

        GameObject lastHoveredObject = null;
        GameObject newHoveredGO = null;

        IPointerEvent newHoveredInputEvent = null;
        IPointerEvent[] lastHoveredEventList = null;

        RaycastHit hitInfo;
        PointerEventData uiGraphicRaycastPointerEventData = new PointerEventData(null);
        List<RaycastResult> uiGraphicRaycastResults = new List<RaycastResult>();
        GraphicRaycaster uiGraphicRaycaster;

        public void Initialize()
        {
            for (int i = 0; i < Enum.GetValues(typeof(WebInterface.ACTION_BUTTON)).Length; i++)
            {
                var buttonId = (WebInterface.ACTION_BUTTON)i;

                if (buttonId == WebInterface.ACTION_BUTTON.ANY)
                    continue;

                InputController_Legacy.i.AddListener(buttonId, OnButtonEvent);
            }

            hoverController = InteractionHoverCanvasController.i;

            if (CursorController.i != null)
            {
                OnPointerHoverStarts += CursorController.i.SetHoverCursor;
                OnPointerHoverEnds += CursorController.i.SetNormalCursor;
            }

            RetrieveCamera();

            Environment.i.platform.updateEventHandler.AddListener(IUpdateEventHandler.EventType.Update, Update);
            Utils.OnCursorLockChanged += HandleCursorLockChanges;
            
            HideOrShowCursor(Utils.IsCursorLocked);
        }

        private IRaycastPointerClickHandler clickHandler;

        public void Update()
        {
            if ( charCamera == null )
                RetrieveCamera();

            if (!CommonScriptableObjects.rendererState.Get() || charCamera == null)
                return;
            if (!Utils.IsCursorLocked) return;

            IWorldState worldState = Environment.i.world.state;

            // We use Physics.Raycast() instead of our raycastHandler.Raycast() as that one is slower, sometimes 2x, because it fetches info we don't need here
            bool didHit = Physics.Raycast(GetRayFromCamera(), out hitInfo, Mathf.Infinity, PhysicsLayers.physicsCastLayerMaskWithoutCharacter);
            bool uiIsBlocking = false;
            string currentSceneId = worldState.currentSceneId;

            bool validCurrentSceneId = !string.IsNullOrEmpty(currentSceneId);
            bool validCurrentScene = validCurrentSceneId && worldState.loadedScenes.ContainsKey(currentSceneId);

            // NOTE: in case of a single scene loaded (preview or builder) sceneId is set to null when stepping outside
            if (didHit && validCurrentSceneId && validCurrentScene)
            {
                UIScreenSpace currentUIScreenSpace = worldState.loadedScenes[currentSceneId].GetSharedComponent<UIScreenSpace>();
                GraphicRaycaster raycaster = currentUIScreenSpace?.graphicRaycaster;

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

            if (CollidersManager.i.GetColliderInfo(hitInfo.collider, out ColliderInfo info))
                newHoveredInputEvent = info.entity.gameObject.GetComponentInChildren<IPointerEvent>();
            else
                newHoveredInputEvent = hitInfo.collider.GetComponentInChildren<IPointerEvent>();

            clickHandler = null;

            if (!EventObjectCanBeHovered(info, hitInfo.distance))
            {
                UnhoverLastHoveredObject();
                return;
            }

            newHoveredGO = newHoveredInputEvent.GetTransform().gameObject;

            if (newHoveredGO != lastHoveredObject)
            {
                UnhoverLastHoveredObject();

                lastHoveredObject = newHoveredGO;

                lastHoveredEventList = newHoveredInputEvent.entity.gameObject.transform.Cast<Transform>()
                                                           .Select(child => child.GetComponent<IPointerEvent>())
                                                           .Where(pointerComponent => pointerComponent != null)
                                                           .ToArray();

                // NOTE: this case is for the Avatar, since it hierarchy differs from other ECS components
                if (lastHoveredEventList?.Length == 0)
                {
                    lastHoveredEventList = newHoveredGO.GetComponents<IPointerEvent>();
                }
                
                OnPointerHoverStarts?.Invoke();
            }

            // OnPointerDown/OnClick and OnPointerUp should display their hover feedback at different moments
            if (lastHoveredEventList != null && lastHoveredEventList.Length > 0)
            {
                bool isEntityShowingHoverFeedback = false;
                
                for (int i = 0; i < lastHoveredEventList.Length; i++)
                {
                    if (lastHoveredEventList[i] is IPointerInputEvent e)
                    {
                        bool eventButtonIsPressed = InputController_Legacy.i.IsPressed(e.GetActionButton());

                        bool isClick = e.GetEventType() == PointerInputEventType.CLICK;
                        bool isDown = e.GetEventType() == PointerInputEventType.DOWN;
                        bool isUp = e.GetEventType() == PointerInputEventType.UP;

                        if (isUp && eventButtonIsPressed)
                        {
                            e.SetHoverState(true);
                            isEntityShowingHoverFeedback = isEntityShowingHoverFeedback || e.ShouldShowHoverFeedback();
                        }
                        else if ((isDown || isClick) && !eventButtonIsPressed)
                        {
                            e.SetHoverState(true);
                            isEntityShowingHoverFeedback = isEntityShowingHoverFeedback || e.ShouldShowHoverFeedback();
                        }
                        else if (!isEntityShowingHoverFeedback)
                        {
                            e.SetHoverState(false);
                        }
                    }
                    else
                    {
                        lastHoveredEventList[i].SetHoverState(true);
                    }
                }
            }

            newHoveredGO = null;
            newHoveredInputEvent = null;
        }

        private bool EventObjectCanBeHovered(ColliderInfo colliderInfo, float distance)
        {
            return newHoveredInputEvent != null &&
                   newHoveredInputEvent.IsAtHoverDistance(distance) &&
                   newHoveredInputEvent.IsVisible() &&
                   AreSameEntity(newHoveredInputEvent, colliderInfo);
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

        void UnhoverLastHoveredObject()
        {
            if (lastHoveredObject == null)
            {
                if ( hoverController != null )
                    hoverController.SetHoverState(false);

                return;
            }

            OnPointerHoverEnds?.Invoke();

            for (int i = 0; i < lastHoveredEventList.Length; i++)
            {
                if (lastHoveredEventList[i] == null)
                    continue;
                lastHoveredEventList[i].SetHoverState(false);
            }

            lastHoveredEventList = null;
            lastHoveredObject = null;
        }

        public void Dispose()
        {
            for (int i = 0; i < Enum.GetValues(typeof(WebInterface.ACTION_BUTTON)).Length; i++)
            {
                var buttonId = (WebInterface.ACTION_BUTTON)i;

                if (buttonId == WebInterface.ACTION_BUTTON.ANY)
                    continue;

                InputController_Legacy.i.RemoveListener(buttonId, OnButtonEvent);
            }

            lastHoveredObject = null;
            newHoveredGO = null;
            newHoveredInputEvent = null;
            lastHoveredEventList = null;

            if (CursorController.i != null)
            {
                OnPointerHoverStarts -= CursorController.i.SetHoverCursor;
                OnPointerHoverEnds -= CursorController.i.SetNormalCursor;
            }

            Environment.i.platform.updateEventHandler.RemoveListener(IUpdateEventHandler.EventType.Update, Update);
            Utils.OnCursorLockChanged -= HandleCursorLockChanges;
        }

        void RetrieveCamera()
        {
            if (charCamera == null)
            {
                charCamera = Camera.main;
            }
        }

        public Ray GetRayFromCamera() { return charCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0)); }

        void OnButtonEvent(WebInterface.ACTION_BUTTON buttonId, InputController_Legacy.EVENT evt, bool useRaycast, bool enablePointerEvent)
        {
            //TODO(Brian): We should remove this when we get a proper initialization layer
            if (!EnvironmentSettings.RUNNING_TESTS)
            {
                if (Utils.LockedThisFrame())
                    return;

                if (!Utils.IsCursorLocked || !renderingEnabled)
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
                ProcessButtonDown(buttonId, useRaycast, enablePointerEvent, pointerEventLayer, globalLayer);
            }
            else if (evt == InputController_Legacy.EVENT.BUTTON_UP)
            {
                ProcessButtonUp(buttonId, useRaycast, enablePointerEvent, pointerEventLayer, globalLayer);
            }
        }

        private void ProcessButtonUp(WebInterface.ACTION_BUTTON buttonId, bool useRaycast, bool enablePointerEvent, LayerMask pointerEventLayer, int globalLayer)
        {
            IWorldState worldState = Environment.i.world.state;

            if (string.IsNullOrEmpty(worldState.currentSceneId))
                return;

            RaycastHitInfo raycastGlobalLayerHitInfo;
            Ray ray = GetRayFromCamera();

            // Raycast for global pointer events
            RaycastResultInfo raycastInfoGlobalLayer = raycastHandler.Raycast(ray, charCamera.farClipPlane, globalLayer, worldState.loadedScenes[worldState.currentSceneId]);
            raycastGlobalLayerHitInfo = raycastInfoGlobalLayer.hitInfo;

            if (pointerInputUpEvent != null)
            {
                // Raycast for pointer event components
                RaycastResultInfo raycastInfoPointerEventLayer = raycastHandler.Raycast(ray, charCamera.farClipPlane, pointerEventLayer, worldState.loadedScenes[worldState.currentSceneId]);

                bool isOnClickComponentBlocked = IsBlockingOnClick(raycastInfoPointerEventLayer.hitInfo, raycastGlobalLayerHitInfo);
                bool isSameEntityThatWasPressed = AreCollidersFromSameEntity(raycastInfoPointerEventLayer.hitInfo, lastPointerDownEventHitInfo);

                if (!isOnClickComponentBlocked && isSameEntityThatWasPressed && enablePointerEvent)
                {
                    pointerInputUpEvent.Report(buttonId, ray, raycastInfoPointerEventLayer.hitInfo.hit);
                }

                pointerInputUpEvent = null;
            }

            ReportGlobalPointerUpEvent(buttonId, useRaycast, raycastGlobalLayerHitInfo, raycastInfoGlobalLayer, worldState.currentSceneId);

            // Raycast for global pointer events (for each PE scene)
            List<string> currentPortableExperienceIds = WorldStateUtils.GetActivePortableExperienceIds();
            for (int i = 0; i < currentPortableExperienceIds.Count; i++)
            {
                raycastInfoGlobalLayer = raycastHandler.Raycast(ray, charCamera.farClipPlane, globalLayer, worldState.loadedScenes[currentPortableExperienceIds[i]]);
                raycastGlobalLayerHitInfo = raycastInfoGlobalLayer.hitInfo;

                ReportGlobalPointerUpEvent(buttonId, useRaycast, raycastGlobalLayerHitInfo, raycastInfoGlobalLayer, currentPortableExperienceIds[i]);
            }
        }

        private void ProcessButtonDown(WebInterface.ACTION_BUTTON buttonId, bool useRaycast, bool enablePointerEvent, LayerMask pointerEventLayer, int globalLayer)
        {
            IWorldState worldState = Environment.i.world.state;

            if (string.IsNullOrEmpty(worldState.currentSceneId))
                return;

            RaycastHitInfo raycastGlobalLayerHitInfo;
            Ray ray = GetRayFromCamera();

            // Raycast for pointer event components
            RaycastResultInfo raycastInfoPointerEventLayer = raycastHandler.Raycast(ray, charCamera.farClipPlane, pointerEventLayer, worldState.loadedScenes[worldState.currentSceneId]);

            // Raycast for global pointer events
            RaycastResultInfo raycastInfoGlobalLayer = raycastHandler.Raycast(ray, charCamera.farClipPlane, globalLayer, worldState.loadedScenes[worldState.currentSceneId]);
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

                var events = hitGameObject.GetComponentsInChildren<IPointerInputEvent>();

                for (var i = 0; i < events.Length; i++)
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

            ReportGlobalPointerDownEvent(buttonId, useRaycast, raycastGlobalLayerHitInfo, raycastInfoGlobalLayer, worldState.currentSceneId);

            // Raycast for global pointer events (for each PE scene)
            List<string> currentPortableExperienceIds = WorldStateUtils.GetActivePortableExperienceIds();
            for (int i = 0; i < currentPortableExperienceIds.Count; i++)
            {
                raycastInfoGlobalLayer = raycastHandler.Raycast(ray, charCamera.farClipPlane, globalLayer, worldState.loadedScenes[currentPortableExperienceIds[i]]);
                raycastGlobalLayerHitInfo = raycastInfoGlobalLayer.hitInfo;

                ReportGlobalPointerDownEvent(buttonId, useRaycast, raycastGlobalLayerHitInfo, raycastInfoGlobalLayer, currentPortableExperienceIds[i]);
            }
        }

        private void ReportGlobalPointerUpEvent(
            WebInterface.ACTION_BUTTON buttonId,
            bool useRaycast,
            RaycastHitInfo raycastGlobalLayerHitInfo,
            RaycastResultInfo raycastInfoGlobalLayer,
            string sceneId)
        {
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

        private void ReportGlobalPointerDownEvent(
            WebInterface.ACTION_BUTTON buttonId,
            bool useRaycast,
            RaycastHitInfo raycastGlobalLayerHitInfo,
            RaycastResultInfo raycastInfoGlobalLayer,
            string sceneId)
        {
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

        bool AreSameEntity(IPointerEvent pointerInputEvent, ColliderInfo colliderInfo) { return pointerInputEvent != null && colliderInfo.entity != null && pointerInputEvent.entity == colliderInfo.entity; }

        bool IsBlockingOnClick(RaycastHitInfo targetOnClickHit, RaycastHitInfo potentialBlockerHit)
        {
            return
                potentialBlockerHit.hit.collider != null // Does a potential blocker hit exist?
                && targetOnClickHit.hit.collider != null // Was a target entity with a pointer event component hit?
                && potentialBlockerHit.hit.distance <= targetOnClickHit.hit.distance // Is potential blocker nearer than target entity?
                && !AreCollidersFromSameEntity(potentialBlockerHit, targetOnClickHit); // Does potential blocker belong to other entity rather than target entity?
        }

        bool EntityHasPointerEvent(IDCLEntity entity)
        {
            return entity.components.ContainsKey(Models.CLASS_ID_COMPONENT.UUID_CALLBACK) ||
                   entity.components.ContainsKey(Models.CLASS_ID_COMPONENT.UUID_ON_UP) ||
                   entity.components.ContainsKey(Models.CLASS_ID_COMPONENT.UUID_ON_DOWN) ||
                   entity.components.ContainsKey(Models.CLASS_ID_COMPONENT.UUID_ON_CLICK);
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
        
        private void HandleCursorLockChanges(bool isLocked)
        {
            HideOrShowCursor(isLocked);

            if (!isLocked)
                UnhoverLastHoveredObject();
        }

        private void HideOrShowCursor(bool isCursorLocked)
        {
            if (isCursorLocked)
                CursorController.i?.Show();
            else
                CursorController.i?.Hide();
        }
    }
}