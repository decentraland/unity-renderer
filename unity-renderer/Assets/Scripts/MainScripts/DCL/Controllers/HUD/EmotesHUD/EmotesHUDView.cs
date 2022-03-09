using DCL;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace EmotesCustomization
{
    public class EmotesHUDView : MonoBehaviour
    {
        [Serializable]
        internal class RarityColor
        {
            public string rarity;
            public Color markColor;
        }

        private const string PATH = "EmotesHUD";

        public event Action<string> onEmoteClicked;
        public event Action OnClose;
        public event Action OnCustomizeClicked;

        [SerializeField] internal Sprite nonAssignedEmoteSprite;
        [SerializeField] internal EmoteWheelSlot[] emoteButtons;
        [SerializeField] internal Button_OnPointerDown[] closeButtons;
        [SerializeField] internal ButtonComponentView openCustomizeButton;
        [SerializeField] internal TMP_Text selectedEmoteName;
        [SerializeField] internal List<RarityColor> rarityColors;

        public static EmotesHUDView Create() { return Instantiate(Resources.Load<GameObject>(PATH)).GetComponent<EmotesHUDView>(); }

        private void Awake()
        {
            for (int i = 0; i < closeButtons.Length; i++)
            {
                closeButtons[i].onPointerDown += Close;
            }

            openCustomizeButton.onClick.AddListener(() => OnCustomizeClicked?.Invoke());
        }

        public void SetVisiblity(bool visible)
        {
            if (visible == gameObject.activeSelf)
                return;

            gameObject.SetActive(visible);
            if (visible)
                AudioScriptableObjects.dialogOpen.Play(true);
            else
                AudioScriptableObjects.dialogClose.Play(true);
        }

        public void SetEmotes(List<WearableItem> emotes)
        {
            for (int i = 0; i < emotes.Count; i++)
            {
                WearableItem equippedEmote = emotes[i];

                if (i < emoteButtons.Length)
                {
                    emoteButtons[i].button.onClick.RemoveAllListeners();
                    emoteButtons[i].onSlotHover -= OnSlotHover;
                    emoteButtons[i].onSlotHover += OnSlotHover;

                    if (equippedEmote != null)
                    {
                        emoteButtons[i].button.onClick.AddListener(() => onEmoteClicked?.Invoke(equippedEmote.id));
                        emoteButtons[i].image.SetImage(equippedEmote.ComposeThumbnailUrl());
                        emoteButtons[i].SetName(equippedEmote.GetName());
                        
                        RarityColor rarityColor = rarityColors.FirstOrDefault(x => x.rarity == equippedEmote.rarity);
                        emoteButtons[i].SetRarity(
                            rarityColor != null, 
                            rarityColor != null ? rarityColor.markColor : Color.white);
                    }
                    else
                    {
                        emoteButtons[i].image.SetImage(nonAssignedEmoteSprite);
                        emoteButtons[i].SetName(string.Empty);
                        emoteButtons[i].SetRarity(false, Color.white);
                    }
                }
            }
        }

        private void OnSlotHover(string emoteName) { selectedEmoteName.text = emoteName; }

        private void Close() { OnClose?.Invoke(); }
        public void OnDestroy() { CleanUp(); }

        public void CleanUp()
        {
            for (int i = 0; i < closeButtons.Length; i++)
            {
                closeButtons[i].onPointerDown -= Close;
            }

            openCustomizeButton.onClick.RemoveAllListeners();
        }
    }
}