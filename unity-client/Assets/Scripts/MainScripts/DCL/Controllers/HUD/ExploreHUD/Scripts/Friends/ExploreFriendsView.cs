using UnityEngine;
using UnityEngine.UI;
using TMPro;

internal class ExploreFriendsView : MonoBehaviour
{
    [SerializeField] Image friendPortrait;
    [SerializeField] Image friendBackground;
    [SerializeField] ShowHideAnimator showHideAnimator;
    [SerializeField] TextMeshProUGUI friendName;
    [SerializeField] UIHoverCallback hoverCallback;

    UserProfile userProfile;

    public void SetUserProfile(UserProfile profile, Color backgroundColor)
    {
        userProfile = profile;
        friendPortrait.sprite = profile.faceSnapshot;
        friendName.text = profile.userName;
        friendBackground.color = backgroundColor;

        if (profile.faceSnapshot == null)
        {
            profile.OnFaceSnapshotReadyEvent += OnFaceSnapshotReadyEvent;
        }
        gameObject.SetActive(true);
    }

    void OnFaceSnapshotReadyEvent(Sprite sprite)
    {
        userProfile.OnFaceSnapshotReadyEvent -= OnFaceSnapshotReadyEvent;
        friendPortrait.sprite = userProfile.faceSnapshot;
    }

    void OnHeadHoverEnter()
    {
        if (userProfile)
        {
            if (!showHideAnimator.gameObject.activeSelf)
            {
                showHideAnimator.gameObject.SetActive(true);
            }
            showHideAnimator.Show();
        }
    }

    void OnHeadHoverExit()
    {
        showHideAnimator.Hide();
    }

    void Awake()
    {
        hoverCallback.OnPointerEnter += OnHeadHoverEnter;
        hoverCallback.OnPointerExit += OnHeadHoverExit;
        showHideAnimator.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (userProfile)
        {
            userProfile.OnFaceSnapshotReadyEvent -= OnFaceSnapshotReadyEvent;
        }

        if (hoverCallback)
        {
            hoverCallback.OnPointerEnter -= OnHeadHoverEnter;
            hoverCallback.OnPointerExit -= OnHeadHoverExit;
        }
    }
}
