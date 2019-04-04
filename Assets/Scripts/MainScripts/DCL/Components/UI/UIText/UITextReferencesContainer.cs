using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DCL.Components
{
    public class UITextReferencesContainer : UIReferencesContainer
    {
        [Header("UI Text Fields")]
        public TextMeshProUGUI text;
        public RectTransform textRectTransform;
        public LayoutElement alignedLayoutElement;
    }
}
