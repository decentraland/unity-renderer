using UnityEngine;

public class AvatarHUDController : IHUD
{
    private AvatarHUDView view;
    public AvatarHUDModel model { get; private set; }
    public bool visibility { get; private set; }
    public bool expanded { get; private set; }

    public event System.Action OnEditAvatarPressed;

    public AvatarHUDController(bool visibility = true, bool expanded = false) : this(new AvatarHUDModel(), visibility, expanded) { }

    public AvatarHUDController(AvatarHUDModel model, bool visibility = true, bool expanded = false)
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
        OnEditAvatarPressed?.Invoke();
    }

    public void SignOut()
    {
        DCL.Interface.WebInterface.LogOut();
    }

    public void SetConfiguration(HUDConfiguration configuration)
    {
        SetActive(configuration.active);
    }

    private void SetActive(bool active)
    {
        view.SetActive(active);
    }
}