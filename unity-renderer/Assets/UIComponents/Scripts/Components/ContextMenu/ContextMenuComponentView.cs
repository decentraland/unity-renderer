using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UIComponents.ContextMenu
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class ContextMenuComponentView : BaseComponentView
    {
        [SerializeField] private bool hideWhenClickedOutsideArea = true;

        private readonly Vector3[] corners = new Vector3[4];

        private RectTransform rectTransform;

        public Transform[] HidingHierarchyTransforms { get; set; }

        public override void Awake()
        {
            base.Awake();

            rectTransform = GetComponent<RectTransform>();
        }

        public virtual void Update()
        {
            if (hideWhenClickedOutsideArea)
                HideIfClickedOutside();
        }

        public void Show(Vector2 position, bool stayInsideScreen = true, bool instant = false)
        {
            base.Show(instant);

            rectTransform.position = position;

            if (stayInsideScreen)
                ClampPositionIntoScreenBorders();
        }

        private void ClampPositionIntoScreenBorders()
        {
            rectTransform.GetWorldCorners(corners);

            var sizeDelta = rectTransform.sizeDelta;

            float minX = sizeDelta.x * 0.5f;
            float maxX = Screen.width - (sizeDelta.x * 0.5f);
            float minY = sizeDelta.y * 0.5f;
            float maxY = Screen.height - (sizeDelta.y * 0.5f);

            Vector3 newPosition = rectTransform.position;
            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
            newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);

            rectTransform.position = newPosition;
        }

        private void HideIfClickedOutside()
        {
            if (!Input.GetMouseButtonDown(0)) return;

            var pointerEventData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition,
            };

            var raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerEventData, raycastResults);

            if (raycastResults.All(result =>
                {
                    if (HidingHierarchyTransforms != null)
                    {
                        foreach (var t in HidingHierarchyTransforms)
                            if (t == result.gameObject.transform)
                                return false;

                        return true;
                    }

                    return result.gameObject.transform != rectTransform
                           && !result.gameObject.transform.IsChildOf(rectTransform);
                }))
            {
                Hide();
            }
        }
    }
}
