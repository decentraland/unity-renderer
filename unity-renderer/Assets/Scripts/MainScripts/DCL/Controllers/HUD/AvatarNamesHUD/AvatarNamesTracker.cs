using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AvatarNamesHUD
{
    public class AvatarNamesTracker
    {
        private static readonly Vector3 OFFSET = new Vector3(0, 3f, 0);
        private static readonly int VOICE_CHAT_ANIMATOR_TALKING = Animator.StringToHash("Talking");
        private const float NAME_VANISHING_POINT_DISTANCE = 15.0f;

        private readonly RectTransform canvasRect;
        private readonly Image background;
        private readonly CanvasGroup backgroundCanvasGroup;
        private readonly TextMeshProUGUI name;
        private readonly CanvasGroup voiceChatCanvasGroup;
        private readonly Animator voiceChatAnimator;

        private PlayerStatus playerStatus;

        public AvatarNamesTracker(RectTransform canvasRect, RectTransform backgroundRect, RectTransform nameRect, RectTransform voiceChatRect)
        {
            this.canvasRect = canvasRect;
            backgroundCanvasGroup = backgroundRect.GetComponent<CanvasGroup>();
            background = backgroundRect.GetComponent<Image>();
            name = nameRect.GetComponent<TextMeshProUGUI>();
            voiceChatCanvasGroup = voiceChatRect.GetComponent<CanvasGroup>();
            voiceChatAnimator = voiceChatRect.GetComponent<Animator>();
        }

        public void SetVisibility(bool visible)
        {
            background?.gameObject.SetActive(visible);
            name?.gameObject.SetActive(visible);
            voiceChatCanvasGroup?.gameObject.SetActive(visible);
            if (visible)
                UpdateVoiceChat();
        }

        public void SetPlayerStatus(PlayerStatus newPlayerStatus)
        {
            playerStatus = newPlayerStatus;
            if (playerStatus == null)
                return;

            name.text = newPlayerStatus.name;
            name.ForceMeshUpdate(); //To get the new bounds
            UpdatePosition();
        }

        private void UpdateVoiceChat()
        {
            if (playerStatus == null)
                return;
            voiceChatAnimator.SetBool(VOICE_CHAT_ANIMATOR_TALKING, playerStatus.isTalking);
        }

        public void UpdatePosition()
        {
            if (playerStatus == null)
                return;

            Camera mainCamera = Camera.main;
            Vector3 screenPoint = mainCamera == null ? Vector3.zero : mainCamera.WorldToViewportPoint(playerStatus.worldPosition + OFFSET);
            float alpha = screenPoint.z < 0 ? 0 : 1.0f + (1.0f - (screenPoint.z / NAME_VANISHING_POINT_DISTANCE));
            screenPoint.Scale(canvasRect.rect.size);


            name.rectTransform.anchoredPosition = screenPoint;
            background.rectTransform.anchoredPosition = screenPoint;
            background.rectTransform.sizeDelta = new Vector2(name.textBounds.extents.x * 2.5f, 30);
            Vector2 voiceChatOffset = -Vector2.Scale(Vector2.right, background.rectTransform.sizeDelta) * 0.5f;
            (voiceChatCanvasGroup.transform as RectTransform).anchoredPosition = background.rectTransform.anchoredPosition + voiceChatOffset;

            voiceChatCanvasGroup.alpha = alpha;
            name.color = new Color(name.color.r, name.color.g, name.color.b, alpha);
            backgroundCanvasGroup.alpha = alpha;
        }
    }
}