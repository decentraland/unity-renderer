using DCL.Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Components
{
    public class UIScrollRectRefContainer : UIReferencesContainer
    {
        [Header("UIScrollRect Fields")]
        public LayoutGroup paddingLayoutGroup;
        public Scrollbar HScrollbar;
        public Scrollbar VScrollbar;
        public ScrollRect scrollRect;
        public RectTransform content;
        public RawImage contentBackground;

        [System.NonSerialized] public bool isDirty;
        [System.NonSerialized] public bool isHorizontal;
        [System.NonSerialized] public bool isVertical;

        private void LateUpdate()
        {
            if (isDirty)
            {
                isDirty = false;
                ComputeContentSize();
            }
        }

        public void EnsureMinContentSize()
        {
            RectTransform r = layoutElementRT;

            Vector2 contentSize = content.sizeDelta;

            if (contentSize.x < layoutElementRT.sizeDelta.x)
            {
                scrollRect.horizontal = false;
                contentSize.x = layoutElementRT.sizeDelta.x;
            }
            else
            {
                scrollRect.horizontal = isHorizontal;
            }

            if (contentSize.y < layoutElementRT.sizeDelta.y)
            {
                contentSize.y = layoutElementRT.sizeDelta.y;
                scrollRect.vertical = false;
            }
            else
            {
                scrollRect.vertical = isVertical;
            }

            content.sizeDelta = contentSize;
        }

        public void ComputeContentSize()
        {
            if (childHookRectTransform.childCount == 0)
            {
                content.sizeDelta = layoutElementRT.sizeDelta;
                scrollRect.horizontal = false;
                scrollRect.vertical = false;
                return;
            }

            Rect finalRect = new Rect();
            finalRect.xMax = float.MinValue;
            finalRect.yMax = float.MinValue;
            finalRect.xMin = float.MaxValue;
            finalRect.yMin = float.MaxValue;

            Vector3[] corners = new Vector3[4];

            RectTransform rt = childHookRectTransform as RectTransform;

            foreach (UIReferencesContainer rc in childHookRectTransform.GetComponentsInChildren<UIReferencesContainer>(true))
            {
                if (rc == childHookRectTransform)
                    continue;

                RectTransform r = rc.childHookRectTransform;
                r.GetWorldCorners(corners);

                for (int i = 0; i < corners.Length; i++)
                {
                    if (corners[i].x < finalRect.xMin)
                        finalRect.xMin = corners[i].x;

                    if (corners[i].x > finalRect.xMax)
                        finalRect.xMax = corners[i].x;

                    if (corners[i].y < finalRect.yMin)
                        finalRect.yMin = corners[i].y;

                    if (corners[i].y > finalRect.yMax)
                        finalRect.yMax = corners[i].y;
                }
            }

            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = new Vector2(finalRect.width / content.lossyScale.x, finalRect.height / content.lossyScale.y);

            EnsureMinContentSize();
        }

    }
}
