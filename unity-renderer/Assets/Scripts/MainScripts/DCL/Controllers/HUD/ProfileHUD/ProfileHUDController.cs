using DCL;
using UnityEngine;
using DCL.Interface;

public class ProfileHUDController : IHUD
{
    private const string URL_CLAIM_NAME = "https://avatars.decentraland.org/claim";

    internal ProfileHUDView view;

    private UserProfile ownUserProfile => UserProfile.GetOwnUserProfile();
    private IMouseCatcher mouseCatcher;

    public ProfileHUDController()
    {
        mouseCatcher = InitialSceneReferences.i?.mouseCatcher;

        view = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("ProfileHUD")).GetComponent<ProfileHUDView>();
        view.name = "_ProfileHUD";

        view.buttonLogOut.onClick.AddListener(WebInterface.LogOut);
        view.buttonClaimName.onClick.AddListener(()=> WebInterface.OpenURL(URL_CLAIM_NAME));

        ownUserProfile.OnUpdate += OnProfileUpdated;
        if (mouseCatcher != null) mouseCatcher.OnMouseLock += OnMouseLocked;
    }

    public void SetVisibility(bool visible)
    {
        view?.SetVisibility(visible);
    }

    public void Dispose()
    {
        if (view)
        {
            Object.Destroy(view.gameObject);
        }
        ownUserProfile.OnUpdate -= OnProfileUpdated;
        if (mouseCatcher != null) mouseCatcher.OnMouseLock -= OnMouseLocked;
    }

    void OnProfileUpdated(UserProfile profile)
    {
        view?.SetProfile(profile);
    }

    void OnMouseLocked()
    {
        view.HideMenu();
    }
}
