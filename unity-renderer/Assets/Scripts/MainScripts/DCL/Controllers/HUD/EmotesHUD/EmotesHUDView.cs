using DCL;
using System;
using System.Collections.Generic;
using UnityEngine;

public class EmotesHUDView : MonoBehaviour
{
    private const string PATH = "EmotesHUD";

    public event Action<string> onEmoteClicked;
    public event Action OnClose;

    [Serializable]
    public class ButtonToEmote
    {
        public Button_OnPointerDown button;
        public ImageComponentView image;
    }

    [SerializeField] internal Sprite nonAssignedEmoteSprite;
    [SerializeField] internal ButtonToEmote[] emoteButtons;
    [SerializeField] internal Button_OnPointerDown[] closeButtons;

    public static EmotesHUDView Create() { return Instantiate(Resources.Load<GameObject>(PATH)).GetComponent<EmotesHUDView>(); }

    private void Awake()
    {
        for (int i = 0; i < closeButtons.Length; i++)
        {
            closeButtons[i].onPointerDown += Close;
        }
    }

    public void SetVisiblity(bool visible)
    {
        gameObject.SetActive(visible);
        if (visible)
            AudioScriptableObjects.dialogOpen.Play(true);
        else
            AudioScriptableObjects.dialogClose.Play(true);
    }

    public void SetEmotes(List<StoredEmote> emotes)
    {
        for (int i = 0; i < emotes.Count; i++)
        {
            StoredEmote equippedEmote = emotes[i];

            if (i < emoteButtons.Length)
            {
                emoteButtons[i].button.onClick.RemoveAllListeners();

                if (equippedEmote != null)
                {
                    emoteButtons[i].button.onClick.AddListener(() => onEmoteClicked?.Invoke(equippedEmote.id));
                    emoteButtons[i].image.SetImage(equippedEmote.pictureUri);
                }
                else
                {
                    emoteButtons[i].image.SetImage(nonAssignedEmoteSprite);
                }
            }
        }
    }

    private void Close() { OnClose?.Invoke(); }
    public void OnDestroy() { CleanUp(); }

    public void CleanUp()
    {
        for (int i = 0; i < closeButtons.Length; i++)
        {
            closeButtons[i].onPointerDown -= Close;
        }
    }
}