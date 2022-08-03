using DCL;
using DCL.Helpers;
using DCL.Interface;
using System;
using System.Collections;
using UnityEngine;
using Environment = DCL.Environment;
using WaitUntil = UnityEngine.WaitUntil;

public class ProfileHUDController : IHUD
{
    private readonly IUserProfileBridge userProfileBridge;

    [Serializable]
    public struct Configuration
    {
        public bool connectedWallet;
    }

    private const string URL_CLAIM_NAME = "https://builder.decentraland.org/claim-name";
    private const string URL_MANA_INFO = "https://docs.decentraland.org/examples/get-a-wallet";
    private const string URL_MANA_PURCHASE = "https://account.decentraland.org";
    private const string URL_TERMS_OF_USE = "https://decentraland.org/terms";
    private const string URL_PRIVACY_POLICY = "https://decentraland.org/privacy";
    private const float FETCH_MANA_INTERVAL = 60;

    public readonly ProfileHUDView view;
    internal AvatarEditorHUDController avatarEditorHud;

    private UserProfile ownUserProfile => UserProfile.GetOwnUserProfile();
    private IMouseCatcher mouseCatcher;
    private Coroutine fetchManaIntervalRoutine = null;
    private Coroutine fetchPolygonManaIntervalRoutine = null;

    public RectTransform tutorialTooltipReference { get => view.tutorialTooltipReference; }

    public event Action OnOpen;
    public event Action OnClose;

    public ProfileHUDController(IUserProfileBridge userProfileBridge)
    {
        this.userProfileBridge = userProfileBridge;
        mouseCatcher = SceneReferences.i?.mouseCatcher;


        view = UnityEngine.Object.Instantiate(GetViewPrefab()).GetComponent<ProfileHUDView>();
        view.name = "_ProfileHUD";

        CommonScriptableObjects.builderInWorldNotNecessaryUIVisibilityStatus.OnChange += ChangeVisibilityForBuilderInWorld;
        DataStore.i.exploreV2.profileCardIsOpen.OnChange += SetAsFullScreenMenuMode;

        view.connectedWalletSection.SetActive(false);
        view.nonConnectedWalletSection.SetActive(false);
        view.ActivateDescriptionEditionMode(false);

        view.buttonLogOut.onClick.AddListener(WebInterface.LogOut);
        view.buttonSignUp.onClick.AddListener(WebInterface.RedirectToSignUp);
        view.buttonClaimName.onClick.AddListener(() => WebInterface.OpenURL(URL_CLAIM_NAME));
        view.buttonTermsOfServiceForConnectedWallets.onPointerDown += () => WebInterface.OpenURL(URL_TERMS_OF_USE);
        view.buttonPrivacyPolicyForConnectedWallets.onPointerDown += () => WebInterface.OpenURL(URL_PRIVACY_POLICY);
        view.buttonTermsOfServiceForNonConnectedWallets.onPointerDown += () => WebInterface.OpenURL(URL_TERMS_OF_USE);
        view.buttonPrivacyPolicyForNonConnectedWallets.onPointerDown += () => WebInterface.OpenURL(URL_PRIVACY_POLICY);
        view.inputName.onSubmit.AddListener(UpdateProfileName);
        view.descriptionEditionInput.onSubmit.AddListener(UpdateProfileDescription);
        view.OnOpen += () =>
        {
            WebInterface.RequestOwnProfileUpdate();
            OnOpen?.Invoke();
        };
        view.OnClose += () => OnClose?.Invoke();

        if (view.manaCounterView)
        {
            view.manaCounterView.buttonManaInfo.onPointerDown += () => WebInterface.OpenURL(URL_MANA_INFO);
            view.manaCounterView.buttonManaPurchase.onClick.AddListener(() => WebInterface.OpenURL(URL_MANA_PURCHASE));
        }

        if (view.polygonManaCounterView)
        {
            view.polygonManaCounterView.buttonManaInfo.onPointerDown += () => WebInterface.OpenURL(URL_MANA_INFO);
            view.polygonManaCounterView.buttonManaPurchase.onClick.AddListener(() => WebInterface.OpenURL(URL_MANA_PURCHASE));
        }

        // ownUserProfile.OnUpdate += OnProfileUpdated;
        if (mouseCatcher != null)
            mouseCatcher.OnMouseLock += OnMouseLocked;

        if (!DCL.Configuration.EnvironmentSettings.RUNNING_TESTS)
        {
            KernelConfig.i.EnsureConfigInitialized().Then(config => OnKernelConfigChanged(config, null));
            KernelConfig.i.OnChange += OnKernelConfigChanged;
        }

        DataStore.i.exploreV2.isInitialized.OnChange += ExploreV2Changed;
        ExploreV2Changed(DataStore.i.exploreV2.isInitialized.Get(), false);
    }

    protected virtual GameObject GetViewPrefab()
    {
        return Resources.Load<GameObject>("ProfileHUD");
    }

    public void ChangeVisibilityForBuilderInWorld(bool current, bool previus) { view.gameObject.SetActive(current); }

