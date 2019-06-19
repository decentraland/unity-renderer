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

        [System.NonSerialized]
        public MouseCatcher mouseCatcher;

        [System.NonSerialized]
        public float inputDetectionPausedTime = 0;

        void Start()
        {
            mouseCatcher = FindObjectOfType<MouseCatcher>();

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

        void Update()
        {
            if(inputDetectionPausedTime > 0)
            {
                inputDetectionPausedTime -= Time.deltaTime;

                if(inputDetectionPausedTime > 0) return;
            }

            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (owner != null && owner.scene != null && owner.scene.isPersistent && !inputField.isFocused)
                {
                    inputField.Select();
                    mouseCatcher.UnlockCursor();
                }
            }
        }
    }
}