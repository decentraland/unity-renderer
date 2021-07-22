using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AvatarNamesHUD
{
    public class AvatarNamesTracker
    {
        private const float NAME_VANISHING_POINT_DISTANCE = 20.0f;
        private readonly int VOICE_CHAT_ANIMATOR_TALKING = Animator.StringToHash("Talking");
        private static Vector3 OFFSET = new Vector3(0, 3f, 0);

        private RectTransform canvasRect;
        private readonly Image background;
        private readonly TextMeshProUGUI name;
        private readonly Animator voiceChat;

        private PlayerStatus playerStatus;

        public AvatarNamesTracker(RectTransform canvasRect, Image background, TextMeshProUGUI name, Animator voiceChat)
        {
            this.canvasRect = canvasRect;
            this.background = background;
            this.name = name;
            this.voiceChat = voiceChat;
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
            background.rectTransform.sizeDelta = name.textBounds.size;
            if (playerStatus.isTalking && !voiceChat.gameObject.activeSelf)
            {
                voiceChat.gameObject.SetActive(playerStatus.isTalking);
            }
            voiceChat.SetBool(VOICE_CHAT_ANIMATOR_TALKING, playerStatus.isTalking);
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
                background.rectTransform.anchoredPosition = screenPoint;
                background.rectTransform.sizeDelta = new Vector2(name.textBounds.extents.x * 2.5f, 30);
                (voiceChat.transform as RectTransform).anchoredPosition = screenPoint - new Vector3(65, 0, 0);
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