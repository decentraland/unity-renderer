using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace DCL.EmotesWheel
{
    public class EmotesWheelView : MonoBehaviour
    {
        public class EmoteSlotData
        {
            public WearableItem emoteItem;
            public Sprite thumbnailSprite;
        }

        [Serializable]
        internal class RarityColor
        {
            public string rarity;
            public Color markColor;
        }

        private const string PATH = "EmotesWheelHUD";
      

        public event Action<string> onEmoteClicked;
        public event Action OnClose;
        public event Action OnCustomizeClicked;

        [SerializeField] internal Sprite nonAssignedEmoteSprite;
        [SerializeField] internal EmoteWheelSlot[] emoteButtons;
        [SerializeField] internal Button_OnPointerDown[] closeButtons;
        [SerializeField] internal ButtonComponentView openCustomizeButton;
        [SerializeField] internal TMP_Text selectedEmoteName;
        [SerializeField] internal List<RarityColor> rarityColors;
        [SerializeField] internal GameObject customizeTitle;

        private HUDCanvasCameraModeController hudCanvasCameraModeController;
        
        public static EmotesWheelView Create() { return Instantiate(Resources.Load<GameObject>(PATH)).GetComponent<EmotesWheelView>(); }

        private void Awake()
        {
            for (int i = 0; i < closeButtons.Length; i++)
            {
                closeButtons[i].onPointerDown += Close;
            }

            openCustomizeButton.onClick.AddListener(() =>
            {
                OnCustomizeClicked?.Invoke();
            });

            selectedEmoteName.text = string.Empty;
            hudCanvasCameraModeController = new HUDCanvasCameraModeController(GetComponent<Canvas>(), DataStore.i.camera.hudsCamera);
        }

        public void SetVisiblity(bool visible)
        {
            if (visible == gameObject.activeSelf)
                return;

            gameObject.SetActive(visible);
            if (visible)
            {
                AudioScriptableObjects.dialogOpen.Play(true);
            }
            else
            {
                AudioScriptableObjects.dialogClose.Play(true);
            }
        }

        public List<EmoteWheelSlot> SetEmotes(List<EmoteSlotData> emotes)
        {
            for (int i = 0; i < emotes.Count; i++)
            {
                EmoteSlotData equippedEmote = emotes[i];

                if (i < emoteButtons.Length)
                {
                    emoteButtons[i].button.onClick.RemoveAllListeners();
                    emoteButtons[i].onSlotHover -= OnSlotHover;
                    emoteButtons[i].onSlotHover += OnSlotHover;

                    if (equippedEmote != null)
                    {
                        emoteButtons[i].button.onClick.AddListener(() => onEmoteClicked?.Invoke(equippedEmote.emoteItem.id));
                        
                        if (equippedEmote.thumbnailSprite != null)
                            emoteButtons[i].SetImage(equippedEmote.thumbnailSprite);
                        else
                            emoteButtons[i].SetImage(equippedEmote.emoteItem.ComposeThumbnailUrl());

                        emoteButtons[i].SetId(equippedEmote.emoteItem.id);
                        emoteButtons[i].SetName(equippedEmote.emoteItem.GetName());
                        
                        RarityColor rarityColor = rarityColors.FirstOrDefault(x => x.rarity == equippedEmote.emoteItem.rarity);
                        emoteButtons[i].SetRarity(
                            rarityColor != null, 
                            rarityColor != null ? rarityColor.markColor : Color.white);
                    }
                    else
                    {
                        emoteButtons[i].SetImage(nonAssignedEmoteSprite);
                        emoteButtons[i].SetId(string.Empty);
                        emoteButtons[i].SetName(string.Empty);
                        emoteButtons[i].SetRarity(false, Color.white);
                    }
                }
            }

            return emoteButtons.ToList();
        }

        private void OnSlotHover(string emoteName) { selectedEmoteName.text = emoteName; }

        private void Close() { OnClose?.Invoke(); }

        public void OnDestroy()
        {
            CleanUp();
            hudCanvasCameraModeController?.Dispose();
        }

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