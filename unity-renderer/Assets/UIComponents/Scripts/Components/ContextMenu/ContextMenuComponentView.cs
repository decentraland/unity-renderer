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

        public void ClampPositionToScreenBorders(Vector2 position)
        {
            rectTransform.position = position;
            rectTransform.GetWorldCorners(corners);

            Bounds bounds = new Bounds();
            bounds.SetMinMax(corners[1], corners[3]);

            Vector3 offset = Vector3.zero;

            offset.x -= Mathf.Min(0f, bounds.min.x);
            offset.y -= Mathf.Min(0f, bounds.max.y);
            offset.x -= Mathf.Max(0f, bounds.max.x - Screen.width);
            offset.y -= Mathf.Max(0f, bounds.min.y - Screen.height);

            rectTransform.position += offset;
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
