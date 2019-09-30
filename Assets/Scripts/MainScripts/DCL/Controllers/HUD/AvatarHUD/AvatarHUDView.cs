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


    [SerializeField] private Sprite defaultSprite;

    [Header("Minimized UI")]
    [SerializeField] private Image topAvatarPic;

    [SerializeField] private Button toggleExpandButton;

    [Header("User Info")]
    [SerializeField] private GameObject expandedContainer;

    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI mailText;

    [Header("Edit Avatar")]
    [SerializeField] private Button editAvatarButton;

    [Header("Sign Out")]
    [SerializeField] private Button signOutButton;

    private AvatarHUDController controller;
    
    private void Initialize(AvatarHUDController controller)
    {
        gameObject.name = VIEW_OBJECT_NAME;
        toggleExpandButton.onClick.AddListener(controller.ToggleExpanded);
        editAvatarButton.onClick.AddListener(controller.EditAvatar);
        signOutButton.onClick.AddListener(controller.SignOut);
    }

    internal static AvatarHUDView Create(AvatarHUDController controller)
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<AvatarHUDView>();
        view.Initialize(controller);
        return view;
    }

    internal void UpdateData(AvatarHUDModel model)
    {
        topAvatarPic.sprite = model.avatarPic ?? defaultSprite;
        nameText.text = model.name;
        mailText.text = model.mail;
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