using DCL.Interface;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Components
{
    public class UIImageReferencesContainer : UIReferencesContainer
    {
        [Header("UI Image Fields")]
        public HorizontalLayoutGroup paddingLayoutGroup;

        public RawImage image;
        public RectTransform imageRectTransform;
    }
}