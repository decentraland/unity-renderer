using DCL;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

internal class ExploreFriendsView : MonoBehaviour
{
    [SerializeField] RawImage friendPortrait;
    [SerializeField] Image friendBackground;
    [SerializeField] ShowHideAnimator showHideAnimator;
    [SerializeField] TextMeshProUGUI friendName;
    [SerializeField] UIHoverCallback hoverCallback;

    UserProfile userProfile;

    public void SetUserProfile(UserProfile profile, Color backgroundColor)
    {
        userProfile = profile;
        friendName.text = profile.userName;
        friendBackground.color = backgroundColor;
        userProfile.snapshotObserver.AddListener(OnFaceSnapshotLoaded);
        gameObject.SetActive(true);
    }

    void OnFaceSnapshotLoaded(Texture2D texture)
    {
        friendPortrait.texture = texture;
    }

    void OnHeadHoverEnter()
    {
        if (!userProfile)
            return;

        if (!showHideAnimator.gameObject.activeSelf)
            showHideAnimator.gameObject.SetActive(true);

        showHideAnimator.Show();
    }

    void OnHeadHoverExit() { showHideAnimator.Hide(); }

    void Awake()
    {
        hoverCallback.OnPointerEnter += OnHeadHoverEnter;
        hoverCallback.OnPointerExit += OnHeadHoverExit;
        showHideAnimator.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (userProfile != null)
            userProfile.snapshotObserver.RemoveListener(OnFaceSnapshotLoaded);

        if (hoverCallback)
        {
            hoverCallback.OnPointerEnter -= OnHeadHoverEnter;
            hoverCallback.OnPointerExit -= OnHeadHoverExit;
        }
    }
}