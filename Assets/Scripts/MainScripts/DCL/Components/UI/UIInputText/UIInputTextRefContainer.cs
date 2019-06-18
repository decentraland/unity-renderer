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

        //Workaround to focus the chat when pressing enter. This should be deleted once the chat scene gets refactored
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (owner != null && owner.scene != null && owner.scene.isPersistent && !inputField.isFocused)
                {
                    inputField.Select();
                    FindObjectOfType<MouseCatcher>().UnlockCursor();
                }
            }
        }
    }
}