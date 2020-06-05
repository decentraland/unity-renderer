using DCL.Components;
using DCL.Configuration;
using DCL.Helpers;
using DCL.Interface;
using UnityEngine;

namespace DCL
{
    public class PointerEventsController : MonoBehaviour
    {
        public static PointerEventsController i { get; private set; }

        public InteractionHoverCanvasController interactionHoverCanvasController;

        private static bool renderingIsDisabled => !CommonScriptableObjects.rendererState.Get();
        public static System.Action OnPointerHoverStarts;
        public static System.Action OnPointerHoverEnds;

        bool isTesting = false;
        RaycastHitInfo lastPointerDownEventHitInfo;
        OnPointerUp pointerUpEvent;
        IRaycastHandler raycastHandler = new RaycastHandler();
        Camera charCamera;
        GameObject lastHoveredObject = null;
        GameObject newHoveredObject = null;
        OnPointerEvent newHoveredEvent = null;
        OnPointerEvent[] lastHoveredEventList = null;
        RaycastHit hitInfo;

        void Awake()
        {
            if (i != null)
            {
                Utils.SafeDestroy(this);

                return;
            }

            i = this;
        }

        public void Initialize(bool isTesting = false)
        {
            this.isTesting = isTesting;

            InputController_Legacy.i.AddListener(WebInterface.ACTION_BUTTON.POINTER, OnButtonEvent);
            InputController_Legacy.i.AddListener(WebInterface.ACTION_BUTTON.PRIMARY, OnButtonEvent);
            InputController_Legacy.i.AddListener(WebInterface.ACTION_BUTTON.SECONDARY, OnButtonEvent);

            RetrieveCamera();
        }

        private IRaycastPointerClickHandler clickHandler;

        void Update()
        {
            if (!CommonScriptableObjects.rendererState.Get() || charCamera == null) return;

            // We use Physics.Raycast() instead of our raycastHandler.Raycast() as that one is slower, sometimes 2x, because it fetches info we don't need here
            if (!Physics.Raycast(GetRayFromCamera(), out hitInfo, Mathf.Infinity, PhysicsLayers.physicsCastLayerMaskWithoutCharacter))
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
                newHoveredEvent = info.entity.gameObject.GetComponentInChildren<OnPointerEvent>();
            else
                newHoveredEvent = hitInfo.collider.GetComponentInChildren<OnPointerEvent>();

            clickHandler = null;

            if (!EventObjectCanBeHovered(newHoveredEvent, info))
            {
                UnhoverLastHoveredObject();
                return;
            }

            newHoveredObject = newHoveredEvent.gameObject;

            if (newHoveredObject != lastHoveredObject)
            {
                UnhoverLastHoveredObject();

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

        private bool EventObjectCanBeHovered(OnPointerEvent targetEvent, ColliderInfo colliderInfo)
        {
            return newHoveredEvent != null && newHoveredEvent.IsAtHoverDistance(DCLCharacterController.i.transform) && (IsAvatarPointerEvent(newHoveredEvent) || (newHoveredEvent.IsVisible() && AreSameEntity(newHoveredEvent, colliderInfo)));
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

        void UnhoverLastHoveredObject()
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
            if (!this.isTesting)
            {
                if (Utils.LockedThisFrame())
                    return;

                if (!Utils.isCursorLocked || renderingIsDisabled)
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

            string sceneId = SceneController.i.currentSceneId;

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

            string sceneId = SceneController.i.currentSceneId;

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
