using TMPro;
using UnityEngine;

namespace DCL.Controllers.HUD.SettingsPanelHUD
{
    /// <summary>
    /// Adjust x position to be at the end of the text (with offset)
    /// </summary>
    public class PositionAdjusterByEndOfText : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private float offset = 25f;

        private RectTransform rectTransform;

        private float containerWidth;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            containerWidth = titleText.rectTransform.rect.width;
        }

        private void OnEnable()
        {
             SetPositionWithOffsetFromTextEnd();
        }

        private void SetPositionWithOffsetFromTextEnd()
        {
            Vector3 newPos = rectTransform.localPosition;
            newPos.x = titleText.preferredWidth - (containerWidth / 2) + offset;

            rectTransform.localPosition = newPos;
        }
    }
}
