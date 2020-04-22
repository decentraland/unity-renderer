public class AvatarHUDController : IHUD
{
    private AvatarHUDView view;
    public AvatarHUDModel model { get; private set; }
    public bool visibility { get; private set; }
    public bool expanded { get; private set; }

    public event System.Action OnEditAvatarPressed;
    public event System.Action OnSettingsPressed;

    public void Initialize(bool visibility = true, bool expanded = false)
    {
        Initialize(new AvatarHUDModel(), visibility, expanded);
    }

    public void Initialize(AvatarHUDModel model, bool visibility = true, bool expanded = false)
    {
        view = AvatarHUDView.Create(this);
        UpdateData(model);
        SetVisibility(visibility);
        SetExpanded(expanded);
    }

    public void UpdateData(AvatarHUDModel model)
    {
        this.model = model;
        view.UpdateData(this.model);
    }

    public void SetVisibility(bool value)
    {
        visibility = value;
        view.SetVisibility(visibility);
    }

    public void SetExpanded(bool value)
    {
        expanded = value;
        view.SetExpanded(expanded);
    }

    public void ToggleExpanded()
    {
        SetExpanded(!expanded);
    }

    public void EditAvatar()
    {
        model.newWearables = 0;
        view.UpdateData(model);
        DCL.Interface.WebInterface.ReportEditAvatarClicked();
        OnEditAvatarPressed?.Invoke();
    }

    public void SignOut()
    {
        DCL.Interface.WebInterface.LogOut();
    }

    public void ShowSettings()
    {
        OnSettingsPressed?.Invoke();
    }

    public void SetConfiguration(HUDConfiguration configuration)
    {
        SetActive(configuration.active);
    }

    private void SetActive(bool active)
    {
        view.SetActive(active);
    }

    public void SetNewWearablesNotification(int wearableCount)
    {
        model.newWearables = wearableCount;
        view.UpdateData(model);
    }

    public void Dispose()
    {
    }
}
