using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Contextual menu with different options about an user.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class UserContextMenu : MonoBehaviour
{
    const string BLOCK_BTN_BLOCK_TEXT = "Block";
    const string BLOCK_BTN_UNBLOCK_TEXT = "Unblock";

    public TextMeshProUGUI userName;
    public Button passportButton;
    public Button blockButton;
    public Button reportButton;
    public TextMeshProUGUI blockText;

    public bool isVisible => gameObject.activeSelf;

    public event System.Action OnShowMenu;
    public event System.Action<string> OnPassport;
    public event System.Action<string> OnReport;
    public event System.Action<string, bool> OnBlock;

    private RectTransform rectTransform;
    private string userId;
    private bool isBlocked;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        passportButton.onClick.AddListener(OnPassportButtonPressed);
        blockButton.onClick.AddListener(OnBlockUserButtonPressed);
        reportButton.onClick.AddListener(OnReportUserButtonPressed);
    }

    private void Update()
    {
        HideIfClickedOutside();
    }

    private void OnDestroy()
    {
        passportButton.onClick.RemoveListener(OnPassportButtonPressed);
        blockButton.onClick.RemoveListener(OnBlockUserButtonPressed);
        reportButton.onClick.RemoveListener(OnReportUserButtonPressed);
    }

    /// <summary>
    /// Configures the context menu with the needed imformation.
    /// </summary>
    /// <param name="userId">User Id</param>
    /// <param name="userName">User name</param>
    public void Initialize(string userId, string userName, bool isBlocked)
    {
        this.userId = userId;
        this.isBlocked = isBlocked;
        this.userName.text = userName;
        UpdateBlockButton();
    }

    /// <summary>
    /// Shows the context menu.
    /// </summary>
    public void Show()
    {
        gameObject.SetActive(true);
        UpdateBlockButton();
        OnShowMenu?.Invoke();
    }

    /// <summary>
    /// Hides the context menu.
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnPassportButtonPressed()
    {
        OnPassport?.Invoke(userId);
        Hide();
    }

    private void OnReportUserButtonPressed()
    {
        OnReport?.Invoke(userId);
        Hide();
    }

    private void OnBlockUserButtonPressed()
    {
        OnBlock?.Invoke(userId, !isBlocked);
        Hide();
    }

    private void UpdateBlockButton()
    {
        blockText.text = isBlocked ? BLOCK_BTN_UNBLOCK_TEXT : BLOCK_BTN_BLOCK_TEXT;
    }

    private void HideIfClickedOutside()
    {
        if (Input.GetMouseButtonDown(0) && 
            !RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition))
        {
            Hide();
        }
    }
}
