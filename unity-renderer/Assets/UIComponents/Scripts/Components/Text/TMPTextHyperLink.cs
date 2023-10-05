using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UIComponents.Scripts.Components.Text
{
    public class TMPTextHyperLink : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private TMP_Text tmpTextBox;
        [SerializeField] private Canvas canvasToCheck;

        private Camera cameraToUse;

        public event Action HyperLinkClicked;

        private void Awake()
        {
            cameraToUse = canvasToCheck.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvasToCheck.worldCamera;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Vector3 mousePosition = new Vector3(eventData.position.x, eventData.position.y, 0); // or Input.mousePosition;

            if (TMP_TextUtilities.FindIntersectingLink(tmpTextBox, mousePosition, cameraToUse) != -1)
                HyperLinkClicked?.Invoke();
        }
    }
}
