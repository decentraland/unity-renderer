using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

[assembly: InternalsVisibleTo("AvatarHUDTests")]

public class AvatarHUDView : MonoBehaviour
{
    private const string VIEW_PATH = "Prefabs/AvatarHUD";
    private const string VIEW_OBJECT_NAME = "_AvatarHUD";


    [SerializeField] private GameObject loadingAvatar;

    [Header("Minimized UI")]
    [SerializeField] private RawImage topAvatarPic;

    [SerializeField] private Button toggleExpandButton;

    [Header("User Info")]
    [SerializeField] private GameObject expandedContainer;

    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI mailText;

    [Header("Edit Avatar")]
    [SerializeField] private Button editAvatarButton;

    [SerializeField] private GameObject newWearableNotification;
    [SerializeField] private TextMeshProUGUI newWearableNotificationText;

    [Header("Sign Out")]
    [SerializeField] private Button signOutButton;

    private AvatarHUDController controller;

    private void Initialize(AvatarHUDController controller)
    {
        gameObject.name = VIEW_OBJECT_NAME;

        toggleExpandButton.onClick.AddListener(controller.ToggleExpanded);

        editAvatarButton.onClick.AddListener(controller.EditAvatar);
        editAvatarButton.onClick.AddListener(controller.ToggleExpanded);

        signOutButton.onClick.AddListener(controller.SignOut);
        signOutButton.onClick.AddListener(controller.ToggleExpanded);
    }

    internal static AvatarHUDView Create(AvatarHUDController controller)
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<AvatarHUDView>();
        view.Initialize(controller);
        return view;
    }

    internal void UpdateData(AvatarHUDModel model)
    {
        topAvatarPic.texture = model.avatarPic;
        loadingAvatar.SetActive(topAvatarPic.texture == null);
        nameText.text = model.name;
        mailText.text = model.mail;
        newWearableNotificationText.text = model.newWearables.ToString();
        newWearableNotification.SetActive(model.newWearables != 0);
    }

    internal void SetVisibility(bool visibility)
    {
        gameObject.SetActive(visibility);
    }

    internal void SetExpanded(bool visibility)
    {
        expandedContainer.SetActive(visibility);
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }
}