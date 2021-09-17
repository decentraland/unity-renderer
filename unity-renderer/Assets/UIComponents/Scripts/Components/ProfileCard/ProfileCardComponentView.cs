using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IProfileCardComponentView
{
    /// <summary>
    /// Event that will be triggered when the profile card is clicked.
    /// </summary>
    Button.ButtonClickedEvent onClick { get; set; }

    /// <summary>
    /// Fill the model and updates the profile card with this data.
    /// </summary>
    /// <param name="model">Data to configure the profile card.</param>
    void Configure(ProfileCardComponentModel model);

    /// <summary>
    /// Set the profile picture.
    /// </summary>
    /// <param name="newPicture">Profile picture.</param>
    void SetProfilePicture(Sprite newPicture);

    /// <summary>
    /// Set the profile name.
    /// </summary>
    /// <param name="newName">Profile name.</param>
    void SetProfileName(string newName);

    /// <summary>
    /// Set the profile address. It will only show the last 4 caracteres.
    /// </summary>
    /// <param name="newAddress">Profile address.</param>
    void SetProfileAddress(string newAddress);

    /// <summary>
    /// Active or deactive the loading indicator.
    /// </summary>
    /// <param name="isVisible">True for showing the loading indicator.</param>
    void SetLoadingIndicatorVisible(bool isVisible);
}

public class ProfileCardComponentView : BaseComponentView, IProfileCardComponentView
{
    [Header("Prefab References")]
    [SerializeField] private Button button;
    [SerializeField] private ImageComponentView profileImage;
    [SerializeField] private TMP_Text profileName;
    [SerializeField] private TMP_Text profileAddress;

    [Header("Configuration")]
    [SerializeField] protected ProfileCardComponentModel model;

    public Button.ButtonClickedEvent onClick
    {
        get { return button?.onClick; }
        set
        {
            model.onClickEvent = value;
            button?.onClick.RemoveAllListeners();
            button?.onClick.AddListener(() =>
            {
                value.Invoke();
            });
        }
    }

    public override void Initialize()
    {
        base.Initialize();
        Configure(model);
    }

    public void Configure(ProfileCardComponentModel model)
    {
        this.model = model;
        RefreshControl();
    }

    public override void RefreshControl()
    {
        if (model == null)
            return;

        SetProfilePicture(model.profilePicture);
        SetProfileName(model.profileName);
        SetProfileAddress(model.profileAddress);
        onClick = model.onClickEvent;
    }

    public override void Dispose()
    {
        if (button == null)
            return;

        button.onClick.RemoveAllListeners();
    }

    public void SetProfilePicture(Sprite newPicture)
    {
        model.profilePicture = newPicture;

        if (profileImage == null)
            return;

        profileImage.SetImage(newPicture);
    }

    public void SetProfileName(string newName)
    {
        model.profileName = newName;

        if (profileName == null)
            return;

        profileName.text = newName;
    }

    public void SetProfileAddress(string newAddress)
    {
        model.profileAddress = newAddress;

        if (profileAddress == null)
            return;

        profileAddress.text = newAddress.Length >= 4 ?  $"#{newAddress.Substring(newAddress.Length - 4, 4)}" : $"#{newAddress}";
    }

    public void SetLoadingIndicatorVisible(bool isVisible) { profileImage.SetLoadingIndicatorVisible(isVisible); }
}