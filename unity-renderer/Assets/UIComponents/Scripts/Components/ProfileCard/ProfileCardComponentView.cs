using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IProfileCardComponentView
{
    /// <summary>
    /// Event that will be triggered when the profile card is clicked.
    /// </summary>
    Button.ButtonClickedEvent onClick { get; }

    /// <summary>
    /// Set the profile picture directly from a sprite.
    /// </summary>
    /// <param name="sprite">Profile picture (sprite).</param>
    void SetProfilePicture(Sprite sprite);

    /// <summary>
    /// Set the profile picture from a 2D texture.
    /// </summary>
    /// <param name="newPicture">Profile picture (2D texture).</param>
    void SetProfilePicture(Texture2D newPicture);

    /// <summary>
    /// Set the profile picture from an uri.
    /// </summary>
    /// <param name="uri">Profile picture (url).</param>
    void SetProfilePicture(string uri);

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
    /// Set the profile name.
    /// </summary>
    /// <param name="newName">Profile name.</param>
    void SetIsClaimedName(bool isClaimedName);

    /// <summary>
    /// Active or deactive the loading indicator.
    /// </summary>
    /// <param name="isVisible">True for showing the loading indicator.</param>
    void SetLoadingIndicatorVisible(bool isVisible);
}

public class ProfileCardComponentView : BaseComponentView, IProfileCardComponentView, IComponentModelConfig<ProfileCardComponentModel>
{
    [Header("Prefab References")]
    [SerializeField] internal Button button;
    [SerializeField] internal ImageComponentView profileImage;
    [SerializeField] internal TMP_Text profileName;
    [SerializeField] internal TMP_Text profileAddress;

    [Header("Configuration")]
    [SerializeField] internal ProfileCardComponentModel model;

    public Button.ButtonClickedEvent onClick => button?.onClick;

    public override void Start()
    {
        if (profileImage != null)
            profileImage.OnLoaded += OnProfileImageLoaded;
    }

    public void Configure(ProfileCardComponentModel newModel)
    {
        model = newModel;
        RefreshControl();
    }

    public override void RefreshControl()
    {
        if (model == null)
            return;

        if (model.profilePictureSprite != null)
            SetProfilePicture(model.profilePictureSprite);
        else if (model.profilePictureTexture != null)
            SetProfilePicture(model.profilePictureTexture);
        else if (!string.IsNullOrEmpty(model.profilePictureUri))
            SetProfilePicture(model.profilePictureUri);
        else
            SetProfilePicture(sprite: null);

        SetProfileName(model.profileName);
        SetProfileAddress(model.profileAddress);
        SetIsClaimedName(model.isClaimedName);
    }

    public override void Dispose()
    {
        base.Dispose();

        if (profileImage != null)
            profileImage.OnLoaded += OnProfileImageLoaded;
    }

    public void SetProfilePicture(Sprite sprite)
    {
        model.profilePictureSprite = sprite;

        if (profileImage == null)
            return;

        profileImage.SetImage(sprite);
    }

    public void SetProfilePicture(Texture2D texture)
    {
        model.profilePictureTexture = texture;

        if (!Application.isPlaying)
            return;

        if (profileImage == null)
            return;

        profileImage.SetImage(texture);
    }

    public void SetProfilePicture(string uri)
    {
        model.profilePictureUri = uri;

        if (!Application.isPlaying)
            return;

        if (profileImage == null)
            return;

        profileImage.SetImage(uri);
    }

    public void SetProfileName(string newName)
    {
        model.profileName = newName;

        if (profileName == null)
            return;

        profileName.text = !string.IsNullOrEmpty(newName) ? (model.isClaimedName ? newName : newName.Split('#')[0]) : string.Empty;
    }

    public void SetProfileAddress(string newAddress)
    {
        model.profileAddress = newAddress;

        if (profileAddress == null)
            return;

        if (!string.IsNullOrEmpty(newAddress))
            profileAddress.text = newAddress.Length >= 4 ? $"#{newAddress.Substring(newAddress.Length - 4, 4)}" : $"#{newAddress}";
        else
            profileAddress.text = string.Empty;
    }

    public void SetIsClaimedName(bool isClaimedName)
    {
        model.isClaimedName = isClaimedName;

        if (profileName != null)
            profileName.alignment = isClaimedName ? TextAlignmentOptions.Center : TextAlignmentOptions.Right;

        if (profileAddress != null)
            profileAddress.gameObject.SetActive(!isClaimedName);
    }

    public void SetLoadingIndicatorVisible(bool isVisible) { profileImage.SetLoadingIndicatorVisible(isVisible); }

    internal void OnProfileImageLoaded(Sprite sprite) { SetProfilePicture(sprite); }
}