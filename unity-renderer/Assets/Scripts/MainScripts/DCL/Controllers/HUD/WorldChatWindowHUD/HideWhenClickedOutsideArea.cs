using UnityEngine;

namespace DCL.Chat.HUD
{
    public class HideWhenClickedOutsideArea : MonoBehaviour
    {
        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = (RectTransform) transform;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0) &&
                !RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition))
            {
                gameObject.SetActive(false);
            }
        }
    }
}