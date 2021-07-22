using TMPro;
using UnityEngine;

namespace AvatarNamesHUD
{
    public class AvatarNamesTracker
    {
        const float NAME_VANISHING_POINT_DISTANCE = 20.0f;
        private static Vector3 OFFSET = new Vector3(0, 3f, 0);

        private RectTransform canvasRect;
        private readonly RectTransform background;
        private readonly TextMeshProUGUI name;

        private PlayerStatus playerStatus;

        public AvatarNamesTracker(RectTransform canvasRect, RectTransform background, TextMeshProUGUI name)
        {
            this.canvasRect = canvasRect;
            this.background = background;
            this.name = name;
        }

        public void SetVisibility(bool visible)
        {
            background?.gameObject.SetActive(visible);
            name?.gameObject.SetActive(visible);
        }

        public void SetPlayerStatus(PlayerStatus newPlayerStatus)
        {
            playerStatus = newPlayerStatus;
            if (playerStatus == null)
                return;

            name.text = newPlayerStatus.name;
            name.ForceMeshUpdate(); //To get the new bounds
            background.sizeDelta = name.textBounds.size;
            UpdatePosition();
        }

        public void UpdatePosition()
        {
            if (playerStatus == null)
                return;

            var mainCamera = Camera.main;
            Vector3 screenPoint = mainCamera == null ? Vector3.zero : mainCamera.WorldToViewportPoint(playerStatus.worldPosition + OFFSET);

            //uiContainer.alpha = 1.0f + (1.0f - (screenPoint.z / NAME_VANISHING_POINT_DISTANCE));

            if (screenPoint.z > 0)
            {
                // if (!uiContainer.gameObject.activeSelf)
                // {
                //     uiContainer.gameObject.SetActive(true);
                // }

                float width = canvasRect.rect.width;
                float height = canvasRect.rect.height;
                screenPoint.Scale(new Vector3(width, height, 0));
                name.rectTransform.anchoredPosition = screenPoint;
                background.anchoredPosition = screenPoint;
                background.sizeDelta = new Vector2(name.textBounds.extents.x * 2.5f, 30);
            }
            else
            {
                // if (uiContainer.gameObject.activeSelf)
                // {
                //     uiContainer.gameObject.SetActive(false);
                // }
            }
        }

    }
}