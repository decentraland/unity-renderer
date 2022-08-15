using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IFriendHeadForPlaceCardComponentView
{
    /// <summary>
    /// Set the background color for the friend.
    /// </summary>
    /// <param name="newText">New color.</param>
    void SetBackgroundColor(Color newColor);

    /// <summary>
    /// Set the user profile information.
    /// </summary>
    /// <param name="profile">User profile.</param>
    void SetUserProfile(UserProfile profile);
}

public class FriendHeadForPlaceCardComponentView : BaseComponentView, IFriendHeadForPlaceCardComponentView, IComponentModelConfig<FriendHeadForPlaceCardComponentModel>
{
    [Header("Prefab References")]
    [SerializeField] internal ImageComponentView friendPortrait;
    [SerializeField] internal Image friendBackground;
    [SerializeField] internal TextMeshProUGUI friendName;
    [SerializeField] internal ShowHideAnimator friendNameShowHideAnimator;

    [Header("Configuration")]
    [SerializeField] internal FriendHeadForPlaceCardComponentModel model;

    public override void Start() { friendNameShowHideAnimator.gameObject.SetActive(false); }

    public void Configure(FriendHeadForPlaceCardComponentModel newModel)
    {
        model = newModel;
        RefreshControl();
    }

    public override void RefreshControl()
    {
        if (model == null)
            return;

        SetBackgroundColor(model.backgroundColor);
        SetUserProfile(model.userProfile);
    }

    public override void OnFocus()
    {
        base.OnFocus();

        if (!model.userProfile)
            return;

        if (!friendNameShowHideAnimator.gameObject.activeSelf)
            friendNameShowHideAnimator.gameObject.SetActive(true);

        friendNameShowHideAnimator.Show();
    }

    public override void OnLoseFocus()
    {
        base.OnLoseFocus();

        friendNameShowHideAnimator.Hide();
    }

    public override void Dispose()
    {
        base.Dispose();

        if (model.userProfile != null)
            model.userProfile.snapshotObserver.RemoveListener(OnFaceSnapshotLoaded);

        if (friendPortrait != null)
            friendPortrait.Dispose();
    }

    public void SetBackgroundColor(Color newColor)
    {
        model.backgroundColor = newColor;

        if (friendBackground != null)
            friendBackground.color = newColor;
    }

    public void SetUserProfile(UserProfile profile)
    {
        model.userProfile = profile;

        if (model.userProfile != null)
        {
            friendName.text = model.userProfile.userName;
            model.userProfile.snapshotObserver.RemoveListener(OnFaceSnapshotLoaded);
            model.userProfile.snapshotObserver.AddListener(OnFaceSnapshotLoaded);
        }
        else
        {
            friendName.text = "";
        }
    }

    internal void OnFaceSnapshotLoaded(Texture2D texture)
    {
        if (friendPortrait == null)
            return;

        friendPortrait.SetImage(texture);
    }
}