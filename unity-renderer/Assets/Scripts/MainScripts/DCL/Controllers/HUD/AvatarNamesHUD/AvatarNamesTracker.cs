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

        internal readonly RectTransform canvasRect;
        internal readonly Image background;
        internal readonly CanvasGroup backgroundCanvasGroup;
        internal readonly TextMeshProUGUI name;
        internal readonly CanvasGroup voiceChatCanvasGroup;
        internal readonly Animator voiceChatAnimator;

        internal PlayerStatus playerStatus;
        internal bool visibility = false;

        private static Camera mainCamera = null;

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
            visibility = visible;
            background.gameObject.SetActive(visibility);
            name.gameObject.SetActive(visibility);
            voiceChatCanvasGroup.gameObject.SetActive(visibility && (playerStatus?.isTalking ?? false));
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

        public void UpdatePosition()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            if (playerStatus == null || mainCamera == null)
                return;

            Vector3 screenPoint = mainCamera == null ? Vector3.zero : mainCamera.WorldToViewportPoint(playerStatus.worldPosition + OFFSET);
            float alpha = screenPoint.z < 0 ? 0 : 1.0f + (1.0f - (screenPoint.z / NAME_VANISHING_POINT_DISTANCE));

            if (screenPoint.z > 0)
            {
                screenPoint.Scale(canvasRect.rect.size);
                name.rectTransform.anchoredPosition = screenPoint;
                background.rectTransform.anchoredPosition = screenPoint;
                background.rectTransform.sizeDelta = new Vector2(name.textBounds.extents.x * 2.5f, 30);
                Vector2 voiceChatOffset = -Vector2.Scale(Vector2.right, background.rectTransform.sizeDelta) * 0.5f;
                (voiceChatCanvasGroup.transform as RectTransform).anchoredPosition = background.rectTransform.anchoredPosition + voiceChatOffset;

                voiceChatCanvasGroup.alpha = alpha;
                name.color = new Color(name.color.r, name.color.g, name.color.b, alpha);
                backgroundCanvasGroup.alpha = alpha;
                voiceChatCanvasGroup?.gameObject.SetActive(visibility && (playerStatus?.isTalking ?? false));
                voiceChatAnimator.SetBool(VOICE_CHAT_ANIMATOR_TALKING, playerStatus.isTalking);

                background.gameObject.SetActive(visibility);
                name.gameObject.SetActive(visibility);
                voiceChatCanvasGroup.gameObject.SetActive(visibility && (playerStatus?.isTalking ?? false));
            }
            else
            {
                background.gameObject.SetActive(false);
                name.gameObject.SetActive(false);
                voiceChatCanvasGroup.gameObject.SetActive(false);
            }
        }

        public void DestroyUIElements()
        {
            GameObject.Destroy(background.gameObject);
            Object.Destroy(name.gameObject);
            Object.Destroy(voiceChatCanvasGroup.gameObject);
        }
    }
}