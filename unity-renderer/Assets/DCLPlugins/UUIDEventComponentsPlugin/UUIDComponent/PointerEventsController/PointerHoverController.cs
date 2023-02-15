using DCL.Components;
using DCL.Helpers;
using DCLPlugins.UUIDEventComponentsPlugin.UUIDComponent.Interfaces;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace DCL
{
    public class PointerHoverController
    {
        /// <summary>
        /// There is only one hovered object at a time
        /// but it contain listeners with different activation range
        /// </summary>
        private struct PointerEventInfo
        {
            /// <summary>
            /// The root object hit by raycast
            /// </summary>
            public GameObject GameObject;

            /// <summary>
            /// Listeners that were activated when they became within the range.
            /// </summary>
            public List<IPointerEvent> ActiveListeners;

            /// <summary>
            /// Listeners that were not active
            /// </summary>
            public List<IPointerEvent> InactiveListeners;
        }

        public event Action OnPointerHoverStarts;
        public event Action OnPointerHoverEnds;

        private PointerEventInfo currentHoveredObjectInfo = new ()
        {
            ActiveListeners = new List<IPointerEvent>(),
            InactiveListeners = new List<IPointerEvent>()
        };

        private readonly InputController_Legacy inputControllerLegacy;
        private readonly InteractionHoverCanvasController hoverCanvas;

        public bool IsObjectHovered => currentHoveredObjectInfo.GameObject;

        public PointerHoverController(InputController_Legacy inputControllerLegacy, InteractionHoverCanvasController hoverCanvas)
        {
            this.inputControllerLegacy = inputControllerLegacy;
            this.hoverCanvas = hoverCanvas;
        }

        public void OnRaycastHit(RaycastHit hit, ColliderInfo colliderInfo, GameObject targetObject,
            Type typeToUse)
        {
            if (targetObject == currentHoveredObjectInfo.GameObject)
            {
                ResolveActiveListeners(hit.distance, colliderInfo, typeToUse);

                if (currentHoveredObjectInfo.ActiveListeners.Count == 0)
                    ResetHoveredObject();
            }
            else
            {
                ResetHoveredObject();
                targetObject.GetComponentsInChildren(currentHoveredObjectInfo.InactiveListeners);
                ResolveActiveListeners(hit.distance, colliderInfo, typeToUse);

                // if the object was hovered but no listeners could accept it,
                // count it as it was not hovered at all
                if (currentHoveredObjectInfo.ActiveListeners.Count > 0)
                {
                    currentHoveredObjectInfo.GameObject = targetObject;
                    OnPointerHoverStarts?.Invoke();
                }
            }
        }

        private void ResolveActiveListeners(float distance, ColliderInfo colliderInfo, Type typeToUse)
        {
            var inactiveListenersCount = currentHoveredObjectInfo.InactiveListeners.Count;

            var activeListeners = ListPool<IPointerEvent>.Get();
            activeListeners.AddRange(currentHoveredObjectInfo.ActiveListeners);

            foreach (var activeListener in activeListeners)
            {
                if (!EventObjectCanBeHovered(typeToUse, activeListener, colliderInfo, distance))
                {
                    // Deactivate Listener
                    activeListener.SetHoverState(false);
                    currentHoveredObjectInfo.InactiveListeners.Add(activeListener);
                    currentHoveredObjectInfo.ActiveListeners.Remove(activeListener);
                }
            }

            ListPool<IPointerEvent>.Release(activeListeners);

            var index = 0;

            while (inactiveListenersCount > 0)
            {
                var inactiveListener = currentHoveredObjectInfo.InactiveListeners[index];

                if (EventObjectCanBeHovered(typeToUse, inactiveListener, colliderInfo, distance))
                {
                    currentHoveredObjectInfo.ActiveListeners.Add(inactiveListener);
                    currentHoveredObjectInfo.InactiveListeners.RemoveAt(index);
                }
                else index++;

                inactiveListenersCount--;
            }

            bool isEntityShowingHoverFeedback = false;

            // Maintain the previous logic to notify about hover state every frame
            foreach (IPointerEvent activeListener in currentHoveredObjectInfo.ActiveListeners)
                isEntityShowingHoverFeedback = SetHoverActive(activeListener, isEntityShowingHoverFeedback);
        }

        private bool SetHoverActive(IPointerEvent pointerEvent, bool isEntityShowingHoverFeedback)
        {
            // It's a copy of old logic, it's not entirely clear why it functions this way exactly

            // OnPointerDown/OnClick and OnPointerUp should display their hover feedback at different moments
            // If cursor is unlocked we ignore the button being pressed, avatars use case.
            if (pointerEvent is IPointerInputEvent e && Utils.IsCursorLocked)
            {
                bool eventButtonIsPressed = inputControllerLegacy.IsPressed(e.GetActionButton());

                switch (e.GetEventType())
                {
                    case PointerInputEventType.UP when eventButtonIsPressed:
                    case PointerInputEventType.CLICK when !eventButtonIsPressed:
                    case PointerInputEventType.DOWN when !eventButtonIsPressed:
                        e.SetHoverState(true);
                        return isEntityShowingHoverFeedback || e.ShouldShowHoverFeedback();
                }

                if (!isEntityShowingHoverFeedback)
                    e.SetHoverState(false);
            }
            else pointerEvent.SetHoverState(true);

            return isEntityShowingHoverFeedback;
        }

        private static bool EventObjectCanBeHovered(Type typeToUse, IPointerEvent pointerEvent, ColliderInfo colliderInfo, float distance) =>
            typeToUse.IsInstanceOfType(pointerEvent) &&
            pointerEvent.IsAtHoverDistance(distance) &&
            pointerEvent.IsVisible() &&
            AreSameEntity(pointerEvent, colliderInfo);

        private static bool AreSameEntity(IPointerEvent pointerInputEvent, ColliderInfo colliderInfo)
        {
            if (pointerInputEvent == null) return false;
            if (pointerInputEvent.entity == null && colliderInfo.entity == null) return true;
            return pointerInputEvent.entity == colliderInfo.entity;
        }

        public void ResetHoveredObject()
        {
            if (currentHoveredObjectInfo.GameObject != null)
            {
                foreach (var listener in currentHoveredObjectInfo.ActiveListeners)
                {
                    // Respect Unity GO lifecycle
                    if (Equals(listener, null))
                        continue;

                    listener.SetHoverState(false);
                }

                currentHoveredObjectInfo.GameObject = null;
                currentHoveredObjectInfo.ActiveListeners.Clear();
                currentHoveredObjectInfo.InactiveListeners.Clear();

                OnPointerHoverEnds?.Invoke();
            }

            if (hoverCanvas != null)
                hoverCanvas.SetHoverState(false);
        }
    }
}
