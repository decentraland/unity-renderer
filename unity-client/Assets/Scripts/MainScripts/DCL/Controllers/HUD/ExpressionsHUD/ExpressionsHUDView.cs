using System;
using UnityEngine;
using UnityEngine.UI;

public class ExpressionsHUDView : MonoBehaviour
{
    private const string PATH = "ExpressionsHUD";

    public delegate void ExpressionClicked(string expressionId);

    [Serializable]
    public class ButtonToExpression
    {
        public string expressionId;
        public Button_OnPointerDown button; // When the button is used to lock/unlock the mouse we have to use onPointerDown
    }

    [SerializeField] internal ButtonToExpression[] buttonToExpressionMap;
    [SerializeField] internal Button showContentButton;
    [SerializeField] internal Button_OnPointerDown[] hideContentButtons;
    [SerializeField] internal RectTransform content;
    [SerializeField] internal InputAction_Trigger openExpressionsAction;
    [SerializeField] internal RawImage avatarPic;

    public static ExpressionsHUDView Create()
    {
        return Instantiate(Resources.Load<GameObject>(PATH)).GetComponent<ExpressionsHUDView>();
    }

    void OnOpenExpressions(DCLAction_Trigger trigger)
    {
        if (!IsVisible())
            return;

        if (Input.GetKeyDown(KeyCode.Escape) && !IsContentVisible())
            return;

        ToggleContent();
    }

    private void Awake()
    {
        openExpressionsAction.OnTriggered += OnOpenExpressions;
        showContentButton.onClick.AddListener(ToggleContent);

        for (int i = 0; i < hideContentButtons.Length; i++)
        {
            hideContentButtons[i].onPointerDown += HideContent;
        }
    }

    internal void Initialize(ExpressionClicked clickedDelegate)
    {
        content.gameObject.SetActive(false);

        foreach (var buttonToExpression in buttonToExpressionMap)
        {
            buttonToExpression.button.onPointerDown += () => clickedDelegate?.Invoke(buttonToExpression.expressionId);
        }
    }

    public void UpdateAvatarSprite(Texture2D avatarTexture)
    {
        if (avatarTexture == null) return;

        avatarPic.texture = avatarTexture;
    }

    internal void ToggleContent()
    {
        if (IsContentVisible())
        {
            HideContent();
        }
        else
        {
            ShowContent();
        }
    }

    internal void ShowContent()
    {
        content.gameObject.SetActive(true);
        DCL.Helpers.Utils.UnlockCursor();
        AudioScriptableObjects.dialogOpen.Play(true);
    }

    internal void HideContent()
    {
        content.gameObject.SetActive(false);
        DCL.Helpers.Utils.LockCursor();
        AudioScriptableObjects.dialogClose.Play(true);
    }

    public bool IsContentVisible()
    {
        return content.gameObject.activeSelf;
    }

    public void SetVisiblity(bool visible)
    {
        gameObject.SetActive(visible);
    }

    public bool IsVisible()
    {
        return gameObject.activeSelf;
    }

    public void OnDestroy()
    {
        CleanUp();
    }

    public void CleanUp()
    {
        openExpressionsAction.OnTriggered -= OnOpenExpressions;

        for (int i = 0; i < hideContentButtons.Length; i++)
        {
            hideContentButtons[i].onPointerDown -= HideContent;
        }
    }
}