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
        public UISizeFitter fitter;

        [System.NonSerialized] public bool isHorizontal;
        [System.NonSerialized] public bool isVertical;
    }
}