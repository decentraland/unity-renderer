using System.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DCL.Interface;
using UnityEngine.Serialization;

namespace DCL.Components
{
    public class UIReferencesContainer : MonoBehaviour, IPointerDownHandler
    {
        [System.NonSerialized] public UIShape owner;

        [Header("Basic Fields")]
        [Tooltip("This needs to always have the root RectTransform.")]
        public RectTransform rectTransform;

        public CanvasGroup canvasGroup;

        public HorizontalLayoutGroup layoutGroup;
        public LayoutElement layoutElement;
        public RectTransform layoutElementRT;

        [Tooltip("Children of this UI object will reparent to this rectTransform.")]
        public RectTransform childHookRectTransform;

        bool VERBOSE = false;

        private bool hasParentRectTransform = false;
        private RectTransform parentRectTransform;

        public RectTransform GetParentRectTransform()
        {
            if (!hasParentRectTransform)
            {
                parentRectTransform = GetComponentInParent<RectTransform>();
                hasParentRectTransform = true;
            }
            return parentRectTransform;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            UIShape.Model ownerModel = (UIShape.Model) owner.GetModel();

            if (VERBOSE)
            {
                Debug.Log("pointer current raycast: " + eventData.pointerCurrentRaycast,
                    eventData.pointerCurrentRaycast.gameObject);
                Debug.Log("pointer press raycast: " + eventData.pointerPressRaycast,
                    eventData.pointerPressRaycast.gameObject);
            }

            if (!string.IsNullOrEmpty(ownerModel.onClick) &&
                eventData.pointerPressRaycast.gameObject == childHookRectTransform.gameObject)
            {
                WebInterface.ReportOnClickEvent(owner.scene.sceneData.sceneNumber, ownerModel.onClick);
            }
        }

        public void SetVisibility(bool visible, float opacity = 1f)
        {
            if (canvasGroup == null) return;

            canvasGroup.alpha = visible ? opacity : 0;
        }

        public void SetBlockRaycast(bool isPointerBlocker)
        {
            if (canvasGroup == null) return;

            canvasGroup.blocksRaycasts = isPointerBlocker;
        }

        public void ResetLayoutElementRTLocalPosition()
        {
            if (layoutElementRT == null) return;

            layoutElementRT.localPosition = Vector3.zero;
        }
    }
}
