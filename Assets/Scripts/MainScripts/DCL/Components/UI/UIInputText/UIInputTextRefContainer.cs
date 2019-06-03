using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace DCL.Components
{
    public class UIInputTextRefContainer : UIReferencesContainer
    {
        [Header("Input Text Fields")]
        public RawImage bgImage;

        public TMP_Text text;
        public TMP_InputField inputField;

        void Start()
        {
            inputField.onSelect.AddListener(OnSelect);
        }

        void OnSelect(string str)
        {
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
            RaycastResult raycastResult = new RaycastResult();
            raycastResult.gameObject = text.gameObject;
            pointerEventData.pointerPressRaycast = raycastResult;

            OnPointerDown(pointerEventData);
        }

    }
}