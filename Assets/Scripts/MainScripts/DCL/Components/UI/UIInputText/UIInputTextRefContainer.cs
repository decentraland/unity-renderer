using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Components
{
    public class UIInputTextRefContainer : UIReferencesContainer
    {
        [Header("Input Text Fields")]
        public RawImage bgImage;

        public TMP_Text text;
        public TMP_InputField inputField;
    }
}