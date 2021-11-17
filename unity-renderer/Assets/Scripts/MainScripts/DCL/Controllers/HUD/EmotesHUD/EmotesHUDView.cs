using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class EmotesHUDView : MonoBehaviour
{
    private const string PATH = "EmotesHUD";

    public delegate void ExpressionClicked(string expressionId);
    public event Action OnClose;

    [Serializable]
    public class ButtonToEmote
    {
        public string expressionId;
        public Button_OnPointerDown button; // When the button is used to lock/unlock the mouse we have to use onPointerDown
    }

    [SerializeField] internal ButtonToEmote[] buttonToEmotesMap;
    [SerializeField] internal Button_OnPointerDown[] closeButtons;
    [SerializeField] internal RawImage avatarPic;

    public static EmotesHUDView Create() { return Instantiate(Resources.Load<GameObject>(PATH)).GetComponent<EmotesHUDView>(); }

    private void Awake()
    {
        for (int i = 0; i < closeButtons.Length; i++)
        {
            closeButtons[i].onPointerDown += Close;
        }
    }

    internal void Initialize(ExpressionClicked clickedDelegate)
    {
        foreach (var buttonToExpression in buttonToEmotesMap)
        {
            buttonToExpression.button.onPointerDown += () => clickedDelegate?.Invoke(buttonToExpression.expressionId);
        }
    }

    public void UpdateAvatarSprite(Texture2D avatarTexture)
    {
        if (avatarTexture == null)
            return;

        avatarPic.texture = avatarTexture;
    }

    public void SetVisiblity(bool visible)
    {
        gameObject.SetActive(visible);
        if (visible)
            AudioScriptableObjects.dialogOpen.Play(true);
        else
            AudioScriptableObjects.dialogClose.Play(true);
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