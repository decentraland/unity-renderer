using DCL;
using DCL.Helpers;
using DCL.Interface;
using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using Environment = DCL.Environment;
using WaitUntil = UnityEngine.WaitUntil;

public class ProfileHUDController : IHUD
{
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
    private const string VIEW_NAME = "_ProfileHUD";
    private const float FETCH_MANA_INTERVAL = 60;

    public readonly IProfileHUDView view;
    private readonly IUserProfileBridge userProfileBridge;

    public event Action OnOpen;
    public event Action OnClose;

    internal AvatarEditorHUDController avatarEditorHud;

    private UserProfile ownUserProfile => UserProfile.GetOwnUserProfile();
    private IMouseCatcher mouseCatcher;
    private Coroutine fetchManaIntervalRoutine = null;
    private Coroutine fetchPolygonManaIntervalRoutine = null;

    private Regex nameRegex = null;


    public RectTransform TutorialTooltipReference => view.TutorialReference;


    public ProfileHUDController(IUserProfileBridge userProfileBridge)
    {
        this.userProfileBridge = userProfileBridge;
        mouseCatcher = SceneReferences.i?.mouseCatcher;

        GameObject viewGo = UnityEngine.Object.Instantiate(GetViewPrefab());
        viewGo.name = VIEW_NAME;
        view = viewGo.GetComponent<IProfileHUDView>();

        CommonScriptableObjects.builderInWorldNotNecessaryUIVisibilityStatus.OnChange += ChangeVisibilityForBuilderInWorld;
        DataStore.i.exploreV2.isOpen.OnChange += SetAsFullScreenMenuMode;
        DataStore.i.exploreV2.profileCardIsOpen.OnChange += SetProfileCardExtended;

        view.SetWalletSectionEnabled(false);
        view.SetNonWalletSectionEnabled(false);
        view.SetDescriptionIsEditing(false);

        view.LogedOutPressed += (object sender, EventArgs args) => WebInterface.LogOut();
        view.SignedUpPressed += (object sender, EventArgs args) => WebInterface.RedirectToSignUp();
        view.ClaimNamePressed += (object sender, EventArgs args) => WebInterface.OpenURL(URL_CLAIM_NAME);

        view.Opened += (object sender, EventArgs args) =>
        {
            WebInterface.RequestOwnProfileUpdate();
            OnOpen?.Invoke();
        };
        view.Closed += (object sender, EventArgs args) => OnClose?.Invoke();
        view.NameSubmitted += (object sender, string name) => UpdateProfileName(name);
        view.DescriptionSubmitted += (object sender, string description) => UpdateProfileDescription(description);

        view.TermsAndServicesPressed += (object sender, EventArgs args) => WebInterface.OpenURL(URL_TERMS_OF_USE);
        view.PrivacyPolicyPressed += (object sender, EventArgs args) => WebInterface.OpenURL(URL_PRIVACY_POLICY);

        if (view.HasManaCounterView() || view.HasPolygonManaCounterView())
        {
            view.ManaInfoPressed += (object sender, EventArgs args) => WebInterface.OpenURL(URL_MANA_INFO);
            view.ManaPurchasePressed += (object sender, EventArgs args) => WebInterface.OpenURL(URL_MANA_PURCHASE);
        }

        ownUserProfile.OnUpdate += OnProfileUpdated;

        if (!DCL.Configuration.EnvironmentSettings.RUNNING_TESTS)
        {
            KernelConfig.i.EnsureConfigInitialized().Then(config => OnKernelConfigChanged(config, null));
            KernelConfig.i.OnChange += OnKernelConfigChanged;
        }

        DataStore.i.exploreV2.isInitialized.OnChange += ExploreV2Changed;
        ExploreV2Changed(DataStore.i.exploreV2.isInitialized.Get(), false);
    }

    private void SetProfileCardExtended(bool isOpenCurrent, bool previous)
    {
        view.ShowExpanded(isOpenCurrent);
    }

    public void ChangeVisibilityForBuilderInWorld(bool current, bool previus) => view.GameObject.SetActive(current);

    public void SetManaBalance(string balance) => view?.SetManaBalance(balance);

    public void SetPolygonManaBalance(double balance) => view?.SetPolygonBalance(balance);

    public void SetVisibility(bool visible)
    {
        if (visible && fetchManaIntervalRoutine == null)
            fetchManaIntervalRoutine = CoroutineStarter.Start(ManaIntervalRoutine());
        else if (!visible && fetchManaIntervalRoutine != null)
        {
            CoroutineStarter.Stop(fetchManaIntervalRoutine);
            fetchManaIntervalRoutine = null;
        }

        if (visible && fetchPolygonManaIntervalRoutine == null)
            fetchPolygonManaIntervalRoutine = CoroutineStarter.Start(PolygonManaIntervalRoutine());
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

        if (view.GameObject)
            UnityEngine.Object.Destroy(view.GameObject);

        ownUserProfile.OnUpdate -= OnProfileUpdated;
        CommonScriptableObjects.builderInWorldNotNecessaryUIVisibilityStatus.OnChange -= ChangeVisibilityForBuilderInWorld;

        if (!DCL.Configuration.EnvironmentSettings.RUNNING_TESTS)
            KernelConfig.i.OnChange -= OnKernelConfigChanged;

        DataStore.i.exploreV2.profileCardIsOpen.OnChange -= SetAsFullScreenMenuMode;
        DataStore.i.exploreV2.isInitialized.OnChange -= ExploreV2Changed;
    }


    protected virtual GameObject GetViewPrefab()
    {
        return Resources.Load<GameObject>(DataStore.i.HUDs.enableNewPassport.Get() ? "ProfileHUD_V2" : "ProfileHUD");
    }


    private void OnProfileUpdated(UserProfile profile) => view?.SetProfile(profile);

    private void ExploreV2Changed(bool current, bool previous) => view.SetStartMenuButtonActive(current);

    private void OnKernelConfigChanged(KernelConfigModel current, KernelConfigModel previous) =>
        nameRegex = new Regex(current.profiles.nameValidRegex);

    private void UpdateProfileName(string newName)
    {
        if (nameRegex != null && !nameRegex.IsMatch(newName))
            return;

        userProfileBridge.SaveUnverifiedName(newName);
    }

    private void UpdateProfileDescription(string description)
    {
        if (!ownUserProfile.hasConnectedWeb3 || view.IsDesciptionIsLongerThanMaxCharacters())
        {
            view.SetDescriptionIsEditing(false);
            return;
        }

        userProfileBridge.SaveDescription(description);
    }

    private void SetAsFullScreenMenuMode(bool currentIsFullScreenMenuMode, bool previousIsFullScreenMenuMode)
    {
        view.ShowProfileIcon(!currentIsFullScreenMenuMode);
    }


    private IEnumerator ManaIntervalRoutine()
    {
        while (true)
        {
            WebInterface.FetchBalanceOfMANA();
            yield return WaitForSecondsCache.Get(FETCH_MANA_INTERVAL);
        }
    }

    private IEnumerator PolygonManaIntervalRoutine()
    {
        while (true)
        {
            yield return new WaitUntil(() => ownUserProfile != null && !string.IsNullOrEmpty(ownUserProfile.userId));

            Promise<double> promise = Environment.i.platform.serviceProviders.theGraph.QueryPolygonMana(ownUserProfile.userId);

            // This can be null if theGraph is mocked
            if (promise != null)
            {
                yield return promise;
                SetPolygonManaBalance(promise.value);
            }

            yield return WaitForSecondsCache.Get(FETCH_MANA_INTERVAL);
        }
    }
}
