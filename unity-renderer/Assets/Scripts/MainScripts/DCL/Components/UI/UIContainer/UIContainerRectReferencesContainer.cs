using UnityEngine;
using UnityEngine.UI;

namespace DCL.Components
{
    public class UIContainerRectReferencesContainer : UIReferencesContainer
    {
        [Header("UI SCREEN SPACE FIELDS")]
        public Image image;

        public UISizeFitter sizeFitter;

        [HideInInspector]
        public Outline optionalOutline;
    }
}
