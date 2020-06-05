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
    [SerializeField] internal Image avatarPic;
    internal InputAction_Trigger.Triggered openExpressionsDelegate;

    public static ExpressionsHUDView Create()
    {
        return Instantiate(Resources.Load<GameObject>(PATH)).GetComponent<ExpressionsHUDView>();
    }

    private void Awake()
    {
        openExpressionsDelegate = (x) =>
        {
            if (!IsVisible())
                return;
            if (Input.GetKeyDown(KeyCode.Escape) && !IsContentVisible())
                return;
            ToggleContent();
        };
        openExpressionsAction.OnTriggered += openExpressionsDelegate;
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

    public void UpdateAvatarSprite(Sprite avatarSprite)
    {
        if (avatarSprite == null) return;

        avatarPic.sprite = avatarSprite;
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
    }

    internal void HideContent()
    {
        content.gameObject.SetActive(false);
        DCL.Helpers.Utils.LockCursor();
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

    public void CleanUp()
    {
        openExpressionsAction.OnTriggered -= openExpressionsDelegate;

        for (int i = 0; i < hideContentButtons.Length; i++)
        {
            hideContentButtons[i].onPointerDown -= HideContent;
        }
    }
}
