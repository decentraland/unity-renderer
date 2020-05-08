using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendsHUD_ContextMenu : MonoBehaviour
{
    const string BLOCK_BTN_BLOCK_TEXT = "Block";
    const string BLOCK_BTN_UNBLOCK_TEXT = "Unblock";

    public Button passportButton;
    public Button blockButton;
    public TextMeshProUGUI blockButtonText;
    public Button reportButton;
    public Button deleteButton;

    public event System.Action<FriendEntryBase> OnPassport;
    public event System.Action<FriendEntryBase> OnReport;
    public event System.Action<FriendEntryBase> OnBlock;
    public event System.Action<FriendEntryBase> OnDelete;

    public void Awake()
    {
        passportButton.onClick.AddListener(OnPassportButtonPressed);
        reportButton.onClick.AddListener(OnReportUserButtonPressed);
        deleteButton.onClick.AddListener(OnDeleteUserButtonPressed);
        blockButton.onClick.AddListener(OnBlockUserButtonPressed);
    }

    internal FriendEntryBase targetEntry { get; private set; }

    internal void Toggle(FriendEntryBase entry)
    {
        RectTransform rectTransform = transform as RectTransform;

        if (entry.transform.parent != null)
        {
            //NOTE(Pravus): By setting the pivot accordingly BEFORE we position the menu, we can have it always
            //              visible in an easier way.
            if (entry.transform.parent.InverseTransformPoint(entry.menuPositionReference.position).y < 0f)
                rectTransform.pivot = new Vector2(0.5f, 0f);
            else
                rectTransform.pivot = new Vector2(0.5f, 1f);
        }

        transform.position = entry.menuPositionReference.position;

        gameObject.SetActive(targetEntry == entry ? !gameObject.activeSelf : true);

        this.targetEntry = entry;

        if (gameObject.activeSelf)
            blockButtonText.text = entry.model.blocked ? BLOCK_BTN_UNBLOCK_TEXT : BLOCK_BTN_BLOCK_TEXT;
    }

    internal void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnBlockUserButtonPressed()
    {
        OnBlock?.Invoke(targetEntry);
        Hide();
    }

    private void OnDeleteUserButtonPressed()
    {
        OnDelete?.Invoke(targetEntry);
        Hide();
    }

    private void OnReportUserButtonPressed()
    {
        OnReport?.Invoke(targetEntry);
        Hide();
    }

    private void OnPassportButtonPressed()
    {
        OnPassport?.Invoke(targetEntry);
        Hide();
    }
}
