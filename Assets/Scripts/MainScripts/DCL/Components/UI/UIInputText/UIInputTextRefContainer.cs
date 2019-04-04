using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DCL.Components
{
    public class UIInputTextRefContainer : UIReferencesContainer
    {
        [Header("Input Text Fields")]
        public RawImage bgImage;
        public TMP_Text text;
        public TMP_InputField inputField;
        public LayoutElement textLayoutElement;
        public RectTransform textLayoutElementRT;
    }
}
