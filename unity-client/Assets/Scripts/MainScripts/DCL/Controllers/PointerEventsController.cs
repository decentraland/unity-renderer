using DCL.Components;
using DCL.Helpers;
using DCL.Interface;
using DCL.Configuration;
using UnityEngine;

namespace DCL
{
    public class PointerEventsController : MonoBehaviour
    {
        public static PointerEventsController i { get; private set; }

        public static bool renderingIsDisabled = true;
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
        OnPointerEvent[] lastHoveredEvents = null;
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

        void Update()
        {
            if (RenderingController.i == null || !RenderingController.i.renderingEnabled || charCamera == null) return;

            // We use Physics.Raycast() instead of our raycastHandler.Raycast() as that one is slower, sometimes 2x, because it fetches info we don't need here
            if (Physics.Raycast(GetRayFromCamera(), out hitInfo, Mathf.Infinity, PhysicsLayers.physicsCastLayerMaskWithoutCharacter))
            {
                newHoveredEvent = hitInfo.transform.GetComponentInParent<OnPointerEvent>();

                if (newHoveredEvent != null && newHoveredEvent.IsAtHoverDistance((DCLCharacterController.i.transform.position - newHoveredEvent.transform.position).magnitude))
                {
                    newHoveredObject = newHoveredEvent.gameObject;

                    if (newHoveredObject != lastHoveredObject)
                    {
                        UnhoverLastHoveredObject();

                        lastHoveredObject = newHoveredObject;
                        lastHoveredEvents = newHoveredObject.GetComponents<OnPointerEvent>();

                        OnPointerHoverStarts?.Invoke();
                    }

                    // OnPointerDown/OnClick and OnPointerUp should display their hover feedback at different moments
                    if (lastHoveredEvents != null && lastHoveredEvents.Length > 0)
                    {
                        for (int i = 0; i < lastHoveredEvents.Length; i++)
                        {
                            bool eventButtonIsPressed = InputController_Legacy.i.IsPressed(lastHoveredEvents[i].GetActionButton());

                            if (lastHoveredEvents[i] is OnPointerUp && eventButtonIsPressed)
                                lastHoveredEvents[i].SetHoverState(true);
                            else if ((lastHoveredEvents[i] is OnPointerDown || lastHoveredEvents[i] is OnClick) && !eventButtonIsPressed)
                                lastHoveredEvents[i].SetHoverState(true);
                            else
                                lastHoveredEvents[i].SetHoverState(false);
                        }
                    }

                    newHoveredObject = null;
                    newHoveredEvent = null;
                }
                else
                {
                    UnhoverLastHoveredObject();
                }
            }
            else
            {
                UnhoverLastHoveredObject();
            }
        }