    public void SetVisibility(bool visible)
    {
        view?.SetVisibility(visible);

        if (visible && fetchManaIntervalRoutine == null)
        {
            fetchManaIntervalRoutine = CoroutineStarter.Start(ManaIntervalRoutine());
        }
        else if (!visible && fetchManaIntervalRoutine != null)
        {
            CoroutineStarter.Stop(fetchManaIntervalRoutine);
            fetchManaIntervalRoutine = null;
        }

        if (visible && fetchPolygonManaIntervalRoutine == null)
        {
            fetchPolygonManaIntervalRoutine = CoroutineStarter.Start(PolygonManaIntervalRoutine());
        }
        else if (!visible && fetchPolygonManaIntervalRoutine != null)
        {
            CoroutineStarter.Stop(fetchPolygonManaIntervalRoutine);
            fetchPolygonManaIntervalRoutine = null;
        }
    }

    public void Dispose()
    {
        if (fetchManaIntervalRoutine != null)
        {
            CoroutineStarter.Stop(fetchManaIntervalRoutine);
            fetchManaIntervalRoutine = null;
        }

        if (view)
        {
            GameObject.Destroy(view.gameObject);
        }

        ownUserProfile.OnUpdate -= OnProfileUpdated;
        CommonScriptableObjects.builderInWorldNotNecessaryUIVisibilityStatus.OnChange -= ChangeVisibilityForBuilderInWorld;
        if (mouseCatcher != null)
            mouseCatcher.OnMouseLock -= OnMouseLocked;

        if (!DCL.Configuration.EnvironmentSettings.RUNNING_TESTS)
        {
            KernelConfig.i.OnChange -= OnKernelConfigChanged;
        }

        view.descriptionPreviewInput.onSubmit.RemoveListener(UpdateProfileDescription);
        DataStore.i.exploreV2.profileCardIsOpen.OnChange -= SetAsFullScreenMenuMode;

        DataStore.i.exploreV2.isInitialized.OnChange -= ExploreV2Changed;
    }

    void OnProfileUpdated(UserProfile profile) { view?.SetProfile(profile); }

    void OnMouseLocked() { HideProfileMenu(); }

    IEnumerator ManaIntervalRoutine()
    {
        while (true)
        {
            WebInterface.FetchBalanceOfMANA();
            yield return WaitForSecondsCache.Get(FETCH_MANA_INTERVAL);
        }
    }

    IEnumerator PolygonManaIntervalRoutine()
    {
        while (true)
        {
            yield return new WaitUntil(() => ownUserProfile != null && !string.IsNullOrEmpty(ownUserProfile.userId));

            Promise<double> promise = Environment.i.platform.serviceProviders.theGraph.QueryPolygonMana(ownUserProfile.userId);

            // This can be null if theGraph is mocked
            if ( promise != null )
            {
                yield return promise;
                SetPolygonManaBalance(promise.value);
            }

            yield return WaitForSecondsCache.Get(FETCH_MANA_INTERVAL);
        }
    }

    /// <summary>
    /// Set an amount of MANA on the HUD.
    /// </summary>
    /// <param name="balance">Amount of MANA.</param>
    public void SetManaBalance(string balance) { view.manaCounterView?.SetBalance(balance); }

    public void SetPolygonManaBalance(double balance) { view.polygonManaCounterView.SetBalance(balance); }

    /// <summary>
    /// Close the Profile menu.
    /// </summary>
    public void HideProfileMenu() { view?.HideMenu(); }

    private void UpdateProfileName(string newName)
    {
        if (view.inputName.wasCanceled)
            return;

        if (!view.IsValidAvatarName(newName))
        {
            view.inputName.ActivateInputField();
            return;
        }

        if (view != null)
        {
            view.SetProfileName(newName);
            view.ActivateProfileNameEditionMode(false);
        }

        userProfileBridge.SaveUnverifiedName(newName);
    }

    private void OnKernelConfigChanged(KernelConfigModel current, KernelConfigModel previous) { view?.SetNameRegex(current.profiles.nameValidRegex); }

    private void UpdateProfileDescription(string description)
    {
        if (view.descriptionEditionInput.wasCanceled
            || !ownUserProfile.hasConnectedWeb3
            || description.Length > view.descriptionEditionInput.characterLimit)
        {
            view.ActivateDescriptionEditionMode(false);
            return;
        }

        view.SetDescription(description);
        view.ActivateDescriptionEditionMode(false);
        userProfileBridge.SaveDescription(description);
    }

    private void SetAsFullScreenMenuMode(bool currentIsFullScreenMenuMode, bool previousIsFullScreenMenuMode)
    {
        view.SetCardAsFullScreenMenuMode(currentIsFullScreenMenuMode);

        if (currentIsFullScreenMenuMode != CommonScriptableObjects.isProfileHUDOpen.Get())
            view.ToggleMenu();
    }

    private void ExploreV2Changed(bool current, bool previous) { view.SetStartMenuButtonActive(current); }
}