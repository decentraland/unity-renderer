public interface IShortcutsController
{
    event System.Action OnCloseClick;

    void Initialize(IShortcutsView publishPopupView);
    void Dispose();
    void SetActive(bool isActive);
    void CloseButtonClicked();
}

public class ShortcutsController : IShortcutsController
{
    public event System.Action OnCloseClick;

    internal IShortcutsView publishPopupView;

    public void Initialize(IShortcutsView publishPopupView)
    {
        this.publishPopupView = publishPopupView;

        publishPopupView.OnCloseButtonClick += CloseButtonClicked;
    }

    public void Dispose()
    {
        publishPopupView.OnCloseButtonClick -= CloseButtonClicked;
    }

    public void SetActive(bool isActive)
    {
        publishPopupView.SetActive(isActive);
    }

    public void CloseButtonClicked()
    {
        OnCloseClick?.Invoke();
    }
}