        void UnhoverLastHoveredObject()
        {
            if (lastHoveredObject == null) return;

            OnPointerHoverEnds?.Invoke();

            for (int i = 0; i < lastHoveredEvents.Length; i++)
            {
                lastHoveredEvents[i].SetHoverState(false);
            }

            lastHoveredEvents = null;
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
            lastHoveredEvents = null;
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
            if (Cursor.lockState != CursorLockMode.None && !renderingIsDisabled || this.isTesting)
            {
                if (charCamera == null)
                {
                    RetrieveCamera();

                    if (charCamera == null)
                        return;
                }

                var pointerEventLayer = PhysicsLayers.physicsCastLayerMaskWithoutCharacter; //Ensure characterController is being filtered
                var globalLayer = pointerEventLayer & ~PhysicsLayers.physicsCastLayerMask;
                RaycastHitInfo raycastGlobalLayerHitInfo;

                if (evt == InputController_Legacy.EVENT.BUTTON_DOWN)
                {
                    Ray ray;

                    ray = GetRayFromCamera();

                    // Raycast for pointer event components
                    RaycastResultInfo raycastInfoPointerEventLayer = raycastHandler.Raycast(ray, charCamera.farClipPlane, pointerEventLayer, null);

                    // Raycast for global pointer events
                    RaycastResultInfo raycastInfoGlobalLayer = raycastHandler.Raycast(ray, charCamera.farClipPlane, globalLayer, null);
                    raycastGlobalLayerHitInfo = raycastInfoGlobalLayer.hitInfo;

                    bool isOnClickComponentBlocked = IsBlockingOnClick(raycastInfoPointerEventLayer.hitInfo, raycastGlobalLayerHitInfo);

                    if (!isOnClickComponentBlocked && raycastInfoPointerEventLayer.hitInfo.hit.rigidbody)
                    {
                        GameObject go = raycastInfoPointerEventLayer.hitInfo.hit.rigidbody.gameObject;

                        go.GetComponentInChildren<OnClick>()?.Report(buttonId, raycastInfoPointerEventLayer.hitInfo.hit);
                        go.GetComponentInChildren<OnPointerDown>()?.Report(buttonId, ray, raycastInfoPointerEventLayer.hitInfo.hit);

                        pointerUpEvent = go.GetComponentInChildren<OnPointerUp>();
                        lastPointerDownEventHitInfo = raycastInfoPointerEventLayer.hitInfo;
                    }

                    string sceneId = SceneController.i.currentSceneId;

                    if (useRaycast && raycastGlobalLayerHitInfo.isValid)
                    {
                        DCL.Interface.WebInterface.ReportGlobalPointerDownEvent(
                            buttonId,
                            raycastInfoGlobalLayer.ray,
                            raycastGlobalLayerHitInfo.hit.point,
                            raycastGlobalLayerHitInfo.hit.normal,
                            raycastGlobalLayerHitInfo.hit.distance,
                            sceneId,
                            raycastGlobalLayerHitInfo.collider.entityId,
                            raycastGlobalLayerHitInfo.collider.meshName,
                            isHitInfoValid: true);
                    }
                    else
                    {
                        DCL.Interface.WebInterface.ReportGlobalPointerDownEvent(buttonId, raycastInfoGlobalLayer.ray, Vector3.zero, Vector3.zero, 0, sceneId);
                    }
                }
                else if (evt == InputController_Legacy.EVENT.BUTTON_UP)
                {
                    Ray ray;
                    ray = GetRayFromCamera();

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
                        DCL.Interface.WebInterface.ReportGlobalPointerUpEvent(
                            buttonId,
                            raycastInfoGlobalLayer.ray,
                            raycastGlobalLayerHitInfo.hit.point,
                            raycastGlobalLayerHitInfo.hit.normal,
                            raycastGlobalLayerHitInfo.hit.distance,
                            sceneId,
                            raycastGlobalLayerHitInfo.collider.entityId,
                            raycastGlobalLayerHitInfo.collider.meshName,
                            isHitInfoValid: true);
                    }
                    else
                    {
                        DCL.Interface.WebInterface.ReportGlobalPointerUpEvent(buttonId, raycastInfoGlobalLayer.ray, Vector3.zero, Vector3.zero, 0, sceneId);
                    }
                }
            }
        }

        bool IsBlockingOnClick(RaycastHitInfo targetOnClickHit, RaycastHitInfo potentialBlockerHit)
        {
            return
                potentialBlockerHit.hit.collider != null // Does a potential blocker hit exist?
                && targetOnClickHit.hit.collider != null // Was a target entity with a pointer event component hit?
                && potentialBlockerHit.hit.distance <= targetOnClickHit.hit.distance // Is potential blocker nearer than target entity?
                && !AreCollidersFromSameEntity(potentialBlockerHit, targetOnClickHit); // Does potential blocker belong to other entity rather than target entity?
        }

        bool AreCollidersFromSameEntity(RaycastHitInfo hitInfo, RaycastHitInfo otherHitInfo)
        {
            // If both entities has OnClick/PointerEvent component
            if (hitInfo.hit.rigidbody && otherHitInfo.hit.rigidbody)
            {
                return hitInfo.hit.rigidbody == otherHitInfo.hit.rigidbody;
            }
            // If only one of them has OnClick/PointerEvent component
            else if ((hitInfo.hit.rigidbody && !otherHitInfo.hit.rigidbody) || (!hitInfo.hit.rigidbody && otherHitInfo.hit.rigidbody))
            {
                return false;
            }
            // None of them has OnClick/PointerEvent component
            else
            {
                return hitInfo.collider.entityId == otherHitInfo.collider.entityId;
            }
        }
    }
}
